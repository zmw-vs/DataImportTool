using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{

    partial class LogViewerForm
    {
        private IContainer components = null;

        private Panel _pnlTop;
        private Label _lblSearch;
        private TextBox _txtSearch;
        private Label _lblModule;
        private ComboBox _cmbModule;
        private Button _btnRefresh;
        private Button _btnClear;
        private Button _btnClose;
        private DataGridView _dgvLogs;
        private Label _lblStatus;
        private DataGridViewTextBoxColumn _colTime;
        private DataGridViewTextBoxColumn _colModule;
        private DataGridViewTextBoxColumn _colType;
        private DataGridViewTextBoxColumn _colMessage;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this._pnlTop = new System.Windows.Forms.Panel();
            this._lblSearch = new System.Windows.Forms.Label();
            this._txtSearch = new System.Windows.Forms.TextBox();
            this._lblModule = new System.Windows.Forms.Label();
            this._cmbModule = new System.Windows.Forms.ComboBox();
            this._btnRefresh = new System.Windows.Forms.Button();
            this._btnClear = new System.Windows.Forms.Button();
            this._btnClose = new System.Windows.Forms.Button();
            this._dgvLogs = new System.Windows.Forms.DataGridView();
            this._colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._colModule = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._colMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._lblStatus = new System.Windows.Forms.Label();
            this._pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dgvLogs)).BeginInit();
            this.SuspendLayout();
            //
            // _pnlTop
            //
            this._pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this._pnlTop.Controls.Add(this._lblSearch);
            this._pnlTop.Controls.Add(this._txtSearch);
            this._pnlTop.Controls.Add(this._lblModule);
            this._pnlTop.Controls.Add(this._cmbModule);
            this._pnlTop.Controls.Add(this._btnRefresh);
            this._pnlTop.Controls.Add(this._btnClear);
            this._pnlTop.Controls.Add(this._btnClose);
            this._pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this._pnlTop.Location = new System.Drawing.Point(0, 0);
            this._pnlTop.Name = "_pnlTop";
            this._pnlTop.Size = new System.Drawing.Size(1000, 56);
            this._pnlTop.TabIndex = 0;
            //
            // _lblSearch
            //
            this._lblSearch.AutoSize = true;
            this._lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblSearch.Location = new System.Drawing.Point(1, 20);
            this._lblSearch.Name = "_lblSearch";
            this._lblSearch.Size = new System.Drawing.Size(57, 20);
            this._lblSearch.TabIndex = 0;
            this._lblSearch.Text = "搜索：";
            //
            // _txtSearch
            //
            this._txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._txtSearch.Location = new System.Drawing.Point(64, 16);
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.Size = new System.Drawing.Size(232, 27);
            this._txtSearch.TabIndex = 1;
            this._txtSearch.TextChanged += new System.EventHandler(this._txtSearch_TextChanged);
            //
            // _lblModule
            //
            this._lblModule.AutoSize = true;
            this._lblModule.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblModule.Location = new System.Drawing.Point(312, 20);
            this._lblModule.Name = "_lblModule";
            this._lblModule.Size = new System.Drawing.Size(57, 20);
            this._lblModule.TabIndex = 2;
            this._lblModule.Text = "模块：";
            //
            // _cmbModule
            //
            this._cmbModule.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbModule.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbModule.Location = new System.Drawing.Point(368, 16);
            this._cmbModule.Name = "_cmbModule";
            this._cmbModule.Size = new System.Drawing.Size(180, 28);
            this._cmbModule.TabIndex = 3;
            this._cmbModule.SelectedIndexChanged += new System.EventHandler(this._cmbModule_SelectedIndexChanged);
            //
            // _btnRefresh
            //
            this._btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnRefresh.Location = new System.Drawing.Point(572, 14);
            this._btnRefresh.Name = "_btnRefresh";
            this._btnRefresh.Size = new System.Drawing.Size(75, 28);
            this._btnRefresh.TabIndex = 4;
            this._btnRefresh.Text = "刷新";
            this._btnRefresh.UseVisualStyleBackColor = true;
            this._btnRefresh.Click += new System.EventHandler(this._btnRefresh_Click);
            //
            // _btnClear
            //
            this._btnClear.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this._btnClear.Location = new System.Drawing.Point(660, 14);
            this._btnClear.Name = "_btnClear";
            this._btnClear.Size = new System.Drawing.Size(75, 28);
            this._btnClear.TabIndex = 5;
            this._btnClear.Text = "清空日志";
            this._btnClear.UseVisualStyleBackColor = true;
            this._btnClear.Click += new System.EventHandler(this._btnClear_Click);
            //
            // _btnClose
            //
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnClose.Location = new System.Drawing.Point(904, 14);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(80, 28);
            this._btnClose.TabIndex = 6;
            this._btnClose.Text = "关闭";
            this._btnClose.UseVisualStyleBackColor = true;
            this._btnClose.Click += new System.EventHandler(this._btnClose_Click);
            //
            // _dgvLogs
            //
            this._dgvLogs.AllowUserToAddRows = false;
            this._dgvLogs.AllowUserToDeleteRows = false;
            this._dgvLogs.AllowUserToResizeRows = false;
            this._dgvLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dgvLogs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._dgvLogs.BackgroundColor = System.Drawing.Color.White;
            this._dgvLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._dgvLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dgvLogs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colTime,
            this._colModule,
            this._colType,
            this._colMessage});
            this._dgvLogs.Location = new System.Drawing.Point(0, 56);
            this._dgvLogs.Name = "_dgvLogs";
            this._dgvLogs.ReadOnly = true;
            this._dgvLogs.RowHeadersVisible = false;
            this._dgvLogs.RowHeadersWidth = 51;
            this._dgvLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._dgvLogs.Size = new System.Drawing.Size(1000, 482);
            this._dgvLogs.TabIndex = 1;
            this._dgvLogs.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this._dgvLogs_CellDoubleClick);
            //
            // _colTime
            //
            this._colTime.FillWeight = 25F;
            this._colTime.HeaderText = "时间";
            this._colTime.MinimumWidth = 6;
            this._colTime.Name = "_colTime";
            this._colTime.ReadOnly = true;
            //
            // _colModule
            //
            this._colModule.FillWeight = 15F;
            this._colModule.HeaderText = "模块";
            this._colModule.MinimumWidth = 6;
            this._colModule.Name = "_colModule";
            this._colModule.ReadOnly = true;
            //
            // _colType
            //
            this._colType.FillWeight = 10F;
            this._colType.HeaderText = "类型";
            this._colType.MinimumWidth = 6;
            this._colType.Name = "_colType";
            this._colType.ReadOnly = true;
            //
            // _colMessage
            //
            this._colMessage.FillWeight = 50F;
            this._colMessage.HeaderText = "消息";
            this._colMessage.MinimumWidth = 6;
            this._colMessage.Name = "_colMessage";
            this._colMessage.ReadOnly = true;
            //
            // _lblStatus
            //
            this._lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._lblStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this._lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblStatus.Location = new System.Drawing.Point(0, 538);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this._lblStatus.Size = new System.Drawing.Size(1000, 24);
            this._lblStatus.TabIndex = 2;
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // LogViewerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1000, 562);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._dgvLogs);
            this.Controls.Add(this._pnlTop);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "LogViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "操作日志";
            this._pnlTop.ResumeLayout(false);
            this._pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dgvLogs)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }
    }
}
