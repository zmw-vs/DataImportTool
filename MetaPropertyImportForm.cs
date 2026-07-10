using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DataImportTool
{
    public partial class MetaPropertyImportForm : Form
    {
        private readonly DbHelper _db;
        private readonly AppConfig _config;
        private List<MetaPropertySetItem> _propertySets;
        private string _selectedFilePath;
        private string _selectedPropertySetInternalName;

        public MetaPropertyImportForm(DbHelper db, AppConfig config)
        {
            InitializeComponent();
            _db = db;
            _config = config;
            _propertySets = new List<MetaPropertySetItem>();
            _selectedFilePath = string.Empty;
            _selectedPropertySetInternalName = string.Empty;
        }

        private void MetaPropertyImportForm_Load(object sender, EventArgs e)
        {
            LoadPropertySets();
        }

        private void LoadPropertySets()
        {
            try
            {
                _propertySets = _db.GetPropertySets();
                cmbPropertySet.DisplayMember = "DisplayName";
                cmbPropertySet.ValueMember = "InternalName";
                cmbPropertySet.DataSource = _propertySets;

                if (_propertySets.Count == 0)
                {
                    LogMessage("[警告] 未找到任何属性集，请先在系统中创建属性集。");
                }
            }
            catch (Exception ex)
            {
                LogMessage(string.Format("[错误] 加载属性集失败：{0}", ex.Message));
                cmbPropertySet.Enabled = false;
            }
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel 文件|*.xlsx";
                ofd.Title = "选择元模型属性导入文件";
                ofd.CheckFileExists = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedFilePath = ofd.FileName;
                    txtFilePath.Text = Path.GetFileName(_selectedFilePath);
                    LogMessage(string.Format("[信息] 已选择文件：{0}", _selectedFilePath));
                }
            }
        }

        private void btnGenerateTemplate_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel 文件|*.xlsx";
                sfd.Title = "保存元模型属性导入模板";
                sfd.FileName = "元模型属性导入模板.xlsx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExcelHelper.GenerateMetaPropertyImportTemplate(sfd.FileName);
                        LogMessage(string.Format("[成功] 模板已生成：{0}", sfd.FileName));
                    }
                    catch (Exception ex)
                    {
                        LogMessage(string.Format("[错误] 生成模板失败：{0}", ex.Message));
                    }
                }
            }
        }

        private bool ContainsChinese(string text)
        {
            foreach (char c in text)
            {
                if (c >= 0x4E00 && c <= 0x9FFF)
                    return true;
            }
            return false;
        }

        private void LogMessage(string message)
        {
            txtLog.AppendText(message + Environment.NewLine);
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            // 校验下拉框必选
            if (cmbPropertySet.SelectedIndex < 0 || string.IsNullOrEmpty(cmbPropertySet.SelectedValue?.ToString()))
            {
                MessageBox.Show("请先选择属性集", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _selectedPropertySetInternalName = cmbPropertySet.SelectedValue.ToString();

            // 校验文件
            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                MessageBox.Show("请先选择 Excel 文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 读取 Excel
                var rows = ExcelHelper.ReadMetaPropertyImport(_selectedFilePath);
                if (rows.Count == 0)
                {
                    MessageBox.Show("Excel 中没有数据行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 校验 A 列（仅提示，不阻断预览）
                var invalidRows = new List<string>();
                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.InternalName))
                        invalidRows.Add(string.Format("第 {0} 行：属性内部名为空", row.RowNumber));
                    else if (ContainsChinese(row.InternalName))
                        invalidRows.Add(string.Format("第 {0} 行：属性内部名 [{1}] 包含中文字符", row.RowNumber, row.InternalName));
                    else if (row.InternalName.Contains(" "))
                        invalidRows.Add(string.Format("第 {0} 行：属性内部名 [{1}] 包含空格，不允许", row.RowNumber, row.InternalName));
                }
                if (invalidRows.Count > 0)
                {
                    var errMsg = string.Format("以下 {0} 行校验失败（预览仍可继续）：{1}{2}",
                        invalidRows.Count, Environment.NewLine, string.Join(Environment.NewLine, invalidRows));
                    MessageBox.Show(errMsg, "校验提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // 调用预览（只读数据库）
                var preview = _db.PreviewMetaPropertyImport(rows);

                // 显示预览窗口（支持冲突解决 + 直接导入）
                using (var previewForm = new MetaPropertyPreviewForm(preview, _db, _selectedPropertySetInternalName, rows))
                {
                    previewForm.ShowDialog(this);
                    if (previewForm.Confirmed)
                    {
                        LogMessage(new string('─', 60));
                        LogMessage("[信息] 导入已从预览窗体完成。");
                        LogMessage(new string('─', 60));

                        // 记录导入详情日志
                        var detail = new ImportDetail();
                        detail.FilePath = _selectedFilePath ?? string.Empty;
                        detail.Parameters = "属性集：" + _selectedPropertySetInternalName;
                        detail.SuccessCount = preview != null ? preview.WillInsert + preview.WillUpdate : 0;
                        detail.FailCount = preview != null ? preview.WillSkip : 0;
                        detail.TotalCount = preview != null ? preview.TotalRows : 0;
                        detail.Cancelled = false;
                        if (preview != null && preview.Rows != null)
                        {
                            foreach (var pr in preview.Rows)
                            {
                                if (pr.Action == "新增")
                                    detail.Changes.Add("新增元模型属性：" + pr.InternalName + " (" + (pr.NewZhCn ?? "") + ")");
                                else if (pr.Action == "更新")
                                    detail.Changes.Add("更新元模型属性：" + pr.InternalName + " [" + (pr.Changes ?? "") + "]");
                                else if (pr.Action == "跳过")
                                    detail.Changes.Add("跳过元模型属性：" + pr.InternalName);
                            }
                        }
                        Logger.Instance.LogImport("元模型属性导入",
                            string.Format("元模型属性导入[属性集:{0}]：新增{1}，更新{2}，跳过{3}",
                                _selectedPropertySetInternalName,
                                preview != null ? preview.WillInsert : 0,
                                preview != null ? preview.WillUpdate : 0,
                                preview != null ? preview.WillSkip : 0),
                            detail);
                    }
                    else
                    {
                        LogMessage("[信息] 用户取消了导入操作");
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(string.Format("[错误] 预览/导入失败：{0}", ex.Message));
                Logger.Instance.LogError("元模型属性导入", "预览/导入失败：" + ex.Message);
                MessageBox.Show(string.Format("预览/导入失败：{0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
