using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DataImportTool
{
    /// <summary>
    /// 分类插入窗口（自己跑完整流程，不依赖 MainForm）
    /// 新流程：选择文件 → 解析数据 → 预览 → 用户确认 → 执行导入 → 结果展示
    /// Designer.cs 提供所有 UI 控件，.cs 仅负责业务逻辑
    /// </summary>
    public partial class InsertModeForm : Form
    {
        // ====== 业务字段 ======
        private readonly DbHelper _db;
        private readonly AppConfig _config;
        private CancellationTokenSource _cts;

        // PRD v2.2：表单生成配置（缓存所选模板的 UI content）
        private string _selectedTemplateUiContent;
        private string _pendingFile; // 待导入文件路径
        private List<KeyValuePair<int, string>> _pendingRows; // 待导入数据

        // ====== 构造 ======
        /// <summary>
        /// 无参构造函数：供 VS 设计器实例化
        /// </summary>
        public InsertModeForm()
        {
            InitializeComponent();
        }

        public InsertModeForm(DbHelper db, AppConfig config)
        {
            _db = db;
            _config = config;
            InitializeComponent();
            InitControls();        // 业务相关初始化（仅事件绑定 + 数据绑定）
            LoadClassificationGroups();
        }

        /// <summary>
        /// 仅事件绑定和业务数据初始化（UI 已在 Designer.cs 创建）
        /// </summary>
        private void InitControls()
        {
            // 动态提示（依赖 _config）
            string nameCol = _config?.NameColumn ?? "Name";
            _lblHint.Text = string.Format("名称列默认使用「{0}」（可在设置中修改）", nameCol);

            // 插入模式切换（已在 Designer.cs 绑定 OnModeChanged，这里仅初始化默认状态）
            OnModeChanged(null, EventArgs.Empty);

            // 表单生成模式（PRD v2.2）
            _rbNoForm.CheckedChanged += OnFormGenModeChanged;
            _rbGenForm.CheckedChanged += OnFormGenModeChanged;
            _cmbTemplate.SelectedIndexChanged += OnTemplateSelected;
            _rbNoForm.Checked = true;
            OnFormGenModeChanged(null, EventArgs.Empty);

        }

        private void OnModeChanged(object sender, EventArgs e)
        {
            bool isGroupMode = _rbGroup.Checked;
            _cmbGroup.Enabled = isGroupMode;
            _lblGroupHint.Enabled = isGroupMode;
        }

        // ====== PRD v2.2：表单生成配置 ======
        private void OnFormGenModeChanged(object sender, EventArgs e)
        {
            bool genForm = _rbGenForm.Checked;
            _cmbTemplate.Enabled = genForm;
            _lblTemplate.Enabled = genForm;
            if (genForm)
            {
                LoadFormTemplates();
            }
            else
            {
                _cmbTemplate.DataSource = null;
                _cmbTemplate.Items.Clear();
                _selectedTemplateUiContent = null;
            }
        }

        private void LoadFormTemplates()
        {
            try
            {
                var list = _db.GetFormTemplateList();
                if (list == null || list.Count == 0)
                {
                    MessageBox.Show("未查询到可用的分类表单模板（plm_config_v2.form_meta_template_definition，class_internal_name='part'）。",
                        "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _cmbTemplate.DataSource = null;
                    _cmbTemplate.Items.Clear();
                    _selectedTemplateUiContent = null;
                    return;
                }
                _cmbTemplate.DataSource = list;
                _cmbTemplate.DisplayMember = "DisplayName";
                _cmbTemplate.ValueMember = "Id";
                _cmbTemplate.SelectedIndex = 0; // 触发 OnTemplateSelected 缓存 content
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载表单模板失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnTemplateSelected(object sender, EventArgs e)
        {
            var item = _cmbTemplate.SelectedItem as FormTemplateInfo;
            if (item == null)
            {
                _selectedTemplateUiContent = null;
                return;
            }
            try
            {
                _selectedTemplateUiContent = _db.GetFormTemplateUiContent(item.Id);
            }
            catch (Exception ex)
            {
                _selectedTemplateUiContent = string.Empty;
                MessageBox.Show("读取模板 UI 内容失败：" + ex.Message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ====== 加载分类组列表 ======
        private void LoadClassificationGroups()
        {
            try
            {
                var groups = _db.GetAllClassificationGroups();
                _cmbGroup.DataSource = groups;
                _cmbGroup.DisplayMember = "Name";
                _cmbGroup.ValueMember = "ObjectId";
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载分类组失败：" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====== 选择文件 ======
        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Excel 文件|*.xlsx;*.xls|所有文件|*.*";
                dialog.Title = "选择分类 Excel 文件";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _pendingFile = dialog.FileName;
                 

                    // 解析数据并预览
                    ParseAndPreview();
                }
            }
        }

        /// <summary>
        /// 解析 Excel 数据并显示预览窗体
        /// </summary>
        private void ParseAndPreview()
        {
            _txtErrors.Clear();
            _lblResult.Text = string.Empty;
            _progressBar.Value = 0;
            _lblProgress.Text = "正在读取 Excel...";

            string nameCol = _config?.NameColumn ?? "Name";

            try
            {
                _pendingRows = ExcelHelper.ReadNameColumn(_pendingFile, nameCol);

                if (_pendingRows == null || _pendingRows.Count == 0)
                {
                    _lblResult.Text = "Excel 中无数据";
                    _lblResult.ForeColor = Color.FromArgb(220, 53, 69);
                    _txtErrors.Text = "Excel 文件中没有有效数据行。";
                    LogErr("Excel 中无数据。");
                    return;
                }

                _lblProgress.Text = string.Format("已读取 {0} 行数据", _pendingRows.Count);
                LogInfo(string.Format("共 {0} 行数据待导入。", _pendingRows.Count));

                // 预校验数据
                var errors = ValidateRows(_pendingRows);

                // 构建预览节点
                var nodes = ImportPreviewHelper.BuildClassificationInsertPreview(_pendingRows);

                // 获取目标位置描述
                string targetDesc = GetTargetDescription();

                // 显示预览窗体
                string title = string.Format("分类导入预览 - {0}", targetDesc);
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
                        _lblResult.Text = "用户取消导入";
                        _lblResult.ForeColor = Color.FromArgb(108, 117, 125);
                        _txtErrors.Text = "用户在预览窗口取消导入操作。";
                    }
                }
            }
            catch (Exception ex)
            {
                _lblResult.Text = "读取失败：" + ex.Message;
                _lblResult.ForeColor = Color.FromArgb(220, 53, 69);
                _txtErrors.Text = ex.ToString();
                LogErr("[读取失败] " + ex.Message);
            }
        }

        /// <summary>
        /// 获取目标位置描述
        /// </summary>
        private string GetTargetDescription()
        {
            if (_rbRoot.Checked)
            {
                return "根节点";
            }
            else if (_cmbGroup.SelectedItem != null)
            {
                var grp = _cmbGroup.SelectedItem as ClassificationGroup;
                return grp != null ? string.Format("分类组：{0}", grp.Name) : "分类组";
            }
            return "未指定";
        }

        /// <summary>
        /// 预校验数据行
        /// </summary>
        private List<ImportError> ValidateRows(List<KeyValuePair<int, string>> rows)
        {
            var errors = new List<ImportError>();

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Value))
                {
                    errors.Add(new ImportError { RowNumber = row.Key, Name = row.Value, Message = "名称为空" });
                }
            }

            return errors;
        }

        /// <summary>
        /// 执行实际导入操作
        /// </summary>
        private void ExecuteImport()
        {
            long? groupId = null;
            bool generateForm = _rbGenForm.Checked;
            if (generateForm && _cmbTemplate.SelectedValue == null)
            {
                MessageBox.Show("请先选择一个表单模板。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_rbGroup.Checked)
            {
                if (_cmbGroup.SelectedValue == null)
                {
                    MessageBox.Show("请先选择一个目标分类组。",
                        "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                groupId = Convert.ToInt64(_cmbGroup.SelectedValue);
            }

            string nameCol = _config?.NameColumn ?? "Name";
          
            _txtErrors.Clear();
            _lblResult.Text = string.Empty;
            _progressBar.Value = 0;
            _lblProgress.Text = "准备中...";

            var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            worker.DoWork += (s, ev) => ev.Result = DoImport(worker, _pendingRows, nameCol, groupId, generateForm, _selectedTemplateUiContent);
            worker.ProgressChanged += (s, ev) =>
            {
                _progressBar.Value = ev.ProgressPercentage;
                _lblProgress.Text = ev.UserState?.ToString() ?? string.Empty;
            };
            worker.RunWorkerCompleted += (s, ev) =>
            {
               
                if (ev.Cancelled)
                {
                    _lblResult.Text = "已取消";
                    _lblResult.ForeColor = Color.FromArgb(220, 53, 69);
                }
                else if (ev.Error != null)
                {
                    _lblResult.Text = "导入失败：" + ev.Error.Message;
                    _lblResult.ForeColor = Color.FromArgb(220, 53, 69);
                }
                else
                {
                    var result = ev.Result as ImportResult;
                    ShowResult(result);
                }
            };

            _cts = new CancellationTokenSource();
            worker.RunWorkerAsync();
        }

        private ImportResult DoImport(BackgroundWorker worker, List<KeyValuePair<int, string>> rows, string nameCol, long? groupId, bool generateForm, string templateUiContent)
        {
            var result = new ImportResult();
            _cts = new CancellationTokenSource();

            string target = groupId.HasValue
                ? string.Format("分类组ID={0}", groupId.Value)
                : "根节点";

            LogInfo(string.Format("===== 分类导入开始：目标={0}，文件={1} =====", target, _pendingFile));
            worker.ReportProgress(0, "正在导入...");

            for (int i = 0; i < rows.Count; i++)
            {
                if (worker.CancellationPending) break;

                int rowNum = rows[i].Key;
                string name = rows[i].Value;
                int pct = (int)((double)(i + 1) / rows.Count * 100);
                worker.ReportProgress(pct, string.Format("{0}/{1}", i + 1, rows.Count));

                if (string.IsNullOrEmpty(name))
                {
                    result.FailCount++;
                    result.Errors.Add(new ImportError { RowNumber = rowNum, Name = name, Message = "名称为空" });
                    continue;
                }

                try
                {
                    var insRes = _db.InsertClassificationWithForm(name, groupId, generateForm, templateUiContent);
                    result.SuccessCount++;
                    string logMsg = string.Format("[成功] 行 {0}：{1}（OBJECT_ID={2}）", rowNum, name, insRes.ClsId);
                    if (insRes.FormSkipped)
                        logMsg += "（" + insRes.FormMessage + "）";
                    LogOk(logMsg);
                }
                catch (Exception ex)
                {
                    result.FailCount++;
                    result.Errors.Add(new ImportError { RowNumber = rowNum, Name = name, Message = ex.Message });
                    LogErr(string.Format("[失败] 行 {0}：{1} - {2}", rowNum, name, ex.Message));
                }
            }

            worker.ReportProgress(100, "完成");
            LogInfo(string.Format("===== 分类导入完成：成功={0}，失败={1} =====",
                result.SuccessCount, result.FailCount));

            return result;
        }

        private void ShowResult(ImportResult result)
        {
            if (result == null)
            {
                _lblResult.Text = "导入结果获取失败（未返回结果对象）";
                _lblResult.ForeColor = Color.FromArgb(220, 53, 69);
                return;
            }

            _lblResult.Text = result.Summary;
            _lblResult.ForeColor = result.FailCount > 0
                ? Color.FromArgb(220, 53, 69)
                : Color.FromArgb(40, 167, 69);

            var sb = new StringBuilder();
            sb.AppendLine(result.Summary);
            if (result.Errors.Count > 0)
            {
                sb.AppendLine("失败详情：");
                foreach (var err in result.Errors)
                    sb.AppendLine(string.Format("  行 {0} [{1}]：{2}",
                        err.RowNumber, err.Name ?? "(空)", err.Message));
            }
            else
            {
                sb.AppendLine("所有数据导入成功！");
            }
            _txtErrors.Text = sb.ToString();
        }

        private const string MODULE_NAME = "分类插入";

        private void LogInfo(string msg) { Logger.Instance.Log(MODULE_NAME, msg); }
        private void LogOk(string msg) { Logger.Instance.LogSuccess(MODULE_NAME, msg); }
        private void LogErr(string msg) { Logger.Instance.LogError(MODULE_NAME, msg); }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
                _cts.Cancel();
            base.OnFormClosing(e);
        }
    }
}
