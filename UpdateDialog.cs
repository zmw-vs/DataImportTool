using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DataImportTool
{
    /// <summary>更新提示对话框：展示新版本信息、下载进度，并触发更新。</summary>
    public class UpdateDialog : Form
    {
        private readonly UpdateInfo _info;
        private readonly string _currentVersion;
        private readonly bool _mandatory;

        private Label _lblTitle;
        private Label _lblVer;
        private TextBox _txtNotes;
        private ProgressBar _progress;
        private Label _lblStatus;
        private Button _btnUpdate;
        private Button _btnSkip;
        private Button _btnLater;
        private BackgroundWorker _bw;

        public UpdateDialog(UpdateInfo info, string currentVersion, bool mandatory)
        {
            _info = info;
            _currentVersion = currentVersion;
            _mandatory = mandatory;
            InitializeComponent();
            BuildUI();
        }

        private void InitializeComponent()
        {
            this.Text = "发现新版本";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(460, 322);
            this.ShowInTaskbar = false;
        }

        private void BuildUI()
        {
            _lblTitle = new Label
            {
                Text = "发现新版本可用",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(16, 14),
                Size = new Size(428, 24),
                ForeColor = Color.FromArgb(40, 167, 69)
            };

            _lblVer = new Label
            {
                Text = string.Format("当前版本：{0}    最新版本：{1}", _currentVersion, _info.Version),
                Location = new Point(16, 46),
                Size = new Size(428, 20),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            var lblNotesCap = new Label
            {
                Text = "更新说明：",
                Location = new Point(16, 74),
                Size = new Size(428, 18),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            _txtNotes = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(16, 96),
                Size = new Size(428, 110),
                Text = _info.ReleaseNotes ?? string.Empty,
                BackColor = Color.White
            };

            _progress = new ProgressBar
            {
                Location = new Point(16, 214),
                Size = new Size(428, 18),
                Style = ProgressBarStyle.Blocks,
                Visible = false
            };

            _lblStatus = new Label
            {
                Text = string.Empty,
                Location = new Point(16, 236),
                Size = new Size(428, 18),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            _btnUpdate = new Button
            {
                Text = "立即更新",
                Location = new Point(250, 262),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnUpdate.FlatAppearance.BorderSize = 0;
            _btnUpdate.Click += BtnUpdate_Click;

            _btnSkip = new Button
            {
                Text = "跳过此版本",
                Location = new Point(346, 262),
                Size = new Size(98, 32)
            };
            _btnSkip.Click += BtnSkip_Click;

            _btnLater = new Button
            {
                Text = "以后再说",
                Location = new Point(154, 262),
                Size = new Size(90, 32)
            };
            _btnLater.Click += BtnLater_Click;

            if (_mandatory)
            {
                _btnSkip.Visible = false;
                _btnLater.Visible = false;
                _btnUpdate.Location = new Point(354, 262);
            }

            this.Controls.AddRange(new Control[]
            {
                _lblTitle, _lblVer, lblNotesCap, _txtNotes,
                _progress, _lblStatus, _btnUpdate, _btnSkip, _btnLater
            });
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            _btnUpdate.Enabled = false;
            _btnSkip.Enabled = false;
            _btnLater.Enabled = false;
            _progress.Visible = true;
            _lblStatus.Text = "正在下载更新...";

            _bw = new BackgroundWorker();
            _bw.WorkerReportsProgress = true;
            _bw.DoWork += Bw_DoWork;
            _bw.ProgressChanged += Bw_ProgressChanged;
            _bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            _bw.RunWorkerAsync();
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var bw = (BackgroundWorker)sender;
            string tempExe = Path.Combine(Path.GetTempPath(),
                string.Format("DataImportTool_update_v{0}.exe", _info.Version));
            UpdateManager.DownloadFile(_info.DownloadUrl, tempExe,
                (pct, recv, total) => bw.ReportProgress(pct, tempExe));
            e.Result = tempExe;
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _progress.Value = Math.Max(0, Math.Min(100, e.ProgressPercentage));
            _lblStatus.Text = string.Format("正在下载更新... {0}%", e.ProgressPercentage);
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _lblStatus.Text = "下载失败：" + e.Error.Message;
                ResetButtons();
                return;
            }

            string tempExe = (string)e.Result;
            if (!UpdateManager.VerifyHash(tempExe, _info))
            {
                _lblStatus.Text = "更新文件校验失败（MD5 不匹配），已取消。";
                try { File.Delete(tempExe); } catch { }
                ResetButtons();
                return;
            }

            _lblStatus.Text = "更新已下载，正在重启应用...";
            UpdateManager.ApplyUpdate(tempExe, _info);
            // ApplyUpdate 会退出程序；若未退出（异常）则关闭对话框
            this.Close();
        }

        private void ResetButtons()
        {
            _progress.Visible = false;
            _btnUpdate.Enabled = true;
            _btnSkip.Enabled = !_mandatory;
            _btnLater.Enabled = !_mandatory;
        }

        private void BtnSkip_Click(object sender, EventArgs e)
        {
            UpdateManager.SetSkipVersion(_info.Version);
            this.DialogResult = DialogResult.Ignore;
            this.Close();
        }

        private void BtnLater_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
