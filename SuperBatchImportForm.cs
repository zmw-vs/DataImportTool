using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace DataImportTool
{
    /// <summary>
    /// 超级批量导入表单（PRD v6.0）
    /// 通过单个 Excel 文件一次性导入分类结构、分类属性值、元模型属性
    /// </summary>
    public partial class SuperBatchImportForm : Form
    {
        private readonly DbHelper _db;
        private SuperBatchData _data;
        private List<FormTemplateInfo> _templates;
        private bool _importing;
        private string _selectedTemplateUiContent;
        private BackgroundWorker _bw;
        private string _bwMode; // 当前后台任务："parse" | "validate"

        public SuperBatchImportForm(DbHelper db)
        {
            InitializeComponent();
            _db = db;
            _data = null;
            _templates = null;
            _importing = false;
            _selectedTemplateUiContent = null;
            InitBackgroundWorker();
        }

        private void SuperBatchImportForm_Load(object sender, EventArgs e)
        {
            LoadTemplates();
            _chkGenerateForm.Checked = true;
            _lblTemplate.Visible = true;
            _cmbTemplate.Visible = true;
            _progressBar.Visible = false;
            UpdateButtonState();
        }

        private void LoadTemplates()
        {
            try
            {
                _templates = _db.GetFormTemplateList();
                _cmbTemplate.Items.Clear();
                _cmbTemplate.Items.Add("（请选择模板）");
                foreach (var t in _templates)
                    _cmbTemplate.Items.Add(t);
                _cmbTemplate.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                AppendLog("加载模板列表失败：" + ex.Message);
            }
        }

        private void UpdateButtonState()
        {
            bool fileReady = _data != null;
            _btnPreview.Enabled = fileReady && !_importing;
            _btnSelectFile.Enabled = !_importing;
            _btnGenTemplate.Enabled = !_importing;
            _cmbTemplate.Enabled = !_importing;
        }

        private void CmbTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedTemplateUiContent = null;
            if (_cmbTemplate.SelectedIndex > 0)
            {
                var tpl = _cmbTemplate.SelectedItem as FormTemplateInfo;
                if (tpl != null)
                {
                    try { _selectedTemplateUiContent = _db.GetFormTemplateUiContent(tpl.Id); }
                    catch { _selectedTemplateUiContent = string.Empty; }
                }
            }
            UpdateButtonState();
        }

        private void ChkGenerateForm_CheckedChanged(object sender, EventArgs e)
        {
            bool gen = _chkGenerateForm.Checked;
            _lblTemplate.Visible = gen;
            _cmbTemplate.Visible = gen;
            if (!gen)
            {
                _cmbTemplate.SelectedIndex = -1;
                _selectedTemplateUiContent = null;
            }
            UpdateButtonState();
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|所有文件 (*.*)|*.*";
                dlg.Title = "选择超级批量导入 Excel 文件";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                _txtFilePath.Text = dlg.FileName;
                _data = null;
                UpdateButtonState();
                SetStatus("正在解析 Excel…");
                AppendLog("开始解析文件：" + dlg.FileName);

                // 把耗时的「Excel 解析 + 统计」放到后台线程，避免界面卡死
                _bwMode = "parse";
                EnterBusy();
                _bw.RunWorkerAsync(dlg.FileName);
            }
        }

        private void BtnGenTemplate_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx";
                dlg.Title = "生成超级批量导入模板";
                dlg.FileName = "超级批量导入模板.xlsx";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    SuperBatchImportEngine.GenerateTemplate(dlg.FileName);
                    MessageBox.Show("模板已生成到：\n" + dlg.FileName, "生成成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("生成模板失败：" + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ====== 预览（预览窗体内含导入按钮）======
        private void BtnPreview_Click(object sender, EventArgs e)
        {
            if (_data == null) return;

            bool gen = _chkGenerateForm.Checked;
            if (gen && _cmbTemplate.SelectedIndex <= 0)
            {
                MessageBox.Show("生成表单需要选择一个模板。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 这些写入 _data 的操作很快，保持同步
            _data.GenerateForms = gen;
            if (gen && _cmbTemplate.SelectedIndex > 0)
            {
                var template = _templates[_cmbTemplate.SelectedIndex - 1];
                _data.TemplateId = template.Id;
                AppendLog("选定模板：" + template.DisplayName);
            }

            // 把耗时的「校验 + 构建预览树」放到后台线程，避免界面卡死
            StartValidation();
        }

        private void InitBackgroundWorker()
        {
            _bw = new BackgroundWorker();
            _bw.WorkerReportsProgress = true;
            _bw.DoWork += Bw_DoWork;
            _bw.ProgressChanged += Bw_ProgressChanged;
            _bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
        }

        private void StartValidation()
        {
            // 校验期间禁用交互，防止数据被中途改动
            _bwMode = "validate";
            EnterBusy();
            SetStatus("正在校验数据…");
            AppendLog("开始后台校验…");
            _bw.RunWorkerAsync();
        }

        private void EnterBusy()
        {
            _btnPreview.Enabled = false;
            _btnSelectFile.Enabled = false;
            _btnGenTemplate.Enabled = false;
            _cmbTemplate.Enabled = false;
            _progressBar.Visible = true;
            _progressBar.Style = ProgressBarStyle.Marquee;
            _progressBar.MarqueeAnimationSpeed = 50;
        }

        private void ExitBusy()
        {
            _progressBar.Visible = false;
            UpdateButtonState();
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var bw = (BackgroundWorker)sender;

            if (_bwMode == "parse")
            {
                // Excel 解析 + 统计（纯读文件，无 DB 访问，线程安全）
                string path = (string)e.Argument;
                bw.ReportProgress(0, "正在解析 Excel（读取分类结构 / 属性 / 元模型）…");
                var data = SuperBatchImportEngine.Parse(path);
                bw.ReportProgress(0, "正在统计解析结果…");
                var stats = SuperBatchImportEngine.ComputeStats(data);
                e.Result = new ParseResult { Data = data, Stats = stats };
                return;
            }

            // 校验 + 构建预览树
            bw.ReportProgress(0, "正在校验数据（分类 / 属性 / 元模型）…");
            var validationResult = SuperBatchImportEngine.Validate(_data, _db);

            bw.ReportProgress(0, "正在收集属性集信息…");
            var setDisplayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var s in _db.GetPropertySets())
                    setDisplayNames[s.InternalName] = s.DisplayName;
            }
            catch { }

            bw.ReportProgress(0, "正在构建预览树…");
            var tree = SuperBatchImportEngine.BuildPreviewTree(_data);

            e.Result = new PreviewBuildResult
            {
                Validation = validationResult,
                Tree = tree,
                SetDisplayNames = setDisplayNames
            };
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string msg)
                SetStatus(msg);
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.IsDisposed || this.Disposing) return;

            if (_bwMode == "parse")
            {
                _progressBar.Visible = false;

                if (e.Error != null)
                {
                    AppendLog("文件解析失败：" + e.Error.Message);
                    SetStatus("解析失败：" + e.Error.Message);
                    _data = null;
                    UpdateButtonState();
                    return;
                }

                var pr = (ParseResult)e.Result;
                _data = pr.Data;
                AppendLog("解析完成：" + pr.Stats.Groups + " 个分类组, " + pr.Stats.Classifications + " 个分类, "
                    + pr.Stats.Properties + " 个属性定义, " + pr.Stats.PropertyValues + " 个属性值, "
                    + pr.Stats.Forms + " 个表单生成, " + pr.Stats.MetaProperties + " 个元模型属性");
                SetStatus("文件解析完成");
                UpdateButtonState();
                return;
            }

            // 校验完成 → 打开预览
            ExitBusy();

            if (e.Error != null)
            {
                AppendLog("校验失败：" + e.Error.Message);
                SetStatus("校验失败：" + e.Error.Message);
                MessageBox.Show("校验过程出错：" + e.Error.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = (PreviewBuildResult)e.Result;
            if (result.Validation.IsValid)
                AppendLog("校验通过，无错误。");
            else
                AppendLog("校验发现 " + result.Validation.Errors.Count + " 个错误，将在预览中显示。");
            SetStatus("校验完成，正在打开预览…");

            try
            {
                using (var preview = new SuperBatchPreviewForm(
                    result.Tree, _data.MetaProperties, result.SetDisplayNames,
                    _db, _data, _selectedTemplateUiContent, result.Validation))
                {
                    preview.ShowDialog(this);

                    if (preview.Imported)
                    {
                        AppendLog("导入结果：" + (preview.ImportResult?.Summary ?? "完成"));
                        _lblResult.Text = "结果：" + (preview.ImportResult?.Summary ?? "完成");
                        _lblResult.ForeColor = Color.FromArgb(40, 167, 69);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("预览失败：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ParseResult
        {
            public SuperBatchData Data;
            public SuperBatchStats Stats;
        }

        private class PreviewBuildResult
        {
            public SuperBatchValidationResult Validation;
            public List<PreviewNode> Tree;
            public Dictionary<string, string> SetDisplayNames;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SetStatus(string text)
        {
            _lblStatus.Text = text;
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
