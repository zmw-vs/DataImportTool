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
    /// 分类组/分类混合导入窗口（PRD v2.4）
    /// 新流程：选择文件 → 解析数据 → 预览 → 用户确认 → 执行导入 → 结果展示
    /// Excel 列：节点名称 / 节点类型（分类组|分类）/ 父节点名称
    /// </summary>
    public partial class GroupImportForm : Form
    {
        // ====== 业务字段（非控件）======
        private DbHelper _db;
        private AppConfig _config;
        private CancellationTokenSource _cts;
        private string _selectedFile;
        private List<ClassificationNodeRow> _pendingRows;

        // ====== 构造 ======
        /// <summary>
        /// 无参构造函数：供 VS 设计器实例化（设计时不连数据库）
        /// </summary>
        public GroupImportForm()
        {
            InitializeComponent();
        }

        public GroupImportForm(DbHelper db, AppConfig config)
        {
            _db = db;
            _config = config;
            InitializeComponent();
            InitControls();
        }

        /// <summary>
        /// 仅事件绑定和数据初始化（UI 已在 Designer.cs 创建）
        /// </summary>
        private void InitControls()
        {
            // 动态提示文字（3 列模板）
            _lblHint.Text = "Excel 第 1 行为表头：节点名称 / 节点类型（分类组|分类）/ 父节点名称；父节点须出现在前面行。";
            _lblGrpFileTitle.Text = "步骤 1：选择 Excel 文件（分类组/分类结构）";
            // 取消按钮
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        // ====== 选择文件 ======
        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Excel 文件|*.xlsx;*.xls|所有文件|*.*";
                dialog.Title = "选择分类组 Excel 文件";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _selectedFile = dialog.FileName;
                    _txtFilePath.Text = _selectedFile;
                   

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

            try
            {
                _pendingRows = ExcelHelper.ReadClassificationNodes(_selectedFile);

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
                var nodes = ImportPreviewHelper.BuildGroupImportPreview(_pendingRows);

                // 显示预览窗体
                using (var previewForm = new ImportPreviewForm(nodes, errors, "分类组/分类导入预览"))
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
        /// 预校验数据行
        /// </summary>
        private List<ImportError> ValidateRows(List<ClassificationNodeRow> rows)
        {
            var errors = new List<ImportError>();
            var nameSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.Name))
                {
                    errors.Add(new ImportError { RowNumber = r.RowNumber, Name = r.Name, Message = "节点名称为空" });
                    continue;
                }
                if (r.NodeType != "分类组" && r.NodeType != "分类")
                {
                    errors.Add(new ImportError { RowNumber = r.RowNumber, Name = r.Name, Message = "节点类型非法：" + (r.NodeType ?? "(空)") });
                    continue;
                }
                if (nameSeen.Contains(r.Name))
                {
                    errors.Add(new ImportError { RowNumber = r.RowNumber, Name = r.Name, Message = "节点名称重复" });
                    continue;
                }
                nameSeen.Add(r.Name);
            }

            return errors;
        }

        /// <summary>
        /// 执行实际导入操作
        /// </summary>
        private void ExecuteImport()
        {
           
            _txtErrors.Clear();
            _lblResult.Text = string.Empty;
            _progressBar.Value = 0;
            _lblProgress.Text = "准备中...";

            var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            ImportResult result = null;

            worker.DoWork += (s, ev) =>
            {
                result = DoImport(worker, _pendingRows);
            };
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
                    LogErr("分类组导入已取消。");
                }
                else if (ev.Error != null)
                {
                    _lblResult.Text = "导入失败：" + ev.Error.Message;
                    _lblResult.ForeColor = Color.FromArgb(220, 53, 69);
                    LogErr("[严重错误] " + ev.Error.ToString());
                }
                else
                {
                    ShowResult(result);
                }
            };

            _cts = new CancellationTokenSource();
            worker.RunWorkerAsync();
        }

        private ImportResult DoImport(BackgroundWorker worker, List<ClassificationNodeRow> rows)
        {
            var result = new ImportResult();
            _cts = new CancellationTokenSource();

            LogInfo("===== 分类组/分类混合导入开始 =====");
            worker.ReportProgress(0, "正在导入...");

            // ====== 预校验：必填、节点类型、名称唯一性 ======
            var nameSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.Name))
                {
                    result.FailCount++;
                    result.Errors.Add(new ImportError { RowNumber = r.RowNumber, Name = r.Name, Message = "节点名称为空" });
                    LogErr(string.Format("[失败-预校验] 行 {0}：节点名称为空", r.RowNumber));
                    continue;
                }
                if (r.NodeType != "分类组" && r.NodeType != "分类")
                {
                    result.FailCount++;
                    result.Errors.Add(new ImportError { RowNumber = r.RowNumber, Name = r.Name, Message = "节点类型非法：" + (r.NodeType ?? "(空)") });
                    LogErr(string.Format("[失败-预校验] 行 {0}：节点类型非法 {1}", r.RowNumber, r.NodeType));
                    continue;
                }
                if (nameSeen.Contains(r.Name))
                {
                    result.FailCount++;
                    result.Errors.Add(new ImportError { RowNumber = r.RowNumber, Name = r.Name, Message = "节点名称重复" });
                    LogErr(string.Format("[失败-预校验] 行 {0}：节点名称重复 {1}", r.RowNumber, r.Name));
                    continue;
                }
                nameSeen.Add(r.Name);
            }

            // ====== 逐行导入（父节点须已在前面行成功处理）======
            var dict = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            var typeDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rows.Count; i++)
            {
                if (worker.CancellationPending) break;
                var r = rows[i];
                int pct = (int)((double)(i + 1) / rows.Count * 100);
                worker.ReportProgress(pct, string.Format("正在导入 {0}/{1} ...", i + 1, rows.Count));

                // 预校验未通过者已在上方记录，此处跳过
                if (string.IsNullOrWhiteSpace(r.Name) || (r.NodeType != "分类组" && r.NodeType != "分类"))
                    continue;

                long parentId = 0;
                string parentType = null;
                if (!string.IsNullOrWhiteSpace(r.ParentName))
                {
                    if (!dict.TryGetValue(r.ParentName, out long pid))
                    {
                        result.FailCount++;
                        result.Errors.Add(new ImportError { RowNumber = r.RowNumber, Name = r.Name, Message = "父节点未找到：" + r.ParentName });
                        LogErr(string.Format("[失败] 行 {0}：父节点未找到 {1}", r.RowNumber, r.ParentName));
                        continue;
                    }
                    parentId = pid;
                    parentType = typeDict[r.ParentName];
                    if (parentType == "分类" && r.NodeType != "分类")
                    {
                        result.FailCount++;
                        result.Errors.Add(new ImportError { RowNumber = r.RowNumber, Name = r.Name, Message = "父节点为分类，子节点只能是分类" });
                        LogErr(string.Format("[失败] 行 {0}：父节点为分类，子节点只能是分类", r.RowNumber));
                        continue;
                    }
                }

                try
                {
                    long newId = _db.InsertClassificationNode(r.Name, r.NodeType, parentId, parentType);
                    dict[r.Name] = newId;
                    typeDict[r.Name] = r.NodeType;
                    result.SuccessCount++;
                    LogOk(string.Format("[成功] 行 {0}：{1}（{2}）-> id={3}", r.RowNumber, r.Name, r.NodeType, newId));
                }
                catch (Exception ex)
                {
                    result.FailCount++;
                    result.Errors.Add(new ImportError { RowNumber = r.RowNumber, Name = r.Name, Message = ex.Message });
                    LogErr(string.Format("[失败] 行 {0}：{1} - {2}", r.RowNumber, r.Name, ex.Message));
                }
            }

            worker.ReportProgress(100, "导入完成");
            LogInfo(string.Format("===== 分类组/分类导入完成：成功={0}，失败={1} =====",
                result.SuccessCount, result.FailCount));
            return result;
        }

        // ====== 结果展示 ======
        private void ShowResult(ImportResult result)
        {
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



        // ====== 生成模板 ======
        private void BtnTemplate_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel 文件|*.xlsx";
                dialog.Title = "保存分类组/分类导入模板";
                dialog.FileName = "分类结构导入模板.xlsx";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExcelHelper.CreateGroupImportTemplate(dialog.FileName);
                        MessageBox.Show("模板已生成：" + dialog.FileName,
                            "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("生成模板失败：" + ex.Message,
                            "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private const string MODULE_NAME = "分类组导入";

        private void LogInfo(string msg) { Logger.Instance.Log(MODULE_NAME, msg); }
        private void LogOk(string msg) { Logger.Instance.LogSuccess(MODULE_NAME, msg); }
        private void LogErr(string msg) { Logger.Instance.LogError(MODULE_NAME, msg); }

        // ====== 取消（窗口关闭时中断后台线程）======
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
                _cts.Cancel();
            base.OnFormClosing(e);
        }
    }
}
