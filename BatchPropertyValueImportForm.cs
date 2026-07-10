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
    public partial class BatchPropertyValueImportForm : Form
    {
        private readonly DbHelper _db;
        private CancellationTokenSource _cts;
        private string _selectedFilePath;

        // 待导入数据
        private List<PropertyValueBatchRow> _pendingRows;
        private Dictionary<string, long> _pendingPropDict;
        private string _pendingClsName;

        public BatchPropertyValueImportForm()
        {
            InitializeComponent();
            InitControls();
        }

        public BatchPropertyValueImportForm(DbHelper db)
        {
            _db = db;
            InitializeComponent();
            InitControls();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                LoadAllClassifications();
            }
        }

        private void InitControls()
        {
            _btnCancel.Click += BtnCancel_Click;
            _btnSelectExcel.Click += BtnSelectExcel_Click;
            _cmbClassification.SelectedIndexChanged += CmbClassification_SelectedIndexChanged;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _lblStatus.Text = "正在取消...";
            }
            else
            {
                this.Close();
            }
        }

        /// <summary>加载所有分类到下拉框（含根目录）</summary>
        private void LoadAllClassifications()
        {
            try
            {
                var list = _db.GetAllClassifications();
                var allItems = new List<ClassificationGroup>();
                allItems.Add(new ClassificationGroup { ObjectId = 0, Name = "根目录" });
                allItems.AddRange(list);

                _cmbClassification.DataSource = allItems;
                _cmbClassification.DisplayMember = "Name";
                _cmbClassification.ValueMember = "ObjectId";
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载分类列表失败：" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>分类切换时刷新属性列表</summary>
        private void CmbClassification_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbClassification.SelectedItem == null) return;
            var item = _cmbClassification.SelectedItem as ClassificationGroup;
            if (item == null) return;

            bool isRoot = item.ObjectId == 0;
            long clsId = item.ObjectId;

            try
            {
                var dict = _db.GetPropertyDictionaryByClassification(clsId, isRoot);
                var names = dict.Keys.OrderBy(n => n).ToList();

                if (names.Count == 0)
                {
                    _lblPropertyCount.Text = "该分类下暂无属性，请先在「插入属性」中添加属性。";
                    _txtPropertyList.Text = string.Empty;
                }
                else
                {
                    _lblPropertyCount.Text = string.Format("该分类下共 {0} 个属性：", names.Count);
                    var sb = new StringBuilder();
                    foreach (var name in names)
                        sb.AppendLine("  - " + name);
                    _txtPropertyList.Text = sb.ToString().TrimEnd();
                }
            }
            catch (Exception ex)
            {
                _lblPropertyCount.Text = "加载属性列表失败：" + ex.Message;
                _txtPropertyList.Text = string.Empty;
            }
        }

        private void BtnSelectExcel_Click(object sender, EventArgs e)
        {
            if (_cmbClassification.SelectedItem == null)
            {
                MessageBox.Show("请先选择一个分类。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var item = _cmbClassification.SelectedItem as ClassificationGroup;
            bool isRoot = item.ObjectId == 0;
            long clsId = item.ObjectId;
            string clsName = isRoot ? "根目录" : item.Name;

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Excel 文件|*.xlsx|所有文件|*.*";
                dialog.Title = "选择批量属性值 Excel";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                // 读取 Excel
                List<PropertyValueBatchRow> rows;
                try
                {
                    rows = ExcelHelper.ReadPropertyValueBatchColumns(dialog.FileName);
                    if (rows.Count == 0)
                    {
                        MessageBox.Show("Excel 中无有效数据行。", "提示",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("读取 Excel 失败：" + ex.Message,
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 获取该分类的属性字典
                Dictionary<string, long> propDict;
                try
                {
                    propDict = _db.GetPropertyDictionaryByClassification(clsId, isRoot);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("加载分类属性失败：" + ex.Message,
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 保存待导入数据
                _selectedFilePath = dialog.FileName;
                _pendingRows = rows;
                _pendingPropDict = propDict;
                _pendingClsName = clsName;

                // 解析并预览
                ParseAndPreview();
            }
        }

        /// <summary>
        /// 解析数据并显示预览窗体
        /// </summary>
        private void ParseAndPreview()
        {
            _lblStatus.Text = string.Format("已读取 {0} 条属性值数据", _pendingRows.Count);

            // 预校验数据
            var errors = ValidateRows(_pendingRows, _pendingPropDict);

            // 构建预览节点
            var nodes = ImportPreviewHelper.BuildPropertyValueImportPreview(_pendingClsName, _pendingRows);

            // 显示预览窗体
            string title = string.Format("批量属性值导入预览 - {0}", _pendingClsName);
            using (var previewForm = new ImportPreviewForm(nodes, errors, title))
            {
                previewForm.ShowDialog(this);

                if (previewForm.Confirmed)
                {
                    // 用户确认，执行导入
                    StartImport(_pendingRows, _pendingPropDict, _pendingClsName);
                }
                else
                {
                    _lblStatus.Text = "用户取消导入";
                    _lblResult.Text = "成功：0  失败：0";
                    _txtErrors.Text = "用户在预览窗口取消导入操作。";
                    _txtErrors.Visible = true;
                }
            }
        }

        /// <summary>
        /// 预校验数据行
        /// </summary>
        private List<ImportError> ValidateRows(List<PropertyValueBatchRow> rows, Dictionary<string, long> propDict)
        {
            var errors = new List<ImportError>();

            foreach (var row in rows)
            {
                // 检查属性名称
                if (string.IsNullOrWhiteSpace(row.PropertyName))
                {
                    errors.Add(new ImportError { RowNumber = row.RowNumber, Name = row.PropertyName, Message = "属性名称为空" });
                    continue;
                }

                // 检查属性值
                if (string.IsNullOrWhiteSpace(row.PropertyValue))
                {
                    errors.Add(new ImportError { RowNumber = row.RowNumber, Name = row.PropertyName, Message = "属性值为空" });
                    continue;
                }

                // 检查属性是否存在于当前分类
                if (!propDict.ContainsKey(row.PropertyName))
                {
                    errors.Add(new ImportError { RowNumber = row.RowNumber, Name = row.PropertyName, Message = "该属性在当前分类下不存在" });
                }
            }

            return errors;
        }

        private void StartImport(List<PropertyValueBatchRow> rows, Dictionary<string, long> propDict, string clsName)
        {
            var result = new ImportResult();
            _cts = new CancellationTokenSource();

            _btnSelectExcel.Enabled = false;
            _btnCancel.Text = "取消";
            _lblStatus.Text = string.Format("正在导入 {0} 条记录...", rows.Count);
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = 0;
            _lblResult.Text = "成功：0  失败：0";
            _lblResult.ForeColor = Color.FromArgb(33, 37, 41);
            _txtErrors.Text = "正在处理，请稍候...";
            _txtErrors.Visible = true;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    for (int i = 0; i < rows.Count; i++)
                    {
                        if (_cts.Token.IsCancellationRequested) break;

                        var row = rows[i];
                        int current = i + 1;
                        int pct = rows.Count <= 0 ? 0 : current * 100 / rows.Count;

                        UpdateProgress(pct, current, rows.Count);
                        UpdateStatus(string.Format("处理行 {0}：{1} = {2}",
                            row.RowNumber, row.PropertyName, row.PropertyValue));

                        // 属性名称匹配
                        if (!propDict.TryGetValue(row.PropertyName, out long propId))
                        {
                            lock (result.Errors)
                            {
                                result.FailCount++;
                                result.Errors.Add(new ImportError
                                {
                                    RowNumber = row.RowNumber,
                                    Name = row.PropertyName,
                                    Message = "未在当前分类下找到属性 [" + row.PropertyName + "]"
                                });
                            }
                            continue;
                        }

                        // 单行事务：插入 property_value + 关系表
                        try
                        {
                            long pvId = _db.InsertPropertyValue(row.PropertyNumber, row.PropertyValue);
                            _db.InsertPropertyToPropertyValue(propId, pvId);
                            lock (result.Errors)
                            {
                                result.SuccessCount++;
                                result.Changes.Add(string.Format("新增属性值：{0} = {1} (编号:{2})",
                                    row.PropertyName, row.PropertyValue, row.PropertyNumber ?? "(无)"));
                            }
                        }
                        catch (Exception ex)
                        {
                            lock (result.Errors)
                            {
                                result.FailCount++;
                                result.Errors.Add(new ImportError
                                {
                                    RowNumber = row.RowNumber,
                                    Name = row.PropertyName,
                                    Message = ex.Message
                                });
                            }
                        }
                    }

                    if (_cts.Token.IsCancellationRequested)
                    {
                        UpdateStatus("用户已取消导入");
                        UpdateProgress(0, 0, 0);
                        UpdateResult(result, clsName, true);
                    }
                    else
                    {
                        UpdateStatus("导入完成");
                        UpdateProgress(100, rows.Count, rows.Count);
                        UpdateResult(result, clsName, false);
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus("严重错误：" + ex.Message);
                }
                finally
                {
                    EnableControls();
                }
            });
        }

        private void UpdateProgress(int pct, int current, int total)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(() => UpdateProgress(pct, current, total)));
            else
            {
                _progressBar.Value = pct;
                _lblProgress.Text = string.Format("{0}/{1}", current, total);
            }
        }

        private void UpdateStatus(string msg)
        {
            if (_lblStatus.InvokeRequired)
                _lblStatus.Invoke(new Action(() => _lblStatus.Text = msg));
            else
                _lblStatus.Text = msg;
        }

        private void UpdateResult(ImportResult result, string clsName, bool cancelled)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(() => UpdateResult(result, clsName, cancelled)));
            else
            {
                _lblResult.Text = result.Summary;
                _lblResult.ForeColor = result.FailCount > 0
                    ? Color.FromArgb(220, 53, 69)
                    : Color.FromArgb(40, 167, 69);

                var sb = new StringBuilder();
                sb.AppendLine(string.Format("分类 [{0}] 的批量属性值导入结果：{1}", clsName, result.Summary));
                if (cancelled)
                    sb.AppendLine("（用户已取消，部分记录可能未处理）");
                sb.AppendLine(new string('=', 50));

                if (result.Errors.Count > 0)
                {
                    sb.AppendLine("失败详情：");
                    foreach (var err in result.Errors)
                        sb.AppendLine("  " + err.ToString());
                }
                else
                {
                    sb.AppendLine("所有属性值导入成功！");
                }

                _txtErrors.Text = sb.ToString();

                // 记录导入日志
                var detail = new ImportDetail();
                detail.FilePath = _selectedFilePath ?? string.Empty;
                detail.Parameters = "分类：" + clsName;
                detail.SuccessCount = result.SuccessCount;
                detail.FailCount = result.FailCount;
                detail.TotalCount = result.SuccessCount + result.FailCount;
                detail.Cancelled = cancelled;
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
                string logMsg = string.Format("批量属性值导入 [{0}]：成功 {1}，失败 {2}",
                    clsName, result.SuccessCount, result.FailCount);
                Logger.Instance.LogImport("批量属性值导入", logMsg, detail);
            }
        }

        private void EnableControls()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(() => EnableControls()));
            else
            {
                _btnSelectExcel.Enabled = true;
                _btnCancel.Text = "关闭";
            }
        }
    }
}
