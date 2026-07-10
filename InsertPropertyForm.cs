using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DataImportTool
{
    public partial class InsertPropertyForm : Form
    {
        private readonly DbHelper _db;
        private CancellationTokenSource _cts;

        // 非控件字段（控件字段在 Designer.cs 中声明）
        private List<PropertyMetaInfo> _allMetaProperties;
        private List<PropertyItem> _existingProperties;
        private ToolTip _toolTip;

        /// <summary>当前分类下已有的属性 InternalName（用于在左侧列表中置灰并锁定）</summary>
        private HashSet<string> _existingInternalNames = new HashSet<string>();

        /// <summary>用户新勾选的属性 InternalName（仅新增项，不含已有项）</summary>
        private HashSet<string> _selectedInternalNames = new HashSet<string>();

        /// <summary>ApplyPropertyFilter / ApplyPropertyDisableState 执行期间阻止 ItemCheck 回调误触</summary>
        private bool _isApplyingFilter = false;

        // 无参构造函数：供 Visual Studio 设计器实例化窗体
        public InsertPropertyForm() : this(null)
        {
        }

        public InsertPropertyForm(DbHelper db)
        {
            _db = db;
            if (db != null)
            {
                _toolTip = new ToolTip();
                _toolTip.AutoPopDelay = 5000;
                _toolTip.InitialDelay = 200;
            }
            InitializeComponent();
            if (db != null) InitControls();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                LoadClassifications();
            }
        }

        private void InitControls()
        {
            // === 搜索框占位提示 ===
            _txtSearch.Text = "🔍 输入属性名称或拼音首字母进行模糊检索...";
            _txtSearch.ForeColor = Color.FromArgb(150, 150, 150);
            _txtSearch.Enter += (s, e) =>
            {
                if (_txtSearch.Text == "🔍 输入属性名称或拼音首字母进行模糊检索...")
                {
                    _txtSearch.Text = string.Empty;
                    _txtSearch.ForeColor = SystemColors.WindowText;
                }
            };
            _txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _txtSearch.Text = "🔍 输入属性名称或拼音首字母进行模糊检索...";
                    _txtSearch.ForeColor = Color.FromArgb(150, 150, 150);
                }
            };

            // === 搜索文本变化 ===
            _txtSearch.TextChanged += (s, e) => ApplyPropertyFilter(_txtSearch.Text);

            // === CheckedListBox 所有者绘制（用于置灰已有属性）===
            _clbProperties.DrawMode = DrawMode.OwnerDrawVariable;
            _clbProperties.DrawItem += ClbProperties_DrawItem;
            _clbProperties.MeasureItem += ClbProperties_MeasureItem;

            // === 模式切换（分类模式下选择分类后自动查询已有属性）===
            _rbClassification.CheckedChanged += OnModeChanged;
            _rbRoot.CheckedChanged += OnModeChanged;

            // === 初始展示已有属性面板的占位提示 ===
            ShowExistingPlaceholder("请选择分类以查看已有属性");

            // === 按钮事件 ===
            _btnLoad.Click += BtnLoad_Click;
            _btnImport.Click += BtnImport_Click;
            _btnCancel.Click += (s, ev) => this.Close();
            _btnClearAll.Click += (s, ev) =>
            {
                // 清空用户新勾选的属性（已有属性由中间面板的 X 管理，不受影响）
                _selectedInternalNames.Clear();
                for (int i = 0; i < _clbProperties.Items.Count; i++)
                {
                    var meta = _clbProperties.Items[i] as PropertyMetaInfo;
                    if (meta != null && !_existingInternalNames.Contains(meta.InternalName))
                        _clbProperties.SetItemChecked(i, false);
                }
                UpdateSearchCountLabel();
            };

            // === 全选（仅可选的新属性）===
            var btnSelectAll = this.Controls["btnSelectAll"] as Button;
            if (btnSelectAll != null)
                btnSelectAll.Click += (s, ev) =>
                {
                    for (int i = 0; i < _clbProperties.Items.Count; i++)
                    {
                        var meta = _clbProperties.Items[i] as PropertyMetaInfo;
                        if (meta == null) continue;
                        if (_existingInternalNames.Contains(meta.InternalName)) continue; // 已有属性锁定
                        _selectedInternalNames.Add(meta.InternalName);
                        _clbProperties.SetItemChecked(i, true);
                    }
                    UpdateSearchCountLabel();
                };

            // === 全不选 ===
            var btnSelectNone = this.Controls["btnSelectNone"] as Button;
            if (btnSelectNone != null)
                btnSelectNone.Click += (s, ev) =>
                {
                    for (int i = 0; i < _clbProperties.Items.Count; i++)
                    {
                        var meta = _clbProperties.Items[i] as PropertyMetaInfo;
                        if (meta != null && _existingInternalNames.Contains(meta.InternalName)) continue;
                        _clbProperties.SetItemChecked(i, false);
                    }
                    _selectedInternalNames.Clear();
                    UpdateSearchCountLabel();
                };

            // === 勾选变化 → 同步 _selectedInternalNames + 刷新预览 ===
            _clbProperties.ItemCheck += (s, ev) =>
            {
                // ApplyPropertyFilter / ApplyPropertyDisableState 重建列表期间忽略
                if (_isApplyingFilter) return;

                this.BeginInvoke(new Action(() =>
                {
                    if (_isApplyingFilter) return;
                    var meta = _clbProperties.Items[ev.Index] as PropertyMetaInfo;
                    if (meta == null) return;

                    bool isExisting = _existingInternalNames.Contains(meta.InternalName);
                    if (isExisting)
                    {
                        // 已有属性：锁定为始终勾选，禁止用户取消
                        if (!_clbProperties.GetItemChecked(ev.Index))
                            _clbProperties.SetItemChecked(ev.Index, true);
                        return;
                    }

                    if (ev.NewValue == CheckState.Checked)
                        _selectedInternalNames.Add(meta.InternalName);
                    else
                        _selectedInternalNames.Remove(meta.InternalName);

                    UpdateSearchCountLabel();
                }));
            };
        }

        // ====== 模式切换：分类模式自动查询所选分类的已有属性；根目录模式隐藏已有/预览面板 ======
        private void OnModeChanged(object sender, EventArgs e)
        {
            bool clsMode = _rbClassification.Checked;
            _cmbClassification.Enabled = clsMode;
            _lblSelectCls.Enabled = clsMode;
            _pnlExisting.Visible = true;    // 已有属性面板始终显示（根目录显示占位提示）
            _pnlSelected.Visible = true;    // 右侧预览面板始终显示（根目录=纯新增，分类=新增+已有）

            if (clsMode)
            {
                if (_cmbClassification.SelectedValue != null)
                    LoadExistingProperties(Convert.ToInt64(_cmbClassification.SelectedValue));
                else
                    ShowExistingPlaceholder("请选择分类以查看已有属性");
            }
            else
            {
                // 根目录模式：查询根级已有属性（CLS_ID = 0）
                LoadRootProperties();
            }
        }

        // ====== 下拉框选择分类后：查询该分类下已有的属性 ======
        private void _cmbClassification_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_rbClassification.Checked) return;
            if (_cmbClassification.SelectedItem == null)
            {
                ShowExistingPlaceholder("请选择分类以查看已有属性");
                return;
            }

            var selectedItem = _cmbClassification.SelectedItem as ClassificationGroup;
            if (selectedItem == null)
            {
                ShowExistingPlaceholder("请选择分类以查看已有属性");
                return;
            }
            long clsId = selectedItem.ObjectId;

            LoadExistingProperties(clsId);
        }

        // ====== 查询并刷新「该分类已有属性」面板 ======
        private void LoadExistingProperties(long clsId)
        {
            if (_db == null) return;
            try
            {
                var list = _db.GetPropertiesByClassificationId(clsId);
                _existingProperties = list;

                // 同步更新已有属性集合（用于在左侧列表中锁定对应项）
                _existingInternalNames.Clear();
                if (list != null)
                {
                    foreach (var p in list)
                        _existingInternalNames.Add(p.InternalName);
                }

                RefreshExistingPanel(list);

                // 如果属性列表已加载，立即锁定已有属性
                if (_allMetaProperties != null)
                    ApplyPropertyDisableState();

                // 已有属性默认进入预览
                RefreshPreviewPanel();
            }
            catch (Exception ex)
            {
                _lblExistingCount.Text = "查询失败";
                RefreshExistingPanel(new List<PropertyItem>());
                MessageBox.Show("查询该分类已有属性失败：" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====== 查询根目录已有属性（CLS_INTERNAL_NAME IS NULL AND CLS_ID = 0）======
        private void LoadRootProperties()
        {
            if (_db == null)
            {
                ShowExistingPlaceholder("（根目录模式：暂无已有属性）");
                return;
            }
            try
            {
                var list = _db.GetRootProperties();
                _existingProperties = list;

                // 同步更新已有属性集合（用于在左侧列表中锁定对应项）
                _existingInternalNames.Clear();
                if (list != null)
                {
                    foreach (var p in list)
                        _existingInternalNames.Add(p.InternalName);
                }

                RefreshExistingPanel(list);

                // 如果属性列表已加载，立即锁定已有属性
                if (_allMetaProperties != null)
                    ApplyPropertyDisableState();

                // 已有属性默认进入预览
                RefreshPreviewPanel();
            }
            catch (Exception ex)
            {
                _lblExistingCount.Text = "查询失败";
                RefreshExistingPanel(new List<PropertyItem>());
                MessageBox.Show("查询根目录已有属性失败：" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====== 渲染「该分类已有属性」列表（每行：属性名 + X 按钮；红底=待删除）=======
        private void RefreshExistingPanel(List<PropertyItem> items)
        {
            _existingProperties = items;
            int count = items != null ? items.Count : 0;
            int deletedCount = 0;
            if (items != null)
                deletedCount = items.Count(p => p.MarkDeleted);
            _lblExistingCount.Text = deletedCount > 0
                ? string.Format("{0} 项（{1} 待删除）", count, deletedCount)
                : string.Format("{0} 项", count);

            int listW = _pnlExistingList.ClientSize.Width - 4;
            if (listW <= 0) listW = 276; // 设计时 fallback

            _pnlExistingList.SuspendLayout();
            _pnlExistingList.Controls.Clear();

            if (count == 0)
            {
                var empty = new Label
                {
                    Text = "（该分类下暂无属性）",
                    Location = new Point(4, 6),
                    Size = new Size(listW, 16),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(108, 117, 125)
                };
                _pnlExistingList.Controls.Add(empty);
                _pnlExistingList.AutoScrollMinSize = new Size(1, 1);
                _pnlExistingList.ResumeLayout(true);
                return;
            }

            int rowH = 28;
            int btnW = 30;
            int y = 4;
            foreach (var item in items)
            {
                bool deleted = item.MarkDeleted;
                var row = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(listW, 24),
                    BackColor = deleted ? Color.FromArgb(248, 215, 218) : Color.FromArgb(232, 244, 253),
                    Tag = item
                };

                // 标签：显示属性名
                var lbl = new Label
                {
                    Text = TruncateText(item.Name, listW - btnW - 10),
                    Location = new Point(4, 4),
                    Size = new Size(listW - btnW - 14, 16),
                    AutoEllipsis = true,
                    Font = new Font("Segoe UI", 8F, deleted ? (FontStyle)(FontStyle.Bold | FontStyle.Strikeout) : FontStyle.Regular),
                    ForeColor = deleted ? Color.FromArgb(220, 53, 69) : Color.FromArgb(33, 37, 41),
                    Cursor = Cursors.Hand
                };
                if (_toolTip != null)
                    _toolTip.SetToolTip(lbl,
                        (deleted ? "[待删除] " : "[已有] ") + item.Name + "\n[" + item.InternalName + "]");

                // X 按钮：点击切换「待删除」标记状态
                var btn = new Button
                {
                    Text = "✕",
                    Location = new Point(listW - btnW, 2),
                    Size = new Size(btnW, 20),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = deleted ? Color.FromArgb(220, 53, 69) : Color.FromArgb(173, 181, 189),
                    BackColor = deleted ? Color.FromArgb(255, 235, 238) : Color.FromArgb(245, 247, 250),
                    Tag = item,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, ev) =>
                {
                    var btnSender = s as Button;
                    var p = btnSender?.Tag as PropertyItem;
                    if (p != null) ToggleDeleteExisting(p);
                };

                row.Controls.Add(lbl);
                row.Controls.Add(btn);
                _pnlExistingList.Controls.Add(row);
                y += rowH;
            }

            int totalH = y + 8;
            _pnlExistingList.AutoScrollMinSize = new Size(1, totalH);
            _pnlExistingList.ResumeLayout(true);
        }

        // ====== 切换已有属性的「待删除」状态 ======
        private void ToggleDeleteExisting(PropertyItem item)
        {
            if (item == null) return;
            item.MarkDeleted = !item.MarkDeleted;
            // 刷新「已有属性」面板（行背景色）
            RefreshExistingPanel(_existingProperties);
            // 刷新「本次插入完整属性预览」（实时增删）
            RefreshPreviewPanel();
        }

        /// <summary>
        /// 渲染「本次插入完整属性预览」面板：
        /// 最终属性 = 已有属性（未标记删除） + 用户新勾选的属性
        /// </summary>
        private void RefreshPreviewPanel()
        {
            if (_allMetaProperties == null) return;

            int listW = _pnlSelectedList.ClientSize.Width - 4;
            if (listW <= 0) listW = 300; // 设计时 fallback

            var rows = new List<PreviewRow>();
            // 已有属性（未被标记删除）
            if (_existingProperties != null)
            {
                foreach (var p in _existingProperties)
                {
                    if (!p.MarkDeleted)
                        rows.Add(new PreviewRow { Name = p.Name, InternalName = p.InternalName, IsNew = false });
                }
            }
            // 用户新勾选的属性（排除已有的，避免重复）
            foreach (var meta in _allMetaProperties)
            {
                if (_selectedInternalNames.Contains(meta.InternalName) &&
                    !_existingInternalNames.Contains(meta.InternalName))
                {
                    rows.Add(new PreviewRow { Name = meta.DisplayName, InternalName = meta.InternalName, IsNew = true });
                }
            }

            _pnlSelectedList.SuspendLayout();
            _pnlSelectedList.Controls.Clear();

            int newCount = rows.Count(r => r.IsNew);
            _lblSelectedCount.Text = string.Format("{0} 项（新增 {1}）", rows.Count, newCount);

            if (rows.Count == 0)
            {
                var empty = new Label
                {
                    Text = "（暂无属性）",
                    Location = new Point(4, 6),
                    Size = new Size(listW, 16),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(108, 117, 125)
                };
                _pnlSelectedList.Controls.Add(empty);
                _pnlSelectedList.AutoScrollMinSize = new Size(1, 1);
                _pnlSelectedList.ResumeLayout(true);
                return;
            }

            int rowH = 26;
            int y = 4;
            foreach (var r in rows)
            {
                var row = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(listW, 22),
                    BackColor = r.IsNew ? Color.FromArgb(232, 255, 240) : Color.FromArgb(248, 249, 250)
                };

                var lbl = new Label
                {
                    Text = TruncateText(r.Name, listW - 56),
                    Location = new Point(4, 4),
                    Size = new Size(listW - 56, 16),
                    AutoEllipsis = true,
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = r.IsNew ? Color.FromArgb(40, 167, 69) : Color.FromArgb(33, 37, 41)
                };
                if (_toolTip != null)
                    _toolTip.SetToolTip(lbl, (r.IsNew ? "[新增] " : "[已有] ") + r.Name + "\n[" + r.InternalName + "]");

                var tag = new Label
                {
                    Text = r.IsNew ? "新增" : "已有",
                    Location = new Point(listW - 50, 4),
                    Size = new Size(46, 16),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = r.IsNew ? Color.FromArgb(40, 167, 69) : Color.FromArgb(108, 117, 125),
                    TextAlign = ContentAlignment.MiddleRight
                };

                row.Controls.Add(lbl);
                row.Controls.Add(tag);
                _pnlSelectedList.Controls.Add(row);
                y += rowH;
            }

            int totalH = y + 8;
            _pnlSelectedList.AutoScrollMinSize = new Size(1, totalH);
            _pnlSelectedList.ResumeLayout(true);
        }

        /// <summary>
        /// 根据 _existingInternalNames 对 CheckedListBox 中已有的属性项锁定（灰显 + 强制勾选）；
        /// 新属性按 _selectedInternalNames 恢复勾选状态。
        /// </summary>
        private void ApplyPropertyDisableState()
        {
            if (_allMetaProperties == null) return;

            _isApplyingFilter = true;
            try
            {
                for (int i = 0; i < _clbProperties.Items.Count; i++)
                {
                    var meta = _clbProperties.Items[i] as PropertyMetaInfo;
                    if (meta == null) continue;

                    bool isExisting = _existingInternalNames.Contains(meta.InternalName);
                    if (isExisting)
                    {
                        // 已有属性：锁定为勾选状态（不可取消）
                        _clbProperties.SetItemCheckState(i, CheckState.Checked);
                    }
                    else
                    {
                        bool shouldCheck = _selectedInternalNames.Contains(meta.InternalName);
                        _clbProperties.SetItemChecked(i, shouldCheck);
                    }
                }
            }
            finally
            {
                _isApplyingFilter = false;
            }
        }

        // ====== CheckedListBox 所有者绘制：已有属性灰显并锁定 ======
        private const int ITEM_HEIGHT = 20;

        private void ClbProperties_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = ITEM_HEIGHT;
        }

        private void ClbProperties_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _clbProperties.Items.Count) return;

            var item = _clbProperties.Items[e.Index] as PropertyMetaInfo;
            bool isExisting = item != null && _existingInternalNames.Contains(item.InternalName);
            bool isNew = item != null && !isExisting && _selectedInternalNames.Contains(item.InternalName);
            bool isChecked = _clbProperties.GetItemChecked(e.Index);
            bool isSelected = (e.State & DrawItemState.Selected) != 0;

            // ---- 整行背景 ----
            Color bgColor;
            if (isExisting)
                bgColor = Color.FromArgb(245, 247, 250);  // 浅灰蓝（已有属性，锁定）
            else if (isNew)
                bgColor = Color.FromArgb(232, 255, 240);  // 浅绿色（本次新增）
            else if (isSelected)
                bgColor = SystemColors.Highlight;
            else
                bgColor = _clbProperties.BackColor;
            e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds);

            // ---- 复选框（宽14px） ----
            const int CB_SIZE = 14;
            const int CB_X = 4;
            int cbY = e.Bounds.Y + (ITEM_HEIGHT - CB_SIZE) / 2;
            var cbRect = new Rectangle(e.Bounds.X + CB_X, cbY, CB_SIZE, CB_SIZE);

            e.Graphics.FillRectangle(new SolidBrush(Color.White), cbRect);
            using (var borderPen = new Pen(Color.FromArgb(173, 181, 189), 1))
            {
                e.Graphics.DrawRectangle(borderPen, cbRect);
            }

            if (isChecked)
            {
                Color checkColor = isExisting
                    ? Color.FromArgb(173, 181, 189)   // 已有属性：灰色
                    : isNew
                        ? Color.FromArgb(40, 167, 69)  // 新增项：绿色
                        : SystemColors.WindowText;     // 正常：深色
                using (var checkPen = new Pen(checkColor, 2))
                {
                    var pts = new Point[] {
                        new Point(cbRect.X + 3,  cbRect.Y + 7),
                        new Point(cbRect.X + 6,  cbRect.Y + 10),
                        new Point(cbRect.X + 10, cbRect.Y + 3)
                    };
                    e.Graphics.DrawLines(checkPen, pts);
                }
            }

            // ---- 文字 ----
            Color textColor;
            if (isExisting)
                textColor = Color.FromArgb(108, 117, 125);  // 灰色（已有属性）
            else if (isNew)
                textColor = Color.FromArgb(40, 167, 69);    // 绿色（本次新增）
            else if (isSelected)
                textColor = Color.White;
            else
                textColor = _clbProperties.ForeColor;

            using (var textBrush = new SolidBrush(textColor))
            {
                int textX = e.Bounds.X + CB_X + CB_SIZE + 6;
                int textW = e.Bounds.Width - (textX - e.Bounds.X) - 4;
                var textRect = new Rectangle(textX, e.Bounds.Y, Math.Max(textW, 0), ITEM_HEIGHT);
                var sf = new StringFormat { LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(item?.DisplayName ?? _clbProperties.Items[e.Index]?.ToString() ?? string.Empty,
                    _clbProperties.Font, textBrush, textRect, sf);
            }

            e.DrawFocusRectangle();
        }

        // ====== 已有属性面板占位提示 ======
        private void ShowExistingPlaceholder(string msg)
        {
            _existingProperties = null;
            _existingInternalNames.Clear();
            _lblExistingCount.Text = "0 项";

            int listW = _pnlExistingList.ClientSize.Width - 4;
            if (listW <= 0) listW = 286;

            _pnlExistingList.SuspendLayout();
            _pnlExistingList.Controls.Clear();
            var empty = new Label
            {
                Text = msg,
                Location = new Point(4, 6),
                Size = new Size(listW, 16),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(108, 117, 125)
            };
            _pnlExistingList.Controls.Add(empty);
            _pnlExistingList.AutoScrollMinSize = new Size(1, 1);
            _pnlExistingList.ResumeLayout(true);

            RefreshPreviewPanel();
        }

        private void LoadClassifications()
        {
            try
            {
                var list = _db.GetAllClassifications();
                _cmbClassification.DataSource = list;
                _cmbClassification.DisplayMember = "Name";
                _cmbClassification.ValueMember = "ObjectId";
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载分类列表失败：" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                _btnLoad.Enabled = false;
                _clbProperties.Items.Clear();
                _lblStatus.Text = "正在加载元模型信息...";
                _lblStatus.Update();

                // 第1步：查询 ljzdysx 记录的 properties 数组
                var propNames = _db.GetPropertyNamesFromMetaElement();
                if (propNames.Count == 0)
                {
                    _lblStatus.Text = "未找到属性定义（ljzdysx meta_element）";
                    _btnLoad.Enabled = true;
                    return;
                }

                // 第2步：查询每个属性的 display_name.zh-cn
                _allMetaProperties = _db.GetPropertyMetaItems(propNames);

                _lblStatus.Text = $"已加载 {propNames.Count} 个属性，正在构建列表...";
                _lblStatus.Update();

                // 首次加载时应用当前搜索条件（空字符串=显示全部）
                ApplyPropertyFilter(_txtSearch.Text);

                // 分类模式下，锁定已有属性
                ApplyPropertyDisableState();

                _lblStatus.Text = $"已加载 {propNames.Count} 个属性，请选择要插入的属性";
                _btnLoad.Enabled = true;
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "加载失败：" + ex.Message;
                _btnLoad.Enabled = true;
                MessageBox.Show("加载属性失败：" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 根据搜索文本模糊过滤 CheckedListBox，
        /// 所有勾选状态由 _selectedInternalNames 全程维护
        /// </summary>
        private void ApplyPropertyFilter(string searchText)
        {
            if (_allMetaProperties == null) return;

            _isApplyingFilter = true;
            try
            {
                string norm = searchText.Trim().ToLower();
                if (norm.StartsWith("\ud83d\udd0d")) norm = string.Empty;

                IEnumerable<PropertyMetaInfo> filtered;
                if (string.IsNullOrEmpty(norm))
                {
                    filtered = _allMetaProperties;
                }
                else
                {
                    filtered = _allMetaProperties.Where(p =>
                        p.DisplayName.ToLower().Contains(norm) ||
                        p.InternalName.ToLower().Contains(norm) ||
                        GetPyFirstLetters(p.DisplayName).ToLower().Contains(norm)
                    );
                }

                _clbProperties.Items.Clear();
                foreach (var p in filtered)
                {
                    bool isChecked = _selectedInternalNames.Contains(p.InternalName);
                    _clbProperties.Items.Add(p, isChecked);
                }
            }
            finally
            {
                _isApplyingFilter = false;
            }

            UpdateSearchCountLabel();
        }

        private static string GetPyFirstLetters(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (c >= 0x4e00 && c <= 0x9fa5)
                    sb.Append(GetPyChar(c));
                else if (char.IsLetter(c))
                    sb.Append(char.ToLower(c));
            }
            return sb.ToString();
        }

        private static char GetPyChar(char c)
        {
            byte[] bytes = System.Text.Encoding.Default.GetBytes(c.ToString());
            int hi = bytes[0] * 256 + bytes[1];
            if (hi < 0xB0A1) return c;
            if (hi < 0xB0C5) return 'a';
            if (hi < 0xB2C1) return 'b';
            if (hi < 0xB4EE) return 'c';
            if (hi < 0xB6EA) return 'd';
            if (hi < 0xB7A2) return 'e';
            if (hi < 0xB8C1) return 'f';
            if (hi < 0xB9FE) return 'g';
            if (hi < 0xBBF7) return 'h';
            if (hi < 0xBFA6) return 'j';
            if (hi < 0xC0AC) return 'k';
            if (hi < 0xC2E8) return 'l';
            if (hi < 0xC4C3) return 'm';
            if (hi < 0xC5B6) return 'n';
            if (hi < 0xC5BE) return 'o';
            if (hi < 0xC6DA) return 'p';
            if (hi < 0xC8BB) return 'q';
            if (hi < 0xC8F6) return 'r';
            if (hi < 0xCBFA) return 's';
            if (hi < 0xCDDA) return 't';
            if (hi < 0xCEF4) return 'w';
            if (hi < 0xD1B9) return 'x';
            if (hi < 0xD4D1) return 'y';
            if (hi < 0xD7FA) return 'z';
            return c;
        }

        /// <summary>截断过长的文本并加省略号</summary>
        private static string TruncateText(string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            using (var g = Graphics.FromImage(new System.Drawing.Bitmap(1, 1)))
            {
                var sz = g.MeasureString(text, new Font("Segoe UI", 8F));
                if (sz.Width <= maxWidth) return text;
            }
            for (int len = text.Length - 1; len > 3; len--)
            {
                string trial = text.Substring(0, len) + "…";
                using (var g = Graphics.FromImage(new System.Drawing.Bitmap(1, 1)))
                {
                    if (g.MeasureString(trial, new Font("Segoe UI", 8F)).Width <= maxWidth)
                        return trial;
                }
            }
            return text;
        }

        /// <summary>更新搜索结果计数标签并刷新预览面板</summary>
        private void UpdateSearchCountLabel()
        {
            if (_clbProperties.Items.Count == 0 && _selectedInternalNames.Count == 0)
            {
                _lblSearchCount.Text = string.Empty;
                RefreshPreviewPanel();
                return;
            }
            int checkedCount = _selectedInternalNames.Count;
            int total = _allMetaProperties != null ? _allMetaProperties.Count : 0;
            _lblSearchCount.Text = $"已选 {checkedCount}/{total} 项";
            RefreshPreviewPanel();
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            if (_allMetaProperties == null || _allMetaProperties.Count == 0)
            {
                MessageBox.Show("请先点击「加载可用属性」按钮获取属性列表。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool hasNew = _selectedInternalNames.Count > 0;
            bool hasDelete = _existingProperties != null && _existingProperties.Any(p => p.MarkDeleted);
            if (!hasNew && !hasDelete)
            {
                string hint = _rbClassification.Checked
                    ? "请先勾选要插入的属性，或在「该分类已有属性」中标记要删除的属性。"
                    : "请先勾选要插入的属性（根目录模式）。";
                MessageBox.Show(hint, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_rbClassification.Checked && _cmbClassification.SelectedValue == null)
            {
                MessageBox.Show("请先在「选择目标分类」中选择一个分类。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            long? clsId = _rbClassification.Checked
                ? Convert.ToInt64(_cmbClassification.SelectedValue)
                : (long?)null;

            string target = clsId.HasValue
                ? $"分类 [{_cmbClassification.Text}]（ID={clsId}）"
                : "根目录";

            var newItems = _allMetaProperties
                .Where(p => _selectedInternalNames.Contains(p.InternalName))
                .ToList();
            var delItems = _existingProperties != null
                ? _existingProperties.Where(p => p.MarkDeleted).ToList()
                : new List<PropertyItem>();

            var result = new ImportResult();
            _cts = new CancellationTokenSource();

            int total = newItems.Count + delItems.Count;
            _lblStatus.Text = $"正在操作 {total} 条属性到 {target}...";
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = 0;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    int done = 0;

                    // 1) 新增属性
                    foreach (var meta in newItems)
                    {
                        if (_cts.Token.IsCancellationRequested) break;
                        done++;
                        SetProgress(done, total);
                        SetStatus($"正在插入：{meta.DisplayName}（{done}/{total}）");
                        try
                        {
                            _db.InsertProperty(meta.InternalName, meta.DisplayName, clsId);
                            lock (result.Errors)
                            {
                                result.SuccessCount++;
                                result.Changes.Add(string.Format("新增属性：{0} ({1})",
                                    meta.DisplayName, meta.InternalName));
                            }
                        }
                        catch (Exception ex)
                        {
                            lock (result.Errors)
                            {
                                result.FailCount++;
                                result.Errors.Add(new ImportError
                                {
                                    RowNumber = done,
                                    Name = meta.DisplayName,
                                    Message = ex.Message
                                });
                            }
                        }
                    }

                    // 2) 删除已标记属性（按 OBJECT_ID）
                    foreach (var p in delItems)
                    {
                        if (_cts.Token.IsCancellationRequested) break;
                        done++;
                        SetProgress(done, total);
                        SetStatus($"正在删除：{p.Name}（{done}/{total}）");
                        try
                        {
                            _db.DeleteProperty(p.ObjectId);
                            lock (result.Errors)
                            {
                                result.SuccessCount++;
                                result.DeleteCount++;
                                result.Changes.Add(string.Format("删除属性：{0} (ID={1})",
                                    p.Name, p.ObjectId));
                            }
                        }
                        catch (Exception ex)
                        {
                            lock (result.Errors)
                            {
                                result.FailCount++;
                                result.Errors.Add(new ImportError
                                {
                                    RowNumber = done,
                                    Name = p.Name,
                                    Message = ex.Message
                                });
                            }
                        }
                    }

                    SetProgress(0, 0);
                    SetStatus("操作完成");
                    ShowResultInUI(result, total, target, newItems.Count, delItems.Count);
                }
                catch (Exception ex)
                {
                    SetStatus("严重错误：" + ex.Message);
                }
            });
        }

        private void SetProgress(int current, int total)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(() =>
                {
                    if (total <= 0) { _progressBar.Value = 0; _lblProgress.Text = string.Empty; }
                    else { _progressBar.Value = Math.Min(100, current * 100 / total); _lblProgress.Text = $"{current}/{total}"; }
                }));
            else
            {
                if (total <= 0) { _progressBar.Value = 0; _lblProgress.Text = string.Empty; }
                else { _progressBar.Value = Math.Min(100, current * 100 / total); _lblProgress.Text = $"{current}/{total}"; }
            }
        }

        private void SetStatus(string msg)
        {
            if (_lblStatus.InvokeRequired)
                _lblStatus.Invoke(new Action(() => _lblStatus.Text = msg));
            else
                _lblStatus.Text = msg;
        }

        private void ShowResultInUI(ImportResult result, int total, string target, int insertCount, int deleteCount)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(() => ShowResultInUI(result, total, target, insertCount, deleteCount)));
            else
            {
                _lblResult.Text = result.Summary;
                _lblResult.ForeColor = result.FailCount > 0
                    ? Color.FromArgb(220, 53, 69)
                    : Color.FromArgb(40, 167, 69);

                var sb = new StringBuilder();
                sb.AppendLine($"向「{target}」操作属性：{result.Summary}");
                sb.AppendLine($"（新增 {insertCount} 项，删除 {deleteCount} 项）");
                sb.AppendLine(new string('=', 40));

                if (result.Errors.Count > 0)
                {
                    sb.AppendLine("失败详情：");
                    foreach (var err in result.Errors)
                        sb.AppendLine($"  [{err.Name}]：{err.Message}");
                }
                else
                {
                    sb.AppendLine("全部操作成功完成！");
                }

                _txtErrors.Text = sb.ToString();

                // 记录操作日志
                var detail = new ImportDetail();
                detail.FilePath = string.Empty;
                detail.Parameters = target;
                detail.SuccessCount = result.SuccessCount;
                detail.FailCount = result.FailCount;
                detail.TotalCount = total;
                detail.Cancelled = false;
                if (result.Errors.Count > 0)
                {
                    foreach (var err in result.Errors)
                        detail.Errors.Add("[" + err.Name + "]：" + err.Message);
                }
                if (result.Changes != null && result.Changes.Count > 0)
                {
                    foreach (var ch in result.Changes)
                        detail.Changes.Add(ch);
                }
                string logMsg = string.Format("属性操作 [{0}]：新增 {1}，删除 {2}，成功 {3}，失败 {4}",
                    target, insertCount, deleteCount, result.SuccessCount, result.FailCount);
                Logger.Instance.LogImport("属性插入/删除", logMsg, detail);

                // 导入完成后刷新状态，使界面与数据库保持一致，避免重复插入/重复删除
                _selectedInternalNames.Clear();
                ApplyPropertyDisableState();
                if (_rbClassification.Checked && _cmbClassification.SelectedValue != null)
                    LoadExistingProperties(Convert.ToInt64(_cmbClassification.SelectedValue));
                else
                    RefreshPreviewPanel();
            }
        }

        // ====== 预览行（仅用于渲染「本次插入完整属性预览」）=====
        private class PreviewRow
        {
            public string Name { get; set; }
            public string InternalName { get; set; }
            public bool IsNew { get; set; }
        }
    }
}
