using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    /// <summary>
    /// 超级批量导入数据预览 + 导入对话框
    /// TreeView 展示分类层级、属性及属性值，底部可直接导入
    /// 校验结果在右侧面板显示，有错误时可导出 Excel
    /// </summary>
    public partial class SuperBatchPreviewForm : Form
    {
        private readonly Dictionary<string, string> _propertySetDisplayNames;
        private readonly DbHelper _db;
        private readonly SuperBatchData _data;
        private readonly string _selectedTemplateUiContent;
        private readonly SuperBatchValidationResult _validationResult;
        private bool _importing;

        // 分类匹配模式开关（默认"作为全新分类导入"，保留原有默认行为）
        private System.Windows.Forms.Panel _pnlMatchMode;
        private System.Windows.Forms.Label _lblMatchMode;
        private System.Windows.Forms.RadioButton _rdoCreateNew;
        private System.Windows.Forms.RadioButton _rdoMatchByName;

        /// <summary>用户是否确认执行了导入</summary>
        public bool Imported { get; private set; }

        /// <summary>导入结果（仅 Imported=true 时有效）</summary>
        public ImportResult ImportResult { get; private set; }

        public SuperBatchPreviewForm(
            List<PreviewNode> tree,
            List<SuperBatchMetaProperty> metaProps,
            Dictionary<string, string> propertySetDisplayNames,
            DbHelper db = null,
            SuperBatchData data = null,
            string selectedTemplateUiContent = null,
            SuperBatchValidationResult validationResult = null)
        {
            InitializeComponent();
            InitMatchModeControls();
            _treeView.MouseUp += _treeView_MouseUp;
            _propertySetDisplayNames = propertySetDisplayNames ?? new Dictionary<string, string>();
            _db = db;
            _data = data;
            _selectedTemplateUiContent = selectedTemplateUiContent;
            _validationResult = validationResult ?? new SuperBatchValidationResult { IsValid = true };

            BuildTree(tree);
            if (metaProps != null && metaProps.Count > 0)
                BuildMetaNode(metaProps);

            // 显示校验结果
            DisplayValidationResult();

            // 如果无数据库连接，隐藏导入按钮
            if (_db == null || _data == null)
            {
                _btnImport.Visible = false;
                _progressBar.Visible = false;
                _txtLog.Visible = false;
            }
        }

        private void InitMatchModeControls()
        {
            // 名称匹配模式开关：默认"始终新建"，保留原有默认行为；
            // 用户可在预览时切换为"按名称匹配并复用已有（分类/分类组/属性值）"
            _pnlMatchMode = new System.Windows.Forms.Panel
            {
                Location = new System.Drawing.Point(12, 44),
                Size = new System.Drawing.Size(960, 32),
                BackColor = System.Drawing.Color.FromArgb(248, 249, 252)
            };
            _lblMatchMode = new System.Windows.Forms.Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(8, 9),
                Text = "匹配模式（按名称复用）："
            };
            _rdoCreateNew = new System.Windows.Forms.RadioButton
            {
                AutoSize = true,
                Checked = true,
                Location = new System.Drawing.Point(105, 7),
                Text = "始终新建（分类/组/属性值）",
                TabIndex = 9
            };
            _rdoMatchByName = new System.Windows.Forms.RadioButton
            {
                AutoSize = true,
                Location = new System.Drawing.Point(330, 7),
                Text = "按名称匹配并复用已有（分类/分类组/属性值）",
                TabIndex = 10
            };
            _pnlMatchMode.Controls.AddRange(new System.Windows.Forms.Control[] { _lblMatchMode, _rdoCreateNew, _rdoMatchByName });
            this.Controls.Add(_pnlMatchMode);
            // 把主面板下移，给开关让位（底部仍对齐 384，下方按钮/进度条不受影响）
            _splitMain.Location = new System.Drawing.Point(12, 80);
            _splitMain.Size = new System.Drawing.Size(960, 304);
        }

        private void DisplayValidationResult()
        {
            if (_validationResult == null || _validationResult.IsValid)
            {
                _lblValidationTitle.Text = "✓ 校验通过，数据无错误";
                _lblValidationTitle.ForeColor = Color.FromArgb(40, 167, 69);
                _lstErrors.Visible = false;
                _btnExportErrors.Visible = false;
                _btnImport.Enabled = _db != null && _data != null;
            }
            else
            {
                _lblValidationTitle.Text = string.Format("✗ 校验发现 {0} 个错误", _validationResult.Errors.Count);
                _lblValidationTitle.ForeColor = Color.FromArgb(220, 53, 69);
                _lstErrors.Clear();
                foreach (var err in _validationResult.Errors)
                    _lstErrors.AppendText("• " + err + Environment.NewLine);
                _lstErrors.Visible = true;
                _btnExportErrors.Visible = true;
                _btnImport.Enabled = false;
                _btnImport.BackColor = Color.FromArgb(108, 117, 125);
                _btnImport.Text = "校验未通过";
            }
        }

        private void BuildMetaNode(List<SuperBatchMetaProperty> metaProps)
        {
            var root = new TreeNode("◆ 元模型属性（" + metaProps.Count + "）");
            foreach (var mp in metaProps)
            {
                var propNode = new TreeNode(mp.InternalName);

                string fieldInfo = string.IsNullOrEmpty(mp.DisplayZhCn)
                    ? mp.InternalName
                    : mp.InternalName + "（" + mp.DisplayZhCn + "）";
                propNode.Nodes.Add("字段信息：" + fieldInfo);

                string setInfo;
                if (string.IsNullOrEmpty(mp.PropertySetInternalName))
                {
                    setInfo = "（不写入）";
                }
                else
                {
                    string setName = mp.PropertySetInternalName;
                    if (_propertySetDisplayNames.TryGetValue(mp.PropertySetInternalName, out var dn) && !string.IsNullOrEmpty(dn))
                        setName = mp.PropertySetInternalName + "（" + dn + "）";
                    setInfo = setName;
                }
                propNode.Nodes.Add("属性集：" + setInfo);

                root.Nodes.Add(propNode);
            }
            _treeView.Nodes.Add(root);
        }

        private void BuildTree(List<PreviewNode> nodes)
        {
            _treeView.Nodes.Clear();
            foreach (var node in nodes)
                _treeView.Nodes.Add(BuildNode(node));
            if (_treeView.Nodes.Count > 0)
                ExpandToDepth(_treeView.Nodes, 0);
        }

        private TreeNode BuildNode(PreviewNode node)
        {
            string label = node.NodeType + "：" + node.Name;
            if (node.IsClassification && node.GenerateForm)
                label += " [表单：" + (string.IsNullOrEmpty(node.FormName) ? node.Name : node.FormName) + "]";
            var tn = new TreeNode(label);

            foreach (var child in node.Children)
                tn.Nodes.Add(BuildNode(child));

            foreach (var prop in node.Properties)
            {
                string propLabel = "属性：" + prop.DisplayName;
                var propNode = new TreeNode(propLabel);
                foreach (var val in prop.Values)
                    propNode.Nodes.Add("属性编号：" + val.Number + "  属性值：" + val.Value);
                if (prop.Values.Count > 0)
                    tn.Nodes.Add(propNode);
            }

            return tn;
        }

        private static void ExpandToDepth(TreeNodeCollection nodes, int targetDepth)
        {
            foreach (TreeNode node in nodes)
            {
                node.Expand();
                if (targetDepth > 0)
                    ExpandToDepth(node.Nodes, targetDepth - 1);
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

        private void CtxExpand_Click(object sender, EventArgs e)
        {
            if (_treeView.SelectedNode != null)
                _treeView.SelectedNode.ExpandAll();
        }

        private void CtxCollapse_Click(object sender, EventArgs e)
        {
            if (_treeView.SelectedNode != null)
                _treeView.SelectedNode.Collapse();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _treeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                _treeView.SelectedNode = _treeView.GetNodeAt(e.X, e.Y);
        }

        // ====== 导出校验错误到 Excel ======
        private void BtnExportErrors_Click(object sender, EventArgs e)
        {
            if (_validationResult == null || _validationResult.Errors.Count == 0) return;

            // 将字符串错误转换为 ImportError 格式
            var errors = new List<ImportError>();
            for (int i = 0; i < _validationResult.Errors.Count; i++)
            {
                errors.Add(new ImportError
                {
                    RowNumber = 0,
                    Name = string.Empty,
                    Message = _validationResult.Errors[i]
                });
            }

            ErrorLogExporter.Export(errors, "校验错误");
        }

        // ====== 开始导入 ======
        private void _btnImport_Click(object sender, EventArgs e)
        {
            if (_db == null || _data == null) return;
            if (_validationResult != null && !_validationResult.IsValid) return;

            // 确认
            var stats = SuperBatchImportEngine.ComputeStats(_data);
            if (_data != null)
                _data.ClassificationMatchMode = _rdoMatchByName.Checked
                    ? ClassificationMatchMode.MatchByName
                    : ClassificationMatchMode.CreateNew;
            string confirmMsg = string.Format("确认导入以下数据？\n\n"
                + "· 分类组：{0} 个\n· 分类：{1} 个\n· 表单生成：{2} 个\n"
                + "· 属性定义：{3} 个\n· 元模型属性：{4} 个\n\n"
                + "整个导入过程将在一个数据库事务中完成，失败自动回滚。",
                stats.Groups, stats.Classifications, stats.Forms, stats.Properties, stats.MetaProperties);

            if (MessageBox.Show(confirmMsg, "确认导入",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            SetImporting(true);
            _txtLog.Clear();
            _progressBar.Value = 0;
            AppendLog("开始导入...");

            var bw = new BackgroundWorker();
            bw.DoWork += (s, args) =>
            {
                args.Result = SuperBatchImportEngine.Import(_data, _db,
                    msg => this.Invoke((Action)(() => AppendLog(msg))),
                    pct => this.Invoke((Action)(() => _progressBar.Value = pct)),
                    _selectedTemplateUiContent);
            };
            bw.RunWorkerCompleted += (s, args) =>
            {
                if (args.Error != null)
                {
                    AppendLog("导入失败：" + args.Error.Message);
                    _lblResult.Text = "结果：失败 - " + args.Error.Message;
                    _lblResult.ForeColor = Color.FromArgb(220, 53, 69);
                    Logger.Instance.LogError("超级批量导入", "导入失败：" + args.Error.Message);
                }
                else if (args.Cancelled)
                {
                    AppendLog("导入已被取消。");
                    _lblResult.Text = "结果：已取消";
                    _lblResult.ForeColor = Color.FromArgb(255, 193, 7);
                }
                else
                {
                    var result = args.Result as ImportResult;
                    if (result != null)
                    {
                        if (result.Aborted)
                        {
                            AppendLog("导入已终止，所有数据已回滚");
                            _lblResult.Text = "已终止 - 数据已回滚";
                            _lblResult.ForeColor = Color.FromArgb(255, 193, 7);
                            MessageBox.Show("导入已终止，所有数据已回滚。", "已终止",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Close();
                        }
                        else
                        {
                            AppendLog("导入结果：" + result.Summary);
                            _lblResult.Text = "结果：" + result.Summary;
                            _lblResult.ForeColor = Color.FromArgb(40, 167, 69);
                            Imported = true;
                            ImportResult = result;

                            // 记录导入日志
                            var detail = new ImportDetail();
                            detail.FilePath = string.Empty;
                            detail.Parameters = string.Format("分类组 {0}，分类 {1}，属性 {2}，属性值 {3}，元模型 {4}",
                                stats.Groups, stats.Classifications, stats.Properties, stats.PropertyValues, stats.MetaProperties);
                            detail.SuccessCount = result.SuccessCount;
                            detail.FailCount = result.FailCount;
                            detail.TotalCount = result.SuccessCount + result.FailCount;
                            detail.Cancelled = false;
                            if (result.Errors.Count > 0)
                            {
                                foreach (var err in result.Errors)
                                    detail.Errors.Add(err.ToString());
                            }
                            if (result.Changes != null && result.Changes.Count > 0)
                            {
                                foreach (var ch in result.Changes)
                                    detail.Changes.Add(ch);
                            }
                            Logger.Instance.LogImport("超级批量导入",
                                string.Format("超级批量导入完成：成功 {0}，失败 {1}", result.SuccessCount, result.FailCount),
                                detail);
                        }
                    }
                }
                SetImporting(false);

                if (Imported)
                {
                    MessageBox.Show("导入完成！\n" + (ImportResult?.Summary ?? "") +
                        "\n\n【后续操作提醒】\n1. 请手动在「维护配置」将属性集关联到 part 类并发布（让系统自动触发在相关表增加字段）\n2. 若属性集已关联到part类，请将 part 类手动发布一次",
                        "导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                }
            };
            bw.RunWorkerAsync();
        }

        private void SetImporting(bool importing)
        {
            _importing = importing;
            _btnImport.Text = importing ? "导入中..." : "开始导入";
            _btnImport.Enabled = !importing && (_validationResult == null || _validationResult.IsValid);
            _btnClose.Enabled = !importing;
            _btnExportErrors.Enabled = !importing;
        }

        private void AppendLog(string msg)
        {
            string line = DateTime.Now.ToString("HH:mm:ss") + " " + msg;
            _txtLog.AppendText(line + Environment.NewLine);
            _txtLog.SelectionStart = _txtLog.Text.Length;
            _txtLog.ScrollToCaret();

            Logger.Instance.Log("超级批量导入", msg);
        }
    }
}
