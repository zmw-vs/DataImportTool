using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DataImportTool
{
    /// <summary>
    /// 操作日志查看窗口
    /// 支持关键字搜索、模块筛选、清空日志
    /// </summary>
    public partial class LogViewerForm : Form
    {
        private List<LogEntry> _allLogs;

        public LogViewerForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            LoadModules();
            RefreshGrid();
        }

        // ====== 加载模块筛选下拉框 ======
        private void LoadModules()
        {
            var modules = Logger.Instance.GetAllModules();
            _cmbModule.Items.Clear();
            _cmbModule.Items.Add("（全部模块）");
            foreach (var m in modules)
                _cmbModule.Items.Add(m);
            _cmbModule.SelectedIndex = 0;
        }

        // ====== 刷新表格 ======
        private void RefreshGrid()
        {
            string keyword = _txtSearch.Text.Trim();
            string moduleFilter = null;
            if (_cmbModule.SelectedIndex > 0)
                moduleFilter = _cmbModule.SelectedItem.ToString();

            _allLogs = string.IsNullOrEmpty(keyword) && string.IsNullOrEmpty(moduleFilter)
                ? Logger.Instance.GetAllLogs()
                : Logger.Instance.Search(keyword, moduleFilter);

            _dgvLogs.Rows.Clear();

            foreach (var entry in _allLogs)
            {
                var rowIndex = _dgvLogs.Rows.Add(
                    entry.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    entry.Module,
                    entry.Type,
                    entry.Message);

                // 根据类型设置行颜色
                var row = _dgvLogs.Rows[rowIndex];
                switch (entry.Type)
                {
                    case "Error":
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(220, 53, 69);
                        break;
                    case "Success":
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(40, 167, 69);
                        break;
                    case "Import":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
                        row.DefaultCellStyle.ForeColor = entry.Detail != null && entry.Detail.FailCount > 0
                            ? Color.FromArgb(220, 53, 69)
                            : Color.FromArgb(40, 167, 69);
                        break;
                    default:
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
                        break;
                }
            }

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            int total = _allLogs.Count;
            int errors = _allLogs.Count(e => e.Type == "Error");
            int success = _allLogs.Count(e => e.Type == "Success");
            _lblStatus.Text = string.Format(
                "共 {0} 条日志  |  成功 {1} 条  |  错误 {2} 条  |  日志文件：{3}",
                total, success, errors, Logger.Instance.GetFilePath());
        }

        // ====== 事件处理 ======

        private void _txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void _cmbModule_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void _btnRefresh_Click(object sender, EventArgs e)
        {
            LoadModules();
            RefreshGrid();
        }

        private void _btnClear_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "确定要清空所有操作日志吗？此操作不可撤销。",
                "确认清空",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Logger.Instance.Clear();
                RefreshGrid();
            }
        }

        private void _btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _dgvLogs_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _allLogs.Count) return;
            var entry = _allLogs[e.RowIndex];
            if (entry.Detail == null) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("时间：" + entry.Time.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("模块：" + entry.Module);
            sb.AppendLine("文件：" + (entry.Detail.FilePath ?? "(无)"));
            if (!string.IsNullOrEmpty(entry.Detail.Parameters))
                sb.AppendLine("参数：" + entry.Detail.Parameters);
            sb.AppendLine("总行数：" + entry.Detail.TotalCount);
            sb.AppendLine("成功：" + entry.Detail.SuccessCount + "  失败：" + entry.Detail.FailCount);
            if (entry.Detail.Cancelled)
                sb.AppendLine("(用户已取消)");
            sb.AppendLine();
            if (entry.Detail.Errors != null && entry.Detail.Errors.Count > 0)
            {
                sb.AppendLine("错误详情：");
                foreach (var err in entry.Detail.Errors)
                    sb.AppendLine("  " + err);
            }
            else
            {
                sb.AppendLine("无错误。");
            }

            if (entry.Detail.Changes != null && entry.Detail.Changes.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("变更记录（" + entry.Detail.Changes.Count + " 条）：");
                foreach (var ch in entry.Detail.Changes)
                    sb.AppendLine("  " + ch);
            }

            MessageBox.Show(sb.ToString(), "导入详情", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
