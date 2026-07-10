using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    public partial class ConfigMigrationForm : Form
    {
        private readonly DbHelper _sourceDb;
        private readonly AppConfig _config;
        private string _srcConn;
        private string _tgtConnCache;
        private string _restoreTag;
        private readonly BackgroundWorker _bw;

        private enum Op { Detect, Backup, Migrate, Restore, Verify }

        public ConfigMigrationForm()
            : this(null, null)
        {
        }

        public ConfigMigrationForm(DbHelper sourceDb, AppConfig config)
        {
            InitializeComponent();
            _sourceDb = sourceDb;
            _config = config;
            _srcConn = (config != null) ? config.BuildConnectionString() : null;
            _bw = new BackgroundWorker();
            _bw.WorkerReportsProgress = true;
            _bw.DoWork += Bw_DoWork;
            _bw.ProgressChanged += (s, e) => Log((string)e.UserState);
            _bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            WireUpEvents();
            LoadData();
        }

        private void WireUpEvents()
        {
            this._btnDetect.Click += _btnDetect_Click;
            this._btnBackup.Click += _btnBackup_Click;
            this._btnMigrate.Click += _btnMigrate_Click;
            this._btnRestore.Click += _btnRestore_Click;
            this._btnVerify.Click += _btnVerify_Click;
            this._rdoWinAuth.CheckedChanged += _rdoWinAuth_CheckedChanged;
            this._rdoSqlAuth.CheckedChanged += _rdoSqlAuth_CheckedChanged;
        }

        private void LoadData()
        {
            _txtSrc.Text = (_config != null) ? _config.GetDisplayInfo() : "(未配置源库)";
            _rdoWinAuth.Checked = true;
            SyncAuthEnabled();
            RefreshBackups();
        }

        private void SyncAuthEnabled()
        {
            bool sql = _rdoSqlAuth.Checked;
            _txtTgtUser.Enabled = sql;
            _txtTgtPwd.Enabled = sql;
        }

        private void _rdoWinAuth_CheckedChanged(object sender, EventArgs e) { if (_rdoWinAuth.Checked) SyncAuthEnabled(); }
        private void _rdoSqlAuth_CheckedChanged(object sender, EventArgs e) { if (_rdoSqlAuth.Checked) SyncAuthEnabled(); }

        // 目标库连接串（UI 线程构造，异常即时反馈）
        private string BuildTgtConn()
        {
            string server = _txtTgtServer.Text.Trim();
            string db = _txtTgtDb.Text.Trim();
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(db))
                throw new Exception("请填写目标库服务器和数据库。");
            if (_rdoWinAuth.Checked)
                return string.Format("Server={0};Database={1};Integrated Security=True;", server, db);
            string user = _txtTgtUser.Text.Trim();
            if (string.IsNullOrEmpty(user))
                throw new Exception("SQL 认证需填写用户名。");
            return string.Format("Server={0};Database={1};User Id={2};Password={3};",
                server, db, user, _txtTgtPwd.Text);
        }

        private ConfigMigrationEngine.Scope GetScope()
        {
            var s = ConfigMigrationEngine.Scope.None;
            if (_chkClassification.Checked) s |= ConfigMigrationEngine.Scope.Classification;
            if (_chkMeta.Checked) s |= ConfigMigrationEngine.Scope.MetaElement;
            if (_chkForm.Checked) s |= ConfigMigrationEngine.Scope.FormTemplates;
            if (_chkConfig.Checked) s |= ConfigMigrationEngine.Scope.ConfigItem;
            return s;
        }

        private void SetButtons(bool enabled)
        {
            _btnDetect.Enabled = enabled;
            _btnBackup.Enabled = enabled;
            _btnMigrate.Enabled = enabled;
            _btnRestore.Enabled = enabled;
            _btnVerify.Enabled = enabled;
        }

        private void Log(string msg)
        {
            if (_txtLog.IsDisposed) return;
            Color c = Color.FromArgb(212, 212, 212);
            if (msg.IndexOf("✗") >= 0 || msg.IndexOf("失败") >= 0 || msg.IndexOf("出错") >= 0 || msg.IndexOf("冲突跳过") >= 0)
                c = Color.FromArgb(240, 95, 95);
            else if (msg.IndexOf("⚠") >= 0 || msg.IndexOf("警告") >= 0)
                c = Color.FromArgb(240, 185, 90);
            else if (msg.IndexOf("完成") >= 0 || msg.IndexOf("成功") >= 0 || msg.IndexOf("一致") >= 0
                || msg.IndexOf("安全") >= 0 || msg.IndexOf("已生成") >= 0 || msg.IndexOf("恢复完成") >= 0)
                c = Color.FromArgb(95, 205, 125);
            _txtLog.SelectionColor = c;
            _txtLog.AppendText(msg + Environment.NewLine);
        }

        // ============================================================
        // 按钮
        // ============================================================
        private void _btnDetect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_srcConn)) { Log("✗ 源库未配置。"); return; }
            RunBw(Op.Detect);
        }

        private void _btnBackup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_srcConn)) { Log("✗ 源库未配置。"); return; }
            if (GetScope() == ConfigMigrationEngine.Scope.None) { Log("✗ 请至少勾选一项范围。"); return; }
            RunBw(Op.Backup);
        }

        private void _btnMigrate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_srcConn)) { Log("✗ 源库未配置。"); return; }
            if (GetScope() == ConfigMigrationEngine.Scope.None) { Log("✗ 请至少勾选一项范围。"); return; }
            if (MessageBox.Show("即将把源库所选范围数据迁移到目标库（保留旧 ID）。\r\n目标非空时会自动备份。确认执行？",
                "确认迁移", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                return;
            RunBw(Op.Migrate);
        }

        private void _btnRestore_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_srcConn)) { Log("✗ 源库未配置。"); return; }
            var sel = _lstBackups.SelectedItem as ConfigMigrationEngine.BackupInfo;
            string which = (sel != null)
                ? ("（备份点：" + sel.ToString() + "）")
                : "（将恢复【最近一次】备份）";
            if (MessageBox.Show("恢复会把目标库回滚到所选备份" + which + "。\r\n该备份【之后】写入的新数据都会被丢弃。确认恢复？",
                "确认恢复", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                return;
            _restoreTag = (sel != null) ? sel.Tag : null;
            RunBw(Op.Restore);
        }

        private void _btnVerify_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_srcConn)) { Log("✗ 源库未配置。"); return; }
            if (GetScope() == ConfigMigrationEngine.Scope.None) { Log("✗ 请至少勾选一项范围。"); return; }
            RunBw(Op.Verify);
        }

        private void RunBw(Op op)
        {
            if (_bw.IsBusy) return;
            string tgt;
            try { tgt = BuildTgtConn(); }
            catch (Exception ex) { Log("✗ " + ex.Message); return; }
            _tgtConnCache = tgt;
            SetButtons(false);
            _prog.Style = ProgressBarStyle.Marquee;
            _bw.RunWorkerAsync(op);
        }

        // ============================================================
        // BackgroundWorker
        // ============================================================
        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var op = (Op)e.Argument;
            var engine = new ConfigMigrationEngine();
            string tgt = _tgtConnCache;
            switch (op)
            {
                case Op.Detect:
                    e.Result = engine.Detect(_srcConn, tgt);
                    break;
                case Op.Backup:
                    engine.Backup(_srcConn, tgt, GetScope(), m => _bw.ReportProgress(0, m));
                    break;
                case Op.Migrate:
                    engine.Migrate(_srcConn, tgt, GetScope(), m => _bw.ReportProgress(0, m), () => false);
                    break;
                case Op.Restore:
                    engine.Restore(tgt, m => _bw.ReportProgress(0, m), _restoreTag);
                    break;
                case Op.Verify:
                    e.Result = engine.Verify(_srcConn, tgt, GetScope());
                    break;
            }
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Log("✗ 执行出错：" + e.Error.Message);
            }
            else
            {
                if (e.Result is ConfigMigrationEngine.DetectResult det) ShowDetect(det);
                else if (e.Result is ConfigMigrationEngine.VerifyResult rep) ShowVerify(rep);
            }
            RefreshBackups();
            SetButtons(true);
            _prog.Style = ProgressBarStyle.Blocks;
        }

        private void ShowDetect(ConfigMigrationEngine.DetectResult det)
        {
            _lvDetect.Items.Clear();
            foreach (var r in det.Rows)
            {
                var it = new ListViewItem(new[]
                {
                    r.Database, r.Table,
                    FormatIdCell(r.SrcRows, r.SrcMax),
                    FormatIdCell(r.TgtRows, r.TgtMax)
                });
                it.ForeColor = (r.TgtRows > 0) ? System.Drawing.Color.FromArgb(200, 80, 0) : System.Drawing.Color.Gray;
                _lvDetect.Items.Add(it);
            }
            Log("【检测结论】" + det.Verdict + "：" + det.Message);
            Log("  源版本=" + det.SrcVersion + "，目标版本=" + det.TgtVersion);
            if (det.Verdict == "B")
                Log("  ⚠ 检测到冲突/版本不一致，禁止直接执行迁移。");

            // 结论横幅：A 绿 / C 橙 / B 红
            if (det.Verdict == "A")
            {
                _lblVerdict.BackColor = Color.FromArgb(40, 125, 65);
                _lblVerdict.Text = "✓ 环境安全（A）：目标库相关表为空，可放心执行迁移";
            }
            else if (det.Verdict == "C")
            {
                _lblVerdict.BackColor = Color.FromArgb(175, 120, 30);
                _lblVerdict.Text = "⚠ 合并场景（C）：目标已有数据但 ID 不重叠，执行将覆盖/插入，请确认";
            }
            else
            {
                _lblVerdict.BackColor = Color.FromArgb(170, 50, 50);
                _lblVerdict.Text = "✗ 禁止执行（B）：" + det.Message;
            }
            _lblVerdict.ForeColor = Color.White;
        }

        /// <summary>检测列表中 ID 单元格文本：无数值主键(-1)的表只显示行数。</summary>
        private static string FormatIdCell(long rows, long max)
        {
            return max >= 0 ? (rows + "(" + max + ")") : rows.ToString();
        }

        /// <summary>刷新备份点列表（窗口加载、备份/迁移/恢复完成后调用）。</summary>
        private void RefreshBackups()
        {
            if (_lstBackups.IsDisposed) return;
            _lstBackups.Items.Clear();
            try
            {
                var engine = new ConfigMigrationEngine();
                var list = engine.ListBackups();
                if (list.Count == 0)
                    _lstBackups.Items.Add("（暂无备份，迁移前会自动生成）");
                else
                    foreach (var b in list) _lstBackups.Items.Add(b);
            }
            catch (Exception ex)
            {
                _lstBackups.Items.Add("（读取备份列表失败：" + ex.Message + "）");
            }
        }

        private void ShowVerify(ConfigMigrationEngine.VerifyResult rep)
        {
            _lvDetect.Items.Clear();
            foreach (var r in rep.Rows)
            {
                var it = new ListViewItem(new[]
                {
                    r.Database, r.Table,
                    r.SrcRows.ToString(),
                    r.TgtRows.ToString()
                });
                it.ForeColor = r.Match ? System.Drawing.Color.FromArgb(40, 140, 40) : System.Drawing.Color.FromArgb(200, 40, 40);
                _lvDetect.Items.Add(it);
            }
            Log("【校验结论】" + rep.Summary);
        }
    }
}
