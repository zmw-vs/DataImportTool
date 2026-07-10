using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DataImportTool
{
    public partial class MetaPropertyPreviewForm : Form
    {
        private readonly MetaPropertyPreviewResult _result;
        private readonly DbHelper _db;
        private readonly string _propertySetInternalName;
        private readonly List<MetaPropertyRow> _originalRows;

        private static readonly Color _colorEditableBg = Color.FromArgb(227, 242, 253);
        private static readonly Color _colorEditableFg = Color.FromArgb(13, 71, 161);
        private static readonly Color _colorIgnoredBg = Color.FromArgb(255, 205, 210);
        private static readonly Color _colorIgnoredFg = Color.FromArgb(183, 28, 28);
        private static readonly Color _colorUnsetBg = Color.FromArgb(255, 255, 200);
        private static readonly Color _colorUnsetFg = Color.FromArgb(180, 130, 0);

        /// <summary>用户点击「确认并继续导入」</summary>
        public bool Confirmed { get; private set; }

        /// <summary>用户点击「终止导入」— 需要回滚</summary>
        public bool Aborted { get; private set; }

        public MetaPropertyPreviewForm(
            MetaPropertyPreviewResult result,
            DbHelper db,
            string propertySetInternalName,
            List<MetaPropertyRow> originalRows)
        {
            _result = result ?? throw new ArgumentNullException("result");
            _db = db;
            _propertySetInternalName = propertySetInternalName;
            _originalRows = originalRows;

            InitializeComponent();
            Text = string.Format("导入预览（共 {0} 行）", result.TotalRows);
            BuildGrid();
            UpdateSummary();
            if (!result.Rows.Any(r => r.Action == "更新"))
                _btnUpdateAll.Visible = _btnIgnoreAll.Visible = false;

            // 区分两种用法：
            //   带 _db —— 元模型独立导入，由「开始导入」按钮完成，不需要「确认并继续导入」
            //   不带 _db —— 超级批量导入中的冲突处理，全靠「确认并继续导入」返回 Confirmed
            if (_db != null)
            {
                _btnConfirm.Visible = false;
                _btnConfirm.Enabled = false;
            }
        }

        public MetaPropertyPreviewForm(MetaPropertyPreviewResult result)
            : this(result, null, null, null)
        {
            _btnImport.Visible = _btnImport.Enabled = false;
        }

        private void BuildGrid()
        {
            dgvPreview.AutoGenerateColumns = false;
            dgvPreview.AllowUserToAddRows = false;
            dgvPreview.AllowUserToDeleteRows = false;
            dgvPreview.ReadOnly = false;
            dgvPreview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPreview.MultiSelect = false;
            dgvPreview.RowHeadersVisible = true;
            dgvPreview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvPreview.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvPreview.ColumnHeadersHeight = 30;
            dgvPreview.RowTemplate.Height = 24;
            dgvPreview.BackgroundColor = Color.White;
            dgvPreview.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvPreview.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RowNumber", HeaderText = "行号", DataPropertyName = "RowNumber",
                Width = 45, MinimumWidth = 40, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "InternalName", HeaderText = "属性内部名", DataPropertyName = "InternalName",
                Width = 130, MinimumWidth = 100, ReadOnly = true
            });

            var actionCol = new DataGridViewComboBoxColumn
            {
                Name = "UserAction",
                HeaderText = "⚡ 处理方式",
                Width = 95, MinimumWidth = 85,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Flat
            };
            actionCol.Items.AddRange("更新", "忽略");
            actionCol.HeaderCell.Style.BackColor = Color.FromArgb(25, 118, 210);
            actionCol.HeaderCell.Style.ForeColor = Color.White;
            actionCol.HeaderCell.Style.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgvPreview.Columns.Add(actionCol);

            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CurrentZhCn", HeaderText = "现有中文名", DataPropertyName = "CurrentZhCn",
                Width = 105, MinimumWidth = 80, ReadOnly = true
            });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CurrentEnUs", HeaderText = "现有英文名", DataPropertyName = "CurrentEnUs",
                Width = 105, MinimumWidth = 80, ReadOnly = true
            });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CurrentZhTw", HeaderText = "现有繁体名", DataPropertyName = "CurrentZhTw",
                Width = 105, MinimumWidth = 80, ReadOnly = true
            });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NewZhCn", HeaderText = "新中文名 ▼", DataPropertyName = "NewZhCn",
                Width = 110, MinimumWidth = 80, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 248, 255) }
            });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NewEnUs", HeaderText = "新英文名 ▼", DataPropertyName = "NewEnUs",
                Width = 110, MinimumWidth = 80, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 248, 255) }
            });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NewZhTw", HeaderText = "新繁体名 ▼", DataPropertyName = "NewZhTw",
                Width = 110, MinimumWidth = 80, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 248, 255) }
            });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Changes", HeaderText = "变更说明", DataPropertyName = "Changes",
                Width = 200, MinimumWidth = 140, ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvPreview.DataSource = new BindingSource { DataSource = _result.Rows };

            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                var data = row.DataBoundItem as MetaPropertyPreviewRow;
                if (data == null) continue;
                var cell = row.Cells["UserAction"] as DataGridViewComboBoxCell;

                if (data.Action == "新增")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(232, 245, 233);
                    LockCombo(cell, "新增", Color.FromArgb(46, 125, 50));
                    data.ShouldUpdate = true;
                }
                else if (data.Action == "更新")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 224);
                    // 不设默认值 — 用户必须手动选
                    if (cell != null)
                    {
                        cell.Value = null;
                        cell.ReadOnly = false;
                        cell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
                        cell.Style.BackColor = _colorUnsetBg;
                        cell.Style.ForeColor = _colorUnsetFg;
                        cell.Style.SelectionBackColor = _colorUnsetBg;
                        cell.Style.SelectionForeColor = _colorUnsetFg;
                    }
                    data.ShouldUpdate = false;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
                    LockCombo(cell, "跳过", Color.FromArgb(158, 158, 158));
                    data.ShouldUpdate = false;
                }
            }

            dgvPreview.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvPreview.IsCurrentCellDirty && dgvPreview.CurrentCell is DataGridViewComboBoxCell)
                    dgvPreview.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvPreview.CellValueChanged += dgvPreview_CellValueChanged;
            dgvPreview.MouseClick += (s, e) =>
            {
                var hit = dgvPreview.HitTest(e.X, e.Y);
                if (hit.Type == DataGridViewHitTestType.None || hit.Type == DataGridViewHitTestType.Cell)
                    dgvPreview.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
        }

        private static void LockCombo(DataGridViewComboBoxCell cell, string text, Color fg)
        {
            if (cell == null) return;
            cell.Value = text;
            cell.ReadOnly = true;
            cell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            cell.FlatStyle = FlatStyle.Flat;
            cell.Style.ForeColor = fg;
            cell.Style.BackColor = Color.Transparent;
        }

        private static void SetComboValue(DataGridViewComboBoxCell cell, string val, Color bg, Color fg)
        {
            if (cell == null || cell.ReadOnly) return;
            cell.Value = val;
            cell.Style.BackColor = bg;
            cell.Style.ForeColor = fg;
            cell.Style.SelectionBackColor = bg;
            cell.Style.SelectionForeColor = fg;
        }

        private void UpdateSummary()
        {
            int totalConflict = _result.Rows.Count(r => r.Action == "更新");
            int willUpdate = _result.Rows.Count(r => r.ShouldUpdate && r.Action == "更新");
            int willIgnore = _result.Rows.Count(r => !r.ShouldUpdate && r.Action == "更新");
            int unselected = totalConflict - willUpdate - willIgnore;

            string status;
            if (unselected > 0)
                status = string.Format(" ⚠ 还有 {0} 条未选择", unselected);
            else
                status = " ✓ 全部已选择";

            lblSummary.Text = string.Format(
                "共 {0} 行 | 新增 {1} | 冲突 {2} | 将更新 {3} | 将忽略 {4} | 跳过 {5}{6}",
                _result.TotalRows, _result.WillInsert, totalConflict, willUpdate, willIgnore,
                _result.WillSkip, status);

            lblInsertCount.Text = string.Format("新增：{0}", _result.WillInsert);
            lblInsertCount.ForeColor = Color.FromArgb(0, 120, 50);

            lblUpdateCount.Text = string.Format("将更新：{0} / 冲突：{1}", willUpdate, totalConflict);
            lblUpdateCount.ForeColor = _colorEditableFg;

            lblSkipCount.Text = string.Format("将忽略：{0}", willIgnore);
            lblSkipCount.ForeColor = willIgnore > 0 ? _colorIgnoredFg : Color.Gray;

            // 启用/禁用确认按钮（独立导入流程中该按钮已隐藏，保持禁用）
            _btnConfirm.Enabled = _btnConfirm.Visible && unselected == 0;
        }

        private void _btnUpdateAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                var data = row.DataBoundItem as MetaPropertyPreviewRow;
                if (data == null || data.Action != "更新") continue;
                SetComboValue(row.Cells["UserAction"] as DataGridViewComboBoxCell, "更新", _colorEditableBg, _colorEditableFg);
                data.ShouldUpdate = true;
            }
            dgvPreview.Refresh();
            UpdateSummary();
        }

        private void _btnIgnoreAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                var data = row.DataBoundItem as MetaPropertyPreviewRow;
                if (data == null || data.Action != "更新") continue;
                SetComboValue(row.Cells["UserAction"] as DataGridViewComboBoxCell, "忽略", _colorIgnoredBg, _colorIgnoredFg);
                data.ShouldUpdate = false;
            }
            dgvPreview.Refresh();
            UpdateSummary();
        }

        private void dgvPreview_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvPreview.Columns[e.ColumnIndex].Name != "UserAction") return;
            if (e.RowIndex >= _result.Rows.Count) return;

            var cell = dgvPreview.Rows[e.RowIndex].Cells["UserAction"] as DataGridViewComboBoxCell;
            if (cell == null || cell.ReadOnly) return;

            var data = _result.Rows[e.RowIndex];
            if (data.Action != "更新") return;

            string val = cell.Value as string;
            if (val == "忽略")
            {
                cell.Style.BackColor = _colorIgnoredBg;
                cell.Style.ForeColor = _colorIgnoredFg;
                cell.Style.SelectionBackColor = _colorIgnoredBg;
                cell.Style.SelectionForeColor = _colorIgnoredFg;
                data.ShouldUpdate = false;
            }
            else if (val == "更新")
            {
                cell.Style.BackColor = _colorEditableBg;
                cell.Style.ForeColor = _colorEditableFg;
                cell.Style.SelectionBackColor = _colorEditableBg;
                cell.Style.SelectionForeColor = _colorEditableFg;
                data.ShouldUpdate = true;
            }

            dgvPreview.InvalidateCell(e.ColumnIndex, e.RowIndex);
            UpdateSummary();
        }

        // ====== 确认并继续导入 ======
        private void _btnConfirm_Click(object sender, EventArgs e)
        {
            // 校验所有冲突行已选择
            var unselected = new List<string>();
            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                var data = row.DataBoundItem as MetaPropertyPreviewRow;
                if (data == null || data.Action != "更新") continue;
                var cell = row.Cells["UserAction"] as DataGridViewComboBoxCell;
                string val = cell?.Value as string;
                if (val != "更新" && val != "忽略")
                    unselected.Add(data.InternalName);
            }
            if (unselected.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append("以下 ").Append(unselected.Count).AppendLine(" 个冲突属性尚未选择处理方式：");
                sb.AppendLine();
                foreach (var n in unselected)
                    sb.Append("  · ").AppendLine(n);
                sb.AppendLine();
                sb.AppendLine("请为每条选择「更新」或「忽略」后再确认。");
                MessageBox.Show(sb.ToString(), "未选择处理方式", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Confirmed = true;
            Aborted = false;
            Close();
        }

        // ====== 终止导入 ======
        private void _btnAbort_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要终止本次导入吗？\n所有已导入的数据将回滚。",
                "确认终止", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Logger.Instance.LogError("元模型属性导入", "用户终止导入，所有数据回滚");
                Aborted = true;
                Confirmed = false;
                Close();
            }
        }

        // ====== 开始导入（有 _db 时可用，在元模型独立导入流程中使用）======
        private void _btnImport_Click(object sender, EventArgs e)
        {
            if (_db == null)
            {
                MessageBox.Show("无法导入：未提供数据库连接信息。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 校验
            var unselected = new List<string>();
            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                var data = row.DataBoundItem as MetaPropertyPreviewRow;
                if (data == null || data.Action != "更新") continue;
                var cell = row.Cells["UserAction"] as DataGridViewComboBoxCell;
                string val = cell?.Value as string;
                if (val != "更新" && val != "忽略")
                    unselected.Add(data.InternalName);
            }
            if (unselected.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append("以下 ").Append(unselected.Count).AppendLine(" 个冲突属性尚未选择处理方式：");
                sb.AppendLine();
                foreach (var n in unselected)
                    sb.Append("  · ").AppendLine(n);
                sb.AppendLine();
                sb.AppendLine("请为每条选择「更新」或「忽略」后再导入。");
                MessageBox.Show(sb.ToString(), "未选择处理方式", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 收集忽略列表
            var skipUpdates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in _result.Rows)
            {
                if (row.Action == "更新" && !row.ShouldUpdate)
                    skipUpdates.Add(row.InternalName);
            }

            if (skipUpdates.Count > 0 && MessageBox.Show(
                string.Format("有 {0} 个已有属性将「忽略」（保留现有值），确定继续吗？", skipUpdates.Count),
                "确认导入", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _btnImport.Enabled = false;
            _btnAbort.Enabled = false;
            _btnConfirm.Enabled = false;
            _lblImportStatus.Text = "正在导入...";
            _lblImportStatus.Visible = true;
            Application.DoEvents();

            try
            {
                var cts = new CancellationTokenSource();
                Action<string> logger = msg => this.Invoke(new Action(() =>
                {
                    _lblImportStatus.Text = msg;
                    Application.DoEvents();
                }));

                var importResult = _db.ImportMetaPropertySet(
                    _propertySetInternalName, _originalRows, cts.Token, logger, skipUpdates);

                Logger.Instance.LogSuccess("元模型属性导入",
                    string.Format("目标属性集：{0}，新增 {1}，更新 {2}，跳过 {3}",
                        _propertySetInternalName, importResult.Inserted, importResult.Updated, importResult.SkippedExist));

                _lblImportStatus.Text = string.Format("导入完成：新增 {0}，更新 {1}，跳过 {2}",
                    importResult.Inserted, importResult.Updated, importResult.SkippedExist);
                Confirmed = true;
                Aborted = false;
                MessageBox.Show(
                    string.Format("导入完成！\n\n新增：{0} 条\n更新：{1} 条\n属性集跳过：{2} 条\n\n【后续操作提醒】\n1. 请手动在「维护配置」将属性集关联到 part 类并发布（让系统自动触发在相关表增加字段）\n2. 若属性集已关联到part类，请将part类手动发布一次",
                        importResult.Inserted, importResult.Updated, importResult.PropertySetSkipped),
                    "导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("元模型属性导入", "导入失败：" + ex.Message);
                _lblImportStatus.Text = "导入失败：" + ex.Message;
                MessageBox.Show("导入失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _btnImport.Enabled = true;
                _btnAbort.Enabled = true;
                _btnConfirm.Enabled = true;
            }
        }

        private void MetaPropertyPreviewForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Esc 仅在「确认并继续导入」可见（超级批量冲突处理流程）时等同确认
            if (e.KeyCode == Keys.Escape && _btnConfirm.Visible)
                _btnConfirm_Click(sender, e);
        }
    }
}
