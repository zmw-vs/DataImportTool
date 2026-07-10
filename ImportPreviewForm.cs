using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DataImportTool
{
    /// <summary>
    /// 通用导入预览窗体
    /// 左侧 TreeView 展示数据预览，右侧显示错误信息
    /// </summary>
    public partial class ImportPreviewForm : Form
    {
        private readonly List<ImportError> _errors;

        /// <summary>用户是否确认执行导入</summary>
        public bool Confirmed { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodes">TreeView 节点列表</param>
        /// <param name="errors">错误列表（可选）</param>
        /// <param name="title">窗体标题（可选）</param>
        public ImportPreviewForm(List<TreeNode> nodes, List<ImportError> errors = null, string title = null)
        {
            InitializeComponent();
            _errors = errors ?? new List<ImportError>();

            // 设置标题
            if (!string.IsNullOrEmpty(title))
            {
                this.Text = title;
                _lblTitle.Text = title;
            }

            // 构建 TreeView
            BuildTree(nodes);

            // 显示错误信息
            DisplayErrors();

            // 绑定事件
            _btnExpandAll.Click += BtnExpandAll_Click;
            _btnCollapseAll.Click += BtnCollapseAll_Click;
            _btnExportErrors.Click += BtnExportErrors_Click;
            _btnImport.Click += BtnImport_Click;
            _btnClose.Click += BtnClose_Click;
        }

        /// <summary>
        /// 构建 TreeView
        /// </summary>
        private void BuildTree(List<TreeNode> nodes)
        {
            _treeView.Nodes.Clear();
            if (nodes == null || nodes.Count == 0)
            {
                _treeView.Nodes.Add(new TreeNode("(无数据)"));
                return;
            }

            foreach (var node in nodes)
            {
                _treeView.Nodes.Add(node);
            }

            // 默认展开第一层
            if (_treeView.Nodes.Count > 0)
            {
                _treeView.Nodes[0].Expand();
            }
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        private void DisplayErrors()
        {
            if (_errors == null || _errors.Count == 0)
            {
                _lblErrorsTitle.Text = "✓ 校验通过，无错误";
                _lblErrorsTitle.ForeColor = Color.FromArgb(40, 167, 69);
                _txtErrors.Text = "数据校验通过，可以执行导入。";
                _btnExportErrors.Visible = false;
                _btnImport.Enabled = true;
            }
            else
            {
                _lblErrorsTitle.Text = string.Format("✗ 校验发现 {0} 个错误", _errors.Count);
                _lblErrorsTitle.ForeColor = Color.FromArgb(220, 53, 69);

                var sb = new StringBuilder();
                foreach (var err in _errors)
                {
                    sb.AppendLine(err.ToString());
                }
                _txtErrors.Text = sb.ToString();

                _btnExportErrors.Visible = true;
                _btnImport.Enabled = false;
                _btnImport.BackColor = Color.FromArgb(108, 117, 125);
                _btnImport.Text = "校验未通过";
            }
        }

        private void BtnExpandAll_Click(object sender, EventArgs e)
        {
            _treeView.BeginUpdate();
            _treeView.ExpandAll();
            _treeView.EndUpdate();
        }

        private void BtnCollapseAll_Click(object sender, EventArgs e)
        {
            _treeView.BeginUpdate();
            _treeView.CollapseAll();
            if (_treeView.Nodes.Count > 0)
                _treeView.Nodes[0].Expand();
            _treeView.EndUpdate();
        }

        private void BtnExportErrors_Click(object sender, EventArgs e)
        {
            if (_errors == null || _errors.Count == 0) return;
            ErrorLogExporter.Export(_errors, "导入错误日志");
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            if (_errors != null && _errors.Count > 0)
            {
                MessageBox.Show("存在校验错误，请先修复错误后再导入。",
                    "无法导入", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Confirmed = true;
            this.Close();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Confirmed = false;
            this.Close();
        }
    }
}
