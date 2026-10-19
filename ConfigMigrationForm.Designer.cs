namespace DataImportTool
{
    partial class ConfigMigrationForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label _lblSrc;
        private System.Windows.Forms.TextBox _txtSrc;
        private System.Windows.Forms.GroupBox _grpTgt;
        private System.Windows.Forms.Label _lblTgtServer;
        private System.Windows.Forms.TextBox _txtTgtServer;
        private System.Windows.Forms.Label _lblTgtDb;
        private System.Windows.Forms.TextBox _txtTgtDb;
        private System.Windows.Forms.RadioButton _rdoWinAuth;
        private System.Windows.Forms.RadioButton _rdoSqlAuth;
        private System.Windows.Forms.Label _lblTgtUser;
        private System.Windows.Forms.TextBox _txtTgtUser;
        private System.Windows.Forms.Label _lblTgtPwd;
        private System.Windows.Forms.TextBox _txtTgtPwd;
        private System.Windows.Forms.GroupBox _grpScope;
        private System.Windows.Forms.CheckBox _chkClassification;
        private System.Windows.Forms.CheckBox _chkMeta;
        private System.Windows.Forms.CheckBox _chkForm;
        private System.Windows.Forms.CheckBox _chkConfig;
        private System.Windows.Forms.Button _btnDetect;
        private System.Windows.Forms.Button _btnBackup;
        private System.Windows.Forms.Button _btnMigrate;
        private System.Windows.Forms.Button _btnRestore;
        private System.Windows.Forms.Button _btnVerify;
        private System.Windows.Forms.Label _lblVerdict;
        private System.Windows.Forms.ListView _lvDetect;
        private System.Windows.Forms.ColumnHeader _colDb;
        private System.Windows.Forms.ColumnHeader _colTable;
        private System.Windows.Forms.ColumnHeader _colSrc;
        private System.Windows.Forms.ColumnHeader _colTgt;
        private System.Windows.Forms.Label _lblBak;
        private System.Windows.Forms.ListBox _lstBackups;
        private System.Windows.Forms.ProgressBar _prog;
        private System.Windows.Forms.RichTextBox _txtLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblSrc = new System.Windows.Forms.Label();
            this._txtSrc = new System.Windows.Forms.TextBox();
            this._grpTgt = new System.Windows.Forms.GroupBox();
            this._lblTgtServer = new System.Windows.Forms.Label();
            this._txtTgtServer = new System.Windows.Forms.TextBox();
            this._lblTgtDb = new System.Windows.Forms.Label();
            this._txtTgtDb = new System.Windows.Forms.TextBox();
            this._rdoWinAuth = new System.Windows.Forms.RadioButton();
            this._rdoSqlAuth = new System.Windows.Forms.RadioButton();
            this._lblTgtUser = new System.Windows.Forms.Label();
            this._txtTgtUser = new System.Windows.Forms.TextBox();
            this._lblTgtPwd = new System.Windows.Forms.Label();
            this._txtTgtPwd = new System.Windows.Forms.TextBox();
            this._grpScope = new System.Windows.Forms.GroupBox();
            this._chkClassification = new System.Windows.Forms.CheckBox();
            this._chkMeta = new System.Windows.Forms.CheckBox();
            this._chkForm = new System.Windows.Forms.CheckBox();
            this._chkConfig = new System.Windows.Forms.CheckBox();
            this._btnDetect = new System.Windows.Forms.Button();
            this._btnBackup = new System.Windows.Forms.Button();
            this._btnMigrate = new System.Windows.Forms.Button();
            this._btnRestore = new System.Windows.Forms.Button();
            this._btnVerify = new System.Windows.Forms.Button();
            this._lblVerdict = new System.Windows.Forms.Label();
            this._lvDetect = new System.Windows.Forms.ListView();
            this._colDb = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colTable = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colSrc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colTgt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._lblBak = new System.Windows.Forms.Label();
            this._lstBackups = new System.Windows.Forms.ListBox();
            this._prog = new System.Windows.Forms.ProgressBar();
            this._txtLog = new System.Windows.Forms.RichTextBox();
            this._grpTgt.SuspendLayout();
            this._grpScope.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblSrc
            // 
            this._lblSrc.AutoSize = true;
            this._lblSrc.Location = new System.Drawing.Point(14, 12);
            this._lblSrc.Name = "_lblSrc";
            this._lblSrc.Size = new System.Drawing.Size(157, 15);
            this._lblSrc.TabIndex = 24;
            this._lblSrc.Text = "源库（当前已连接）：";
            // 
            // _txtSrc
            // 
            this._txtSrc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtSrc.Location = new System.Drawing.Point(14, 32);
            this._txtSrc.Name = "_txtSrc";
            this._txtSrc.ReadOnly = true;
            this._txtSrc.Size = new System.Drawing.Size(1259, 25);
            this._txtSrc.TabIndex = 0;
            // 
            // _grpTgt
            // 
            this._grpTgt.Controls.Add(this._lblTgtServer);
            this._grpTgt.Controls.Add(this._txtTgtServer);
            this._grpTgt.Controls.Add(this._lblTgtDb);
            this._grpTgt.Controls.Add(this._txtTgtDb);
            this._grpTgt.Controls.Add(this._rdoWinAuth);
            this._grpTgt.Controls.Add(this._rdoSqlAuth);
            this._grpTgt.Controls.Add(this._lblTgtUser);
            this._grpTgt.Controls.Add(this._txtTgtUser);
            this._grpTgt.Controls.Add(this._lblTgtPwd);
            this._grpTgt.Controls.Add(this._txtTgtPwd);
            this._grpTgt.Location = new System.Drawing.Point(14, 64);
            this._grpTgt.Name = "_grpTgt";
            this._grpTgt.Size = new System.Drawing.Size(360, 236);
            this._grpTgt.TabIndex = 1;
            this._grpTgt.TabStop = false;
            this._grpTgt.Text = "  目标库（新库）连接配置";
            // 
            // _lblTgtServer
            // 
            this._lblTgtServer.AutoSize = true;
            this._lblTgtServer.Location = new System.Drawing.Point(18, 30);
            this._lblTgtServer.Name = "_lblTgtServer";
            this._lblTgtServer.Size = new System.Drawing.Size(67, 15);
            this._lblTgtServer.TabIndex = 0;
            this._lblTgtServer.Text = "服务器：";
            // 
            // _txtTgtServer
            // 
            this._txtTgtServer.Location = new System.Drawing.Point(137, 27);
            this._txtTgtServer.Name = "_txtTgtServer";
            this._txtTgtServer.Size = new System.Drawing.Size(179, 25);
            this._txtTgtServer.TabIndex = 2;
            // 
            // _lblTgtDb
            // 
            this._lblTgtDb.AutoSize = true;
            this._lblTgtDb.Location = new System.Drawing.Point(18, 68);
            this._lblTgtDb.Name = "_lblTgtDb";
            this._lblTgtDb.Size = new System.Drawing.Size(67, 15);
            this._lblTgtDb.TabIndex = 3;
            this._lblTgtDb.Text = "数据库：";
            // 
            // _txtTgtDb
            // 
            this._txtTgtDb.Location = new System.Drawing.Point(137, 65);
            this._txtTgtDb.Name = "_txtTgtDb";
            this._txtTgtDb.Size = new System.Drawing.Size(179, 25);
            this._txtTgtDb.TabIndex = 3;
            // 
            // _rdoWinAuth
            // 
            this._rdoWinAuth.AutoSize = true;
            this._rdoWinAuth.Checked = true;
            this._rdoWinAuth.Location = new System.Drawing.Point(18, 108);
            this._rdoWinAuth.Name = "_rdoWinAuth";
            this._rdoWinAuth.Size = new System.Drawing.Size(122, 19);
            this._rdoWinAuth.TabIndex = 4;
            this._rdoWinAuth.TabStop = true;
            this._rdoWinAuth.Text = "Windows 认证";
            this._rdoWinAuth.CheckedChanged += new System.EventHandler(this._rdoWinAuth_CheckedChanged);
            // 
            // _rdoSqlAuth
            // 
            this._rdoSqlAuth.AutoSize = true;
            this._rdoSqlAuth.Location = new System.Drawing.Point(177, 108);
            this._rdoSqlAuth.Name = "_rdoSqlAuth";
            this._rdoSqlAuth.Size = new System.Drawing.Size(90, 19);
            this._rdoSqlAuth.TabIndex = 5;
            this._rdoSqlAuth.Text = "SQL 认证";
            this._rdoSqlAuth.CheckedChanged += new System.EventHandler(this._rdoSqlAuth_CheckedChanged);
            // 
            // _lblTgtUser
            // 
            this._lblTgtUser.AutoSize = true;
            this._lblTgtUser.Location = new System.Drawing.Point(18, 146);
            this._lblTgtUser.Name = "_lblTgtUser";
            this._lblTgtUser.Size = new System.Drawing.Size(67, 15);
            this._lblTgtUser.TabIndex = 6;
            this._lblTgtUser.Text = "用户名：";
            // 
            // _txtTgtUser
            // 
            this._txtTgtUser.Enabled = false;
            this._txtTgtUser.Location = new System.Drawing.Point(137, 143);
            this._txtTgtUser.Name = "_txtTgtUser";
            this._txtTgtUser.Size = new System.Drawing.Size(179, 25);
            this._txtTgtUser.TabIndex = 6;
            // 
            // _lblTgtPwd
            // 
            this._lblTgtPwd.AutoSize = true;
            this._lblTgtPwd.Location = new System.Drawing.Point(18, 184);
            this._lblTgtPwd.Name = "_lblTgtPwd";
            this._lblTgtPwd.Size = new System.Drawing.Size(52, 15);
            this._lblTgtPwd.TabIndex = 7;
            this._lblTgtPwd.Text = "密码：";
            // 
            // _txtTgtPwd
            // 
            this._txtTgtPwd.Enabled = false;
            this._txtTgtPwd.Location = new System.Drawing.Point(137, 181);
            this._txtTgtPwd.Name = "_txtTgtPwd";
            this._txtTgtPwd.PasswordChar = '*';
            this._txtTgtPwd.Size = new System.Drawing.Size(179, 25);
            this._txtTgtPwd.TabIndex = 7;
            // 
            // _grpScope
            // 
            this._grpScope.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._grpScope.Controls.Add(this._chkClassification);
            this._grpScope.Controls.Add(this._chkMeta);
            this._grpScope.Controls.Add(this._chkForm);
            this._grpScope.Controls.Add(this._chkConfig);
            this._grpScope.Location = new System.Drawing.Point(465, 69);
            this._grpScope.Name = "_grpScope";
            this._grpScope.Size = new System.Drawing.Size(443, 236);
            this._grpScope.TabIndex = 8;
            this._grpScope.TabStop = false;
            this._grpScope.Text = "  迁移范围";
            // 
            // _chkClassification
            // 
            this._chkClassification.AutoSize = true;
            this._chkClassification.Checked = true;
            this._chkClassification.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkClassification.Location = new System.Drawing.Point(18, 35);
            this._chkClassification.Name = "_chkClassification";
            this._chkClassification.Size = new System.Drawing.Size(159, 19);
            this._chkClassification.TabIndex = 9;
            this._chkClassification.Text = "分类/组/关系(7表)";
            // 
            // _chkMeta
            // 
            this._chkMeta.AutoSize = true;
            this._chkMeta.Checked = true;
            this._chkMeta.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkMeta.Location = new System.Drawing.Point(18, 82);
            this._chkMeta.Name = "_chkMeta";
            this._chkMeta.Size = new System.Drawing.Size(104, 19);
            this._chkMeta.TabIndex = 10;
            this._chkMeta.Text = "元模型字段";
            // 
            // _chkForm
            // 
            this._chkForm.AutoSize = true;
            this._chkForm.Checked = true;
            this._chkForm.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkForm.Location = new System.Drawing.Point(18, 133);
            this._chkForm.Name = "_chkForm";
            this._chkForm.Size = new System.Drawing.Size(89, 19);
            this._chkForm.TabIndex = 11;
            this._chkForm.Text = "分类表单";
            // 
            // _chkConfig
            // 
            this._chkConfig.AutoSize = true;
            this._chkConfig.Checked = true;
            this._chkConfig.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkConfig.Location = new System.Drawing.Point(18, 175);
            this._chkConfig.Name = "_chkConfig";
            this._chkConfig.Size = new System.Drawing.Size(74, 19);
            this._chkConfig.TabIndex = 12;
            this._chkConfig.Text = "配置项";
            // 
            // _btnDetect
            // 
            this._btnDetect.Location = new System.Drawing.Point(14, 352);
            this._btnDetect.Name = "_btnDetect";
            this._btnDetect.Size = new System.Drawing.Size(183, 34);
            this._btnDetect.TabIndex = 13;
            this._btnDetect.Text = "检测";
            this._btnDetect.UseVisualStyleBackColor = true;
            this._btnDetect.Click += new System.EventHandler(this._btnDetect_Click);
            // 
            // _btnBackup
            // 
            this._btnBackup.Location = new System.Drawing.Point(206, 352);
            this._btnBackup.Name = "_btnBackup";
            this._btnBackup.Size = new System.Drawing.Size(183, 34);
            this._btnBackup.TabIndex = 14;
            this._btnBackup.Text = "备份(手动)";
            this._btnBackup.UseVisualStyleBackColor = true;
            this._btnBackup.Click += new System.EventHandler(this._btnBackup_Click);
            // 
            // _btnMigrate
            // 
            this._btnMigrate.Location = new System.Drawing.Point(398, 352);
            this._btnMigrate.Name = "_btnMigrate";
            this._btnMigrate.Size = new System.Drawing.Size(183, 34);
            this._btnMigrate.TabIndex = 15;
            this._btnMigrate.Text = "执行迁移";
            this._btnMigrate.UseVisualStyleBackColor = true;
            this._btnMigrate.Click += new System.EventHandler(this._btnMigrate_Click);
            // 
            // _btnRestore
            // 
            this._btnRestore.Location = new System.Drawing.Point(590, 352);
            this._btnRestore.Name = "_btnRestore";
            this._btnRestore.Size = new System.Drawing.Size(183, 34);
            this._btnRestore.TabIndex = 16;
            this._btnRestore.Text = "恢复";
            this._btnRestore.UseVisualStyleBackColor = true;
            this._btnRestore.Click += new System.EventHandler(this._btnRestore_Click);
            // 
            // _btnVerify
            // 
            this._btnVerify.Location = new System.Drawing.Point(782, 352);
            this._btnVerify.Name = "_btnVerify";
            this._btnVerify.Size = new System.Drawing.Size(187, 34);
            this._btnVerify.TabIndex = 17;
            this._btnVerify.Text = "校验(行数/外键)";
            this._btnVerify.UseVisualStyleBackColor = true;
            this._btnVerify.Click += new System.EventHandler(this._btnVerify_Click);
            // 
            // _lblVerdict
            // 
            this._lblVerdict.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._lblVerdict.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(90)))), ((int)(((byte)(110)))));
            this._lblVerdict.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._lblVerdict.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblVerdict.ForeColor = System.Drawing.Color.White;
            this._lblVerdict.Location = new System.Drawing.Point(14, 308);
            this._lblVerdict.Name = "_lblVerdict";
            this._lblVerdict.Size = new System.Drawing.Size(1259, 36);
            this._lblVerdict.TabIndex = 18;
            this._lblVerdict.Text = "① 填写目标库连接 → ② 点【检测】确认环境安全 → ③ 执行迁移 → ④ 校验";
            this._lblVerdict.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lvDetect
            // 
            this._lvDetect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._lvDetect.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colDb,
            this._colTable,
            this._colSrc,
            this._colTgt});
            this._lvDetect.FullRowSelect = true;
            this._lvDetect.GridLines = true;
            this._lvDetect.HideSelection = false;
            this._lvDetect.Location = new System.Drawing.Point(14, 400);
            this._lvDetect.Name = "_lvDetect";
            this._lvDetect.Size = new System.Drawing.Size(714, 150);
            this._lvDetect.TabIndex = 19;
            this._lvDetect.UseCompatibleStateImageBehavior = false;
            this._lvDetect.View = System.Windows.Forms.View.Details;
            // 
            // _colDb
            // 
            this._colDb.Text = "库";
            this._colDb.Width = 56;
            // 
            // _colTable
            // 
            this._colTable.Text = "表";
            this._colTable.Width = 344;
            // 
            // _colSrc
            // 
            this._colSrc.Text = "源 max/rows";
            this._colSrc.Width = 80;
            // 
            // _colTgt
            // 
            this._colTgt.Text = "目标 max/rows";
            this._colTgt.Width = 80;
            // 
            // _lblBak
            // 
            this._lblBak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._lblBak.AutoSize = true;
            this._lblBak.Location = new System.Drawing.Point(959, 387);
            this._lblBak.Name = "_lblBak";
            this._lblBak.Size = new System.Drawing.Size(187, 15);
            this._lblBak.TabIndex = 23;
            this._lblBak.Text = "可用备份点（最新在前）：";
            // 
            // _lstBackups
            // 
            this._lstBackups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._lstBackups.FormattingEnabled = true;
            this._lstBackups.HorizontalScrollbar = true;
            this._lstBackups.ItemHeight = 15;
            this._lstBackups.Location = new System.Drawing.Point(965, 405);
            this._lstBackups.Name = "_lstBackups";
            this._lstBackups.Size = new System.Drawing.Size(301, 139);
            this._lstBackups.TabIndex = 20;
            // 
            // _prog
            // 
            this._prog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._prog.Location = new System.Drawing.Point(14, 556);
            this._prog.Name = "_prog";
            this._prog.Size = new System.Drawing.Size(1259, 20);
            this._prog.TabIndex = 21;
            // 
            // _txtLog
            // 
            this._txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtLog.Location = new System.Drawing.Point(14, 582);
            this._txtLog.Name = "_txtLog";
            this._txtLog.ReadOnly = true;
            this._txtLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this._txtLog.Size = new System.Drawing.Size(1259, 104);
            this._txtLog.TabIndex = 22;
            this._txtLog.Text = "";
            // 
            // ConfigMigrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1289, 713);
            this.Controls.Add(this._txtLog);
            this.Controls.Add(this._prog);
            this.Controls.Add(this._lstBackups);
            this.Controls.Add(this._lblBak);
            this.Controls.Add(this._lvDetect);
            this.Controls.Add(this._lblVerdict);
            this.Controls.Add(this._btnVerify);
            this.Controls.Add(this._btnRestore);
            this.Controls.Add(this._btnMigrate);
            this.Controls.Add(this._btnBackup);
            this.Controls.Add(this._btnDetect);
            this.Controls.Add(this._grpScope);
            this.Controls.Add(this._grpTgt);
            this.Controls.Add(this._txtSrc);
            this.Controls.Add(this._lblSrc);
            this.MinimumSize = new System.Drawing.Size(1003, 760);
            this.Name = "ConfigMigrationForm";
            this.Text = "配置迁移（测试库 → 正式库）";
            this._grpTgt.ResumeLayout(false);
            this._grpTgt.PerformLayout();
            this._grpScope.ResumeLayout(false);
            this._grpScope.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}