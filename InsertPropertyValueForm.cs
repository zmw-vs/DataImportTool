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
    public partial class InsertPropertyValueForm : Form
    {
        private readonly DbHelper _db;
        private CancellationTokenSource _cts;
        private string _selectedFilePath;

        // 待导入数据
        private List<PropertyValueRow> _pendingRows;
        private long _pendingPropId;
        private string _pendingPropName;

        /// <summary>
        /// 无参构造函数：供 VS 设计器实例化（设计时不连数据库）
        /// </summary>
        public InsertPropertyValueForm()
        {
            InitializeComponent(); // Designer.cs
            InitControls();        // 程序化创建所有控件
        }

        public InsertPropertyValueForm(DbHelper db)
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
                // 默认进入根目录模式，加载根目录属性
                LoadRootProperties();
            }
        }

        /// <summary>
        /// 仅事件绑定（UI 已在 Designer.cs 创建）
        /// </summary>
        private void InitControls()
        {
            _btnCancel.Click += BtnCancel_Click;
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

        private void _rbRoot_CheckedChanged(object sender, EventArgs e)
        {
            if (_rbRoot.Checked)
            {
                _lblClassification.Visible = false;
                _cmbClassification.Visible = false;
                _cmbClassification.Enabled = false;
                LoadRootProperties();
            }
        }

        private void _rbClassification_CheckedChanged(object sender, EventArgs e)
        {
            if (_rbClassification.Checked)
            {
                _lblClassification.Visible = true;
                _cmbClassification.Visible = true;
                _cmbClassification.Enabled = true;
                _cmbProperty.DataSource = null;
                _cmbProperty.Items.Clear();
                LoadAllClassifications();
            }
        }

        private void _cmbClassification_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_rbClassification.Checked && _cmbClassification.SelectedItem != null)
            {
                var cls = _cmbClassification.SelectedItem as ClassificationGroup;
                if (cls != null)
                {
                    LoadPropertiesByClassification(cls.ObjectId);
                }
            }
        }

        /// <summary>加载所有分类到 cmbClassification（分类模式下调用）</summary>
        private void LoadAllClassifications()
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

        /// <summary>加载根目录属性（根模式调用）</summary>
        private void LoadRootProperties()
        {
            try
            {
                var list = _db.GetRootProperties();
                _cmbProperty.DataSource = list;
                _cmbProperty.DisplayMember = "Name";
                _cmbProperty.ValueMember = "ObjectId";
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载根目录属性失败：" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>根据分类 ID 加载该分类下的属性（分类模式调用）</summary>
        private void LoadPropertiesByClassification(long clsId)
        {
            try
            {
                var list = _db.GetPropertiesByClassificationId(clsId);
                _cmbProperty.DataSource = list;
                _cmbProperty.DisplayMember = "Name";
                _cmbProperty.ValueMember = "ObjectId";

                if (list.Count == 0)
                {
                    _txtErrors.Text = string.Format("分类 [{0}] 下暂无属性，请先在「插入属性」功能中添加。", _cmbClassification.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载属性列表失败：" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            if (_cmbProperty.SelectedValue == null)
            {
                MessageBox.Show("请先选择一个属性。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            long propId = Convert.ToInt64(_cmbProperty.SelectedValue);
            string propName = _cmbProperty.Text;

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Excel 文件|*.xlsx|所有文件|*.*";
                dialog.Title = "选择属性值 Excel";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    _selectedFilePath = dialog.FileName;
                    var rows = ExcelHelper.ReadPropertyValueColumns(dialog.FileName);
                    if (rows.Count == 0)
                    {
                        MessageBox.Show("Excel 中无数据。",
                            "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 保存待导入数据
                    _pendingRows = rows;
                    _pendingPropId = propId;
                    _pendingPropName = propName;

                    // 解析并预览
                    ParseAndPreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("读取 Excel 失败：" + ex.Message,
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 解析数据并显示预览窗体
        /// </summary>
        private void ParseAndPreview()
        {
            _lblStatus.Text = string.Format("已读取 {0} 条属性值数据", _pendingRows.Count);

            // 预校验数据
            var errors = ValidateRows(_pendingRows);

            // 构建预览节点
            var nodes = ImportPreviewHelper.BuildSinglePropertyValueImportPreview(_pendingPropName, _pendingRows);

            // 显示预览窗体
            string title = string.Format("属性值导入预览 - {0}", _pendingPropName);
            using (var previewForm = new ImportPreviewForm(nodes, errors, title))
            {
                previewForm.ShowDialog(this);

                if (previewForm.Confirmed)
                {
                    // 用户确认，执行导入
                    ExecuteImport();
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
        private List<ImportError> ValidateRows(List<PropertyValueRow> rows)
        {
            var errors = new List<ImportError>();

            foreach (var row in rows)
            {
                // 检查属性值是否为空
                if (string.IsNullOrWhiteSpace(row.PropertyValue))
                {
                    errors.Add(new ImportError { RowNumber = row.RowNumber, Name = row.PropertyValue, Message = "属性值为空" });
                }
            }

            return errors;
        }

        /// <summary>
        /// 执行实际导入操作
        /// </summary>
        private void ExecuteImport()
        {
            var result = new ImportResult();
            _cts = new CancellationTokenSource();

            _lblStatus.Text = string.Format("正在导入 {0} 条属性值...", _pendingRows.Count);
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = 0;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    for (int i = 0; i < _pendingRows.Count; i++)
                    {
                        if (_cts.Token.IsCancellationRequested) break;

                        var row = _pendingRows[i];
                        SetProgress(i + 1, _pendingRows.Count);
                        SetStatus(string.Format("正在插入：{0}（{1}/{2}）", row.PropertyValue, i + 1, _pendingRows.Count));

                        try
                        {
                            long pvId = _db.InsertPropertyValue(row.PropertyNumber, row.PropertyValue);
                            _db.InsertPropertyToPropertyValue(_pendingPropId, pvId);
                            lock (result.Errors)
                            {
                                result.SuccessCount++;
                                result.Changes.Add(string.Format("新增属性值：{0} = {1} (编号:{2})",
                                    _pendingPropName, row.PropertyValue, row.PropertyNumber ?? "(无)"));
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
                                    Name = row.PropertyValue,
                                    Message = ex.Message
                                });
                            }
                        }
                    }

                    SetProgress(0, 0);
                    SetStatus("导入完成");
                    ShowResultInUI(result, _pendingPropName, _selectedFilePath);
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
                    else { _progressBar.Value = Math.Min(100, current * 100 / total); _lblProgress.Text = string.Format("{0}/{1}", current, total); }
                }));
            else
            {
                if (total <= 0) { _progressBar.Value = 0; _lblProgress.Text = string.Empty; }
                else { _progressBar.Value = Math.Min(100, current * 100 / total); _lblProgress.Text = string.Format("{0}/{1}", current, total); }
            }
        }

        private void SetStatus(string msg)
        {
            if (_lblStatus.InvokeRequired)
                _lblStatus.Invoke(new Action(() => _lblStatus.Text = msg));
            else
                _lblStatus.Text = msg;
        }

        private void ShowResultInUI(ImportResult result, string propName, string filePath)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(() => ShowResultInUI(result, propName, filePath)));
            else
            {
                _lblResult.Text = result.Summary;
                _lblResult.ForeColor = result.FailCount > 0
                    ? Color.FromArgb(220, 53, 69)
                    : Color.FromArgb(40, 167, 69);

                var sb = new StringBuilder();
                sb.AppendLine(string.Format("属性 [{0}] 的属性值：{1}", propName, result.Summary));
                sb.AppendLine(new string('=', 40));

                if (result.Errors.Count > 0)
                {
                    sb.AppendLine("失败详情：");
                    foreach (var err in result.Errors)
                        sb.AppendLine(string.Format("  行 {0} [{1}]：{2}", err.RowNumber, err.Name, err.Message));
                }
                else
                {
                    sb.AppendLine("所有属性值导入成功！");
                }

                _txtErrors.Text = sb.ToString();

                // 记录导入日志
                var detail = new ImportDetail();
                detail.FilePath = filePath ?? string.Empty;
                detail.Parameters = "属性：" + propName;
                detail.SuccessCount = result.SuccessCount;
                detail.FailCount = result.FailCount;
                detail.TotalCount = result.SuccessCount + result.FailCount;
                detail.Cancelled = false;
                if (result.Errors.Count > 0)
                {
                    foreach (var err in result.Errors)
                        detail.Errors.Add(string.Format("行 {0} [{1}]：{2}", err.RowNumber, err.Name, err.Message));
                }
                if (result.Changes != null && result.Changes.Count > 0)
                {
                    foreach (var ch in result.Changes)
                        detail.Changes.Add(ch);
                }
                string logMsg = string.Format("属性值导入 [{0}]：成功 {1}，失败 {2}",
                    propName, result.SuccessCount, result.FailCount);
                Logger.Instance.LogImport("属性值导入", logMsg, detail);
            }
        }
    }
}
