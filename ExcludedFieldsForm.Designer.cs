namespace DataImportTool
{
    partial class ExcludedFieldsForm
    {
        /// <summary>必需的设计器变量。</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>清理所有正在使用的资源。</summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>设计器支持所需的方法 - 不要修改使用代码编辑器修改此方法的内容。</summary>
        private void InitializeComponent()
        {
            this._lblTitle = new System.Windows.Forms.Label();
            this._txtSearch = new System.Windows.Forms.TextBox();
            this._lblCount = new System.Windows.Forms.Label();
            this._clb = new System.Windows.Forms.CheckedListBox();
            this._btnAll = new System.Windows.Forms.Button();
            this._btnNone = new System.Windows.Forms.Button();
            this._btnSave = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // _lblTitle
            //
            this._lblTitle.Location = new System.Drawing.Point(12, 12);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new System.Drawing.Size(436, 32);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "勾选需要“默认排除”的字段。保存后，新建分类的匹配规则将自动剔除这些字段（手动勾选仍可加回）。";
            //
            // _txtSearch
            //
            this._txtSearch.Location = new System.Drawing.Point(12, 50);
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.Size = new System.Drawing.Size(300, 25);
            this._txtSearch.TabIndex = 1;
            this._txtSearch.TextChanged += new System.EventHandler(this._txtSearch_TextChanged);
            //
            // _lblCount
            //
            this._lblCount.Location = new System.Drawing.Point(320, 53);
            this._lblCount.Name = "_lblCount";
            this._lblCount.Size = new System.Drawing.Size(128, 20);
            this._lblCount.TabIndex = 2;
            //
            // _clb
            //
            this._clb.CheckOnClick = true;
            this._clb.Location = new System.Drawing.Point(12, 80);
            this._clb.Name = "_clb";
            this._clb.Size = new System.Drawing.Size(436, 420);
            this._clb.TabIndex = 3;
            this._clb.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this._clb_ItemCheck);
            //
            // _btnAll
            //
            this._btnAll.Location = new System.Drawing.Point(12, 510);
            this._btnAll.Name = "_btnAll";
            this._btnAll.Size = new System.Drawing.Size(80, 30);
            this._btnAll.TabIndex = 4;
            this._btnAll.Text = "全选";
            this._btnAll.Click += new System.EventHandler(this._btnAll_Click);
            //
            // _btnNone
            //
            this._btnNone.Location = new System.Drawing.Point(100, 510);
            this._btnNone.Name = "_btnNone";
            this._btnNone.Size = new System.Drawing.Size(80, 30);
            this._btnNone.TabIndex = 5;
            this._btnNone.Text = "全不选";
            this._btnNone.Click += new System.EventHandler(this._btnNone_Click);
            //
            // _btnSave
            //
            this._btnSave.Location = new System.Drawing.Point(280, 510);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(75, 30);
            this._btnSave.TabIndex = 6;
            this._btnSave.Text = "保存";
            this._btnSave.Click += new System.EventHandler(this._btnSave_Click);
            //
            // _btnCancel
            //
            this._btnCancel.Location = new System.Drawing.Point(363, 510);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 30);
            this._btnCancel.TabIndex = 7;
            this._btnCancel.Text = "取消";
            this._btnCancel.Click += new System.EventHandler(this._btnCancel_Click);
            //
            // _lblStatus
            //
            this._lblStatus.ForeColor = System.Drawing.Color.DarkGreen;
            this._lblStatus.Location = new System.Drawing.Point(12, 548);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(436, 20);
            this._lblStatus.TabIndex = 8;
            //
            // ExcludedFieldsForm
            //
            this.ClientSize = new System.Drawing.Size(460, 585);
            this.Controls.Add(this._lblTitle);
            this.Controls.Add(this._txtSearch);
            this.Controls.Add(this._lblCount);
            this.Controls.Add(this._clb);
            this.Controls.Add(this._btnAll);
            this.Controls.Add(this._btnNone);
            this.Controls.Add(this._btnSave);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExcludedFieldsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "默认排除字段";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _lblTitle;
        private System.Windows.Forms.TextBox _txtSearch;
        private System.Windows.Forms.Label _lblCount;
        private System.Windows.Forms.CheckedListBox _clb;
        private System.Windows.Forms.Button _btnAll;
        private System.Windows.Forms.Button _btnNone;
        private System.Windows.Forms.Button _btnSave;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Label _lblStatus;
    }
}
