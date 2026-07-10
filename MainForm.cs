using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DataImportTool
{
    /// <summary>
    /// 功能导航中心（Hub）
    /// 只放功能入口 + 状态栏，导入逻辑一概不放这。
    /// 点入口弹二级窗口，具体操作都在那个窗口里做。
    /// </summary>
    public partial class MainForm : Form
    {
        // ====== 字段 ======
        private DbHelper _db;
        private AppConfig _config;
        private MenuStrip _menuStrip;

        /// <summary>当前配置（供子窗体访问 NameColumn 等配置项）</summary>
        public AppConfig Config => _config;

        // ====== 构造 ======
        public MainForm()
        {
            InitializeComponent();
        }

        // ====== 页面加载 ======
        private void MainForm_Load(object sender, EventArgs e)
        {
            BuildMenuStrip();
            LoadConfigAndConnect();
            BuildHub();
            Logger.Instance.Log("系统", "主窗体已加载。");
            StartupCheckUpdate();
        }

        // ====== 构建 Hub 卡片入口 ======
        // 所有控件已在 Designer.cs 中创建，这里只注册悬停效果和状态栏默认文字
        private void BuildHub()
        {
            // 为每张卡片的所有子控件（图标/标题/描述 Label）递归绑定 Click，
            // 确保点击卡片任意位置都能激活对应功能（Panel 自身的 Click 已在 Designer 中绑定）。
            BindChildClicks(_pnlCard1, btnGroupImport_Click);
            BindChildClicks(_pnlCard2, btnClsImport_Click);
            BindChildClicks(_pnlCard4, btnPropertyImport_Click);
            BindChildClicks(_pnlCard5, btnPropertyValueImport_Click);
            BindChildClicks(_pnlCard6, btnSettings_Click);
            BindChildClicks(_pnlCard7, btnBatchPropertyImport_Click);
            BindChildClicks(_pnlCard8, btnSuperBatchImport_Click);
            BindChildClicks(_pnlCard9, btnBatchImport_Click);
            BindChildClicks(_pnlCard10, btnWftScriptRemove_Click);
            BindChildClicks(_pnlCard11, btnLogViewer_Click);

            // 状态栏默认文字
            _lblStatusDb.Text = "状态：未知";
            _lblStatusInfo.Text = string.Empty;
            _lblVersion.Text = "v" + UpdateManager.CurrentVersionString;
        }

        // ====== 配置加载 ======
        private void LoadConfigAndConnect()
        {
            _config = ConfigManager.Load();
            RefreshConnectionInfo();

            if (!_config.IsConfigured)
            {
                if (_lblStatusDb != null)
                {
                    _lblStatusDb.Text = "状态：未配置";
                    _lblStatusDb.ForeColor = Color.Gray;
                }
                if (_lblStatusInfo != null)
                    _lblStatusInfo.Text = "请在「设置」中配置数据库连接";
                return;
            }

            TryConnect();
        }

        public void LogToApp(string module, string msg, string type = "Info")
        {
            Logger.Instance.Log(module, msg, type);
        }

        private void RefreshConnectionInfo()
        {
            if (_lblStatusInfo == null) return;
            if (_config == null || !_config.IsConfigured)
            {
                _lblStatusInfo.Text = string.Empty;
                return;
            }
            _lblStatusInfo.Text = _config.GetDisplayInfo();
        }

        private bool TryConnect()
        {
            if (_config == null || !_config.IsConfigured) return false;

            try
            {
                var connStr = _config.BuildConnectionString();
                _db = new DbHelper(connStr);
                _db.TestConnection();

                if (_lblStatusDb != null)
                {
                    _lblStatusDb.Text = "状态：已连接";
                    _lblStatusDb.ForeColor = Color.FromArgb(40, 167, 69);
                }
                Logger.Instance.LogSuccess("系统", "数据库连接成功。");
                ShowPlmVersion();
                return true;
            }
            catch (Exception ex)
            {
                if (_lblStatusDb != null)
                {
                    _lblStatusDb.Text = "状态：连接异常";
                    _lblStatusDb.ForeColor = Color.FromArgb(220, 53, 69);
                }
                if (_lblStatusInfo != null)
                    _lblStatusInfo.Text = "点击「设置」修复连接";
                Logger.Instance.LogError("系统", "数据库连接失败：" + ex.Message);
                return false;
            }
        }

        // ====== PLM 版本检测（连接成功后更新状态栏提示）======
        private void ShowPlmVersion()
        {
            if (_db == null) return;
            try
            {
                var ver = _db.DetectPlmVersion();
                if (ver == DbHelper.PlmVersion.V2027)
                {
                    _lblStatusInfo.Text = "当前PLM为2027版本，\u201c生成分类表单\u201d等功能不适用，请酌情使用！";
                    _lblStatusInfo.ForeColor = Color.FromArgb(220, 53, 69);
                }
                else if (ver == DbHelper.PlmVersion.V2026)
                {
                    _lblStatusInfo.Text = "当前PLM为2026版本，功能适用！";
                    _lblStatusInfo.ForeColor = Color.FromArgb(40, 167, 69);
                }
                else
                {
                    _lblStatusInfo.Text = _config.GetDisplayInfo();
                    _lblStatusInfo.ForeColor = Color.FromArgb(108, 117, 125);
                }
                Logger.Instance.Log("系统", "PLM 版本检测：" + ver);
                Logger.Instance.Log("系统", "插入分类时 " + DbHelper.ColSourceAbstractClsId + " 字段："
                    + (_db.ClassificationHasSourceAbstractClsId()
                        ? "检测到该列，写入默认值 0"
                        : "表中不存在该列，自动跳过"));
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("系统", "PLM 版本检测失败：" + ex.Message);
            }
        }



        // ====== 功能按钮事件（统一处理：检查连接 → 打开二级窗口）======
        private bool EnsureDbConnected()
        {
            if (_config == null || !_config.IsConfigured)
            {
                btnSettings_Click(null, null);
                if (_config == null || !_config.IsConfigured)
                {
                    MessageBox.Show("请先在「设置」中配置数据库连接。",
                        "未配置连接", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                TryConnect();
            }

            try
            {
                _db.TestConnection();
                _lblStatusDb.Text = "状态：已连接";
                _lblStatusDb.ForeColor = Color.FromArgb(40, 167, 69);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库连接失败：" + ex.Message,
                    "连接失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSettings_Click(null, null);
                return false;
            }
        }

        // 功能一：分类组导入
        private void btnGroupImport_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected())
            {
                return;
            }
            using (var form = new GroupImportForm(_db, _config))
            {
                form.ShowDialog();
            }
        }

        // 功能二：分类插入
        private void btnClsImport_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected()) return;
            using (var form = new InsertModeForm(_db, _config))
            {
                form.ShowDialog();
            }
        }

        // 功能三：插入属性
        private void btnPropertyImport_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected()) return;
            using (var form = new InsertPropertyForm(_db))
            {
                form.ShowDialog();
            }
        }

        // 功能六：属性值导入
        private void btnPropertyValueImport_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected()) return;
            using (var form = new InsertPropertyValueForm(_db))
            {
                form.ShowDialog();
            }
        }


        // PRD v2.1：批量属性值导入（旧版，三列格式）
        private void btnBatchPropertyValueImport_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected()) return;
            using (var form = new BatchPropertyValueImportForm(_db))
            {
                form.ShowDialog();
            }
        }

        // PRD v3.0：批量导入属性值（物品维度）
        private void btnBatchPropertyImport_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected()) return;
            using (var form = new BatchPropertyImportForm(_db))
            {
                form.ShowDialog();
            }
        }

        // 功能七：设置

        // 超级批量导入
        private void btnSuperBatchImport_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected())
            {
                return;
            }
            using (var form = new SuperBatchImportForm(_db))
            {
                form.ShowDialog();
            }
        }



        // 功能八：元模型属性导入（Card9）
        private void btnBatchImport_Click(object sender, EventArgs e)
        {
            using (var form = new MetaPropertyImportForm(_db, _config))
            {
                form.ShowDialog();
            }
        }

        // 功能十：去除工作流模板脚本（Card10）
        private void btnWftScriptRemove_Click(object sender, EventArgs e)
        {
            using (var form = new WftScriptRemoveForm())
            {
                form.ShowDialog();
            }
        }
        private void btnSettings_Click(object sender, EventArgs e)
        {
            var form = new SettingsForm(_config);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _config = form.Config;
                try
                {
                    ConfigManager.Save(_config);
                    Logger.Instance.LogSuccess("设置", "配置已保存。");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存配置失败：" + ex.Message,
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                RefreshConnectionInfo();
                if (_config.IsConfigured)
                {
                    if (TryConnect())
                        MessageBox.Show("连接成功！", "提示",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("配置已保存，但连接测试失败。",
                            "连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnSetupMethod_Click(object sender, EventArgs e)
        {
            var img = DataImportTool.Properties.EmbeddedImages._设置方法;
            if (img == null) { MessageBox.Show("图片加载失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            var form = new Form
            {
                Text = "设置方法",
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(900, 620),
                FormBorderStyle = FormBorderStyle.Sizable,
                MaximizeBox = true,
                MinimizeBox = true
            };
            var pb = new PictureBox
            {
                Image = img,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill
            };
            form.Controls.Add(pb);
            form.ShowDialog(this);
        }

        private void _card1Title_Click(object sender, EventArgs e)
        {

        }

        private void _pnlCard1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void _pnlCard8_Paint(object sender, PaintEventArgs e)
        {
        }

        // 功能九：查看日志
        private void btnLogViewer_Click(object sender, EventArgs e)
        {
            using (var form = new LogViewerForm())
            {
                form.ShowDialog(this);
            }
        }

        // ====== 卡片悬停高亮（仅视觉变化，不改变 Location，避免位移导致点击错位）======
        private void Card_MouseEnter(object sender, EventArgs e)
        {
            var ctrl = sender as Control;
            var card = ctrl as Panel ?? ctrl?.Parent as Panel;
            if (card != null) card.BackColor = Color.FromArgb(233, 236, 239);
        }

        private void Card_MouseLeave(object sender, EventArgs e)
        {
            var ctrl = sender as Control;
            var card = ctrl as Panel ?? ctrl?.Parent as Panel;
            if (card != null) card.BackColor = Color.White;
        }

        // 递归为卡片的所有子控件绑定 Click（点击图标/标题/描述也能激活功能）
        private void BindChildClicks(Control parent, EventHandler handler)
        {
            foreach (Control c in parent.Controls)
            {
                c.Click += handler;
                BindChildClicks(c, handler);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        // ====== 自动更新：菜单与检测 ======
        private void BuildMenuStrip()
        {
            _menuStrip = new MenuStrip();
            _menuStrip.Dock = DockStyle.Top;

            // 系统配置管理（与「帮助」同级相邻）
            var sysConfig = new ToolStripMenuItem("系统配置管理(&S)");
            var matchRule = new ToolStripMenuItem("匹配规则配置(&M)");
            matchRule.Click += MatchRuleConfigMenu_Click;
            var codeLink = new ToolStripMenuItem("领码设置(&C)");
            codeLink.Click += CodeLinkSettingMenu_Click;
            var configMig = new ToolStripMenuItem("配置迁移(&I)");
            configMig.Click += ConfigMigrationMenu_Click;
            sysConfig.DropDownItems.Add(matchRule);
            sysConfig.DropDownItems.Add(codeLink);
            sysConfig.DropDownItems.Add(configMig);

            // 数据维护
            var dataMaint = new ToolStripMenuItem("数据维护(&D)");
            var batchAddPropVal = new ToolStripMenuItem("批量追加通用属性值(&P)");
            batchAddPropVal.Click += BatchAddCommonPropValMenu_Click;
            dataMaint.DropDownItems.Add(batchAddPropVal);

            // 帮助（仅保留检查更新与关于）
            var help = new ToolStripMenuItem("帮助(&H)");
            var checkUpdate = new ToolStripMenuItem("检查更新(&U)");
            checkUpdate.Click += CheckUpdateMenu_Click;
            var about = new ToolStripMenuItem("关于(&A)");
            about.Click += AboutMenu_Click;
            help.DropDownItems.Add(checkUpdate);
            help.DropDownItems.Add(about);

            _menuStrip.Items.Add(sysConfig);
            _menuStrip.Items.Add(dataMaint);
            _menuStrip.Items.Add(help);
            this.Controls.Add(_menuStrip);
            _menuStrip.SendToBack();
            this.MainMenuStrip = _menuStrip;
        }

        // 启动静默检查（后台线程，不阻塞 UI）
        private void StartupCheckUpdate()
        {
            var bw = new BackgroundWorker();
            bw.DoWork += (s, e) => { e.Result = UpdateManager.CheckForUpdate(); };
            bw.RunWorkerCompleted += (s, e) =>
            {
                if (this.IsDisposed || this.Disposing) return;
                var result = e.Result as UpdateCheckResult;
                if (result == null || !result.Success || !result.UpdateAvailable || result.Info == null)
                    return;
                if (!result.Info.IsMandatory)
                {
                    string skipped = UpdateManager.GetSkipVersion();
                    if (!string.IsNullOrWhiteSpace(skipped) &&
                        string.Equals(skipped, result.Info.Version, StringComparison.OrdinalIgnoreCase))
                        return;
                }
                this.Invoke((MethodInvoker)delegate
                {
                    using (var dlg = new UpdateDialog(result.Info, UpdateManager.CurrentVersionString, result.Info.IsMandatory))
                    {
                        dlg.ShowDialog(this);
                    }
                });
            };
            bw.RunWorkerAsync();
        }

        // 打开匹配规则配置
        private void MatchRuleConfigMenu_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected()) return;
            using (var form = new MatchRuleConfigForm(_db))
            {
                form.ShowDialog();
            }
        }

        // 打开领码设置
        private void CodeLinkSettingMenu_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected()) return;
            using (var form = new CodeLinkSettingForm(_db))
            {
                form.ShowDialog();
            }
        }

        // 打开配置迁移
        private void ConfigMigrationMenu_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected()) return;
            using (var form = new ConfigMigrationForm(_db, _config))
            {
                form.ShowDialog();
            }
        }

        // 打开批量追加通用属性值
        private void BatchAddCommonPropValMenu_Click(object sender, EventArgs e)
        {
            if (!EnsureDbConnected()) return;
            using (var form = new BatchAddCommonPropertyValueForm(_db))
            {
                form.ShowDialog();
            }
        }

        // 手动检查更新（菜单）
        private void CheckUpdateMenu_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            var bw = new BackgroundWorker();
            bw.DoWork += (s, ev) => { ev.Result = UpdateManager.CheckForUpdate(); };
            bw.RunWorkerCompleted += (s, ev) =>
            {
                if (this.IsDisposed) return;
                this.Cursor = Cursors.Default;
                var res = ev.Result as UpdateCheckResult;
                if (res == null || !res.Success)
                {
                    MessageBox.Show(this, res != null ? res.ErrorMessage : "检查更新失败。",
                        "检查更新", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!res.UpdateAvailable || res.Info == null)
                {
                    MessageBox.Show(this, "当前已是最新版本。", "检查更新",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using (var dlg = new UpdateDialog(res.Info, UpdateManager.CurrentVersionString, res.Info.IsMandatory))
                {
                    dlg.ShowDialog(this);
                }
            };
            bw.RunWorkerAsync();
        }

        private void AboutMenu_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                string.Format("DataImportTool{0}版本：v{1}{0}更新服务器：{2}",
                    Environment.NewLine, UpdateManager.CurrentVersionString, UpdateManager.UpdateServerUrl),
                "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
