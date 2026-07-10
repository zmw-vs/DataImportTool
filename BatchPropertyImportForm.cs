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
    /// <summary>
    /// 批量导入属性值（物品维度）：
    /// 给定分类下的 N 个属性，每个属性 2 列（PROPERTY_VALUE + PROPERTY_NUMBER），
    /// Excel 每行 = 一个物品，所有属性值并行填入。
    /// 写入：t_property_value + t_property_to_property_value
    /// 
    /// 新流程：选择文件 → 解析数据 → 预览 → 用户确认 → 执行导入 → 结果展示
    /// </summary>
    public partial class BatchPropertyImportForm : Form
    {
        private readonly DbHelper _db;
        private CancellationTokenSource _cts;
        private string _selectedFilePath;

        // 当前选中分类
        private ClassificationGroup _selectedClassification;
        private bool _isRoot;

        // 当前分类下的属性名列表（由 RefreshPropertyList 填充，供 Excel 导入使用）
        private List<string> _propertyNames;

        // 待导入数据
        private List<PropertyBatchRow> _pendingRows;
        private Dictionary<string, long> _pendingPropDict;
        private string _pendingClsName;

        public BatchPropertyImportForm()
        {
            InitializeComponent();
            InitControls();
        }

        public BatchPropertyImportForm(DbHelper db)
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
                LoadClassifications();
            }
        }

        private void InitControls()
        {
            _cmbClassification.SelectedIndexChanged += CmbClassification_SelectedIndexChanged;
            _btnSelectExcel.Click += BtnSelectExcel_Click;
            _btnGenerateTemplate.Click += BtnGenerateTemplate_Click;
            _btnCancel.Click += BtnCancel_Click;
        }

        private void LoadClassifications()
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

        private void CmbClassification_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbClassification.SelectedItem == null) return;
            var item = _cmbClassification.SelectedItem as ClassificationGroup;
            if (item == null) return;

            _selectedClassification = item;
            _isRoot = item.ObjectId == 0;
            RefreshPropertyList();
        }

        private void RefreshPropertyList()
        {
            _txtPropertyList.Text = "正在加载属性列表...";
            _lblPropertyCount.Text = "正在加载...";

            if (_db == null)
            {
                _lblPropertyCount.Text = "数据库未连接，请先关闭此窗口并检查连接。";
                _txtPropertyList.Text = string.Empty;
                return;
            }
            if (_selectedClassification == null)
            {
                _txtPropertyList.Text = "（请在上方选择一个分类）";
                return;
            }

            try
            {
                var dict = _db.GetPropertyDictionaryByClassification(_selectedClassification.ObjectId, _isRoot);
                _propertyNames = dict.Keys.OrderBy(n => n).ToList();

                if (_propertyNames.Count == 0)
                {
                    _lblPropertyCount.Text = "该分类下暂无属性，请先在「插入属性」中添加属性。";
                    _txtPropertyList.Text = "（无属性，请先添加属性后再进行导入）";
                    return;
                }

                _lblPropertyCount.Text = string.Format(
                    "该分类下共 {0} 个属性：", _propertyNames.Count);

                // 仅显示属性名列表，每行一个
                var sb = new StringBuilder();
                for (int i = 0; i < _propertyNames.Count; i++)
                {
                    sb.AppendFormat("{0}. {1}", i + 1, _propertyNames[i]);
                    sb.AppendLine();
                }

                _txtPropertyList.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                _lblPropertyCount.Text = "加载属性列表失败：" + ex.Message;
                _txtPropertyList.Text = ex.ToString();
            }
        }

        // ====== 选择 Excel 并导入 ======
        private void BtnSelectExcel_Click(object sender, EventArgs e)
        {
            if (_selectedClassification == null)
            {
                MessageBox.Show("请先选择一个分类。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Excel 文件|*.xlsx|所有文件|*.*";
                dialog.Title = "选择批量属性值 Excel";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                // 读取属性字典
                Dictionary<string, long> propDict;
                try
                {
                    propDict = _db.GetPropertyDictionaryByClassification(
                        _selectedClassification.ObjectId, _isRoot);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("加载分类属性失败：" + ex.Message,
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (propDict.Count == 0)
                {
                    MessageBox.Show("该分类下暂无属性，请先在「插入属性」中添加属性。",
                        "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 读取 Excel
                List<PropertyBatchRow> rows;
                try
                {
                    rows = ExcelHelper.ReadBatchPropertyImport(dialog.FileName, propDict.Keys.ToList());
                    if (rows.Count == 0)
                    {
                        MessageBox.Show("Excel 中无有效数据行（第1行为属性名标题，数据从第2行起）。",
                            "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("读取 Excel 失败：" + ex.Message,
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string clsName = _isRoot ? "根目录" : _selectedClassification.Name;
                _selectedFilePath = dialog.FileName;

                // 保存待导入数据
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
            _lblStatus.Text = string.Format("已读取 {0} 个物品数据", _pendingRows.Count);

            // 预校验数据
            var errors = ValidateRows(_pendingRows);

            // 构建预览节点
            var nodes = ImportPreviewHelper.BuildPropertyImportPreview(_pendingClsName, _pendingRows);

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
                }
            }
        }

        /// <summary>
        /// 预校验数据行
        /// </summary>
        private List<ImportError> ValidateRows(List<PropertyBatchRow> rows)
        {
            var errors = new List<ImportError>();

            foreach (var row in rows)
            {
                // 检查是否有属性值
                if (row.PropertyValues == null || row.PropertyValues.Count == 0)
                {
                    errors.Add(new ImportError { RowNumber = row.RowNumber, Name = row.ItemName ?? "(空)", Message = "该行无属性值数据" });
                }
            }

            return errors;
        }

        private void StartImport(List<PropertyBatchRow> rows, Dictionary<string, long> propDict, string clsName)
        {
            var result = new ImportResult();
            _cts = new CancellationTokenSource();

            SetControlsEnabled(false);
            _lblStatus.Text = string.Format("正在导入 {0} 个物品的属性值...", rows.Count);
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = 0;
            _lblResult.Text = "成功：0  失败：0";
            _lblResult.ForeColor = Color.FromArgb(33, 37, 41);
            _txtErrors.Text = "正在处理，请稍候...";

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    for (int i = 0; i < rows.Count; i++)
                    {
                        if (_cts.Token.IsCancellationRequested) break;

                        var row = rows[i];
                        int pct = rows.Count <= 0 ? 0 : (i + 1) * 100 / rows.Count;
                        UpdateProgress(pct, i + 1, rows.Count);
                        UpdateStatus(string.Format("处理物品 [{0}]（{1}/{2}）",
                            string.IsNullOrEmpty(row.ItemName) ? "(空)" : row.ItemName,
                            i + 1, rows.Count));

                        // 遍历该行所有属性值，逐条写入
                        foreach (var kvp in row.PropertyValues)
                        {
                            string propName = kvp.Key;
                            string propVal = kvp.Value ?? string.Empty;

                            // 跳过空值（Excel中该属性在此行无数据）
                            if (string.IsNullOrEmpty(propVal)) continue;

                            if (!propDict.TryGetValue(propName, out long propId))
                            {
                                // 该属性在字典中不存在，跳过（已由 ExcelHelper 过滤）
                                continue;
                            }

                            // 查找 PROPERTY_NUMBER（同属性同编号，找同名属性值中的 NUMBER）
                            string propNum = row.PropertyNumbers.ContainsKey(propName)
                                ? row.PropertyNumbers[propName] ?? string.Empty
                                : string.Empty;

                            try
                            {
                                long pvId = _db.InsertPropertyValue(propNum, propVal);
                                _db.InsertPropertyToPropertyValue(propId, pvId);
                                lock (result.Errors)
                                {
                                    result.SuccessCount++;
                                    result.Changes.Add(string.Format("新增属性值：[{0}] {1} = {2}",
                                        row.ItemName ?? "(空)", propName, propVal));
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
                                        Name = propName + "=" + propVal,
                                        Message = ex.Message
                                    });
                                }
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
                sb.AppendLine(string.Format("分类 [{0}] 批量属性值导入结果：{1}", clsName, result.Summary));
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
                string logMsg = string.Format("批量属性导入 [{0}]：成功 {1}，失败 {2}",
                    clsName, result.SuccessCount, result.FailCount);
                Logger.Instance.LogImport("批量属性导入", logMsg, detail);
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            _btnSelectExcel.Enabled = enabled;
            _btnGenerateTemplate.Enabled = enabled;
            _cmbClassification.Enabled = enabled;
            _btnCancel.Text = enabled ? "关闭" : "取消";
        }

        private void EnableControls()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(EnableControls));
            else
                SetControlsEnabled(true);
        }

        // ====== 生成 Excel 模板 ======
        private void BtnGenerateTemplate_Click(object sender, EventArgs e)
        {
            if (_selectedClassification == null)
            {
                MessageBox.Show("请先选择一个分类。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Dictionary<string, long> propDict;
            try
            {
                propDict = _db.GetPropertyDictionaryByClassification(
                    _selectedClassification.ObjectId, _isRoot);
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载分类属性失败：" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (propDict.Count == 0)
            {
                MessageBox.Show("该分类下暂无属性，无法生成模板。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel 文件|*.xlsx";
                dialog.Title = "保存 Excel 模板";
                dialog.FileName = string.Format("批量属性值导入_模板_{0}.xlsx",
                    _isRoot ? "根目录" : _selectedClassification.Name);
                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var propNames = propDict.Keys.OrderBy(n => n).ToList();
                    ExcelHelper.GenerateBatchPropertyImportTemplate(dialog.FileName, propNames);
                    MessageBox.Show(string.Format("模板已生成：\n{0}\n\n请填写数据后使用「选择 Excel 并导入」。",
                        dialog.FileName),
                        "生成成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("生成模板失败：" + ex.Message,
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
    }
}
