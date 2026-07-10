namespace DataImportTool
{
    partial class MatchRuleConfigForm
    {
        /// <summary>必需的设计器变量。</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>清理所有正在使用的资源。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>设计器支持所需的方法 - 不要使用代码编辑器修改此方法的内容。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._lblSourceType = new System.Windows.Forms.Label();
            this._cmbSourceType = new System.Windows.Forms.ComboBox();
            this._lblStatus = new System.Windows.Forms.Label();
            this.lblRules = new System.Windows.Forms.Label();
            this._lstRules = new System.Windows.Forms.ListBox();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._lblClass = new System.Windows.Forms.Label();
            this._cmbClass = new System.Windows.Forms.ComboBox();
            this._chkStrict = new System.Windows.Forms.CheckBox();
            this._lblSearch = new System.Windows.Forms.Label();
            this._txtSearch = new System.Windows.Forms.TextBox();
            this.lblFields = new System.Windows.Forms.Label();
            this._clbFields = new System.Windows.Forms.CheckedListBox();
            this._btnSelectAll = new System.Windows.Forms.Button();
            this._btnClearFields = new System.Windows.Forms.Button();
            this._btnDelete = new System.Windows.Forms.Button();
            this._btnPreSave = new System.Windows.Forms.Button();
            this._btnLoad = new System.Windows.Forms.Button();
            this._btnSave = new System.Windows.Forms.Button();
            this._btnClose = new System.Windows.Forms.Button();
            this._btnExcludeFields = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _lblSourceType
            // 
            this._lblSourceType.AutoSize = true;
            this._lblSourceType.Location = new System.Drawing.Point(20, 16);
            this._lblSourceType.Name = "_lblSourceType";
            this._lblSourceType.Size = new System.Drawing.Size(82, 15);
            this._lblSourceType.TabIndex = 0;
            this._lblSourceType.Text = "生产类型：";
            // 
            // _cmbSourceType
            // 
            this._cmbSourceType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbSourceType.Location = new System.Drawing.Point(95, 14);
            this._cmbSourceType.Name = "_cmbSourceType";
            this._cmbSourceType.Size = new System.Drawing.Size(180, 23);
            this._cmbSourceType.TabIndex = 1;
            // 
            // _lblStatus
            // 
            this._lblStatus.AutoSize = true;
            this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this._lblStatus.Location = new System.Drawing.Point(360, 18);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(0, 15);
            this._lblStatus.TabIndex = 2;
            // 
            // lblRules
            // 
            this.lblRules.AutoSize = true;
            this.lblRules.Location = new System.Drawing.Point(20, 48);
            this.lblRules.Name = "lblRules";
            this.lblRules.Size = new System.Drawing.Size(240, 15);
            this.lblRules.TabIndex = 3;
            this.lblRules.Text = "制造件分类规则（绿=已预保存）：";
            // 
            // _lstRules
            // 
            this._lstRules.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._lstRules.HorizontalScrollbar = true;
            this._lstRules.ItemHeight = 15;
            this._lstRules.Location = new System.Drawing.Point(20, 68);
            this._lstRules.Name = "_lstRules";
            this._lstRules.Size = new System.Drawing.Size(250, 409);
            this._lstRules.TabIndex = 4;
            // 
            // _toolTip
            // 
            this._toolTip.AutoPopDelay = 8000;
            this._toolTip.InitialDelay = 500;
            this._toolTip.ReshowDelay = 100;
            this._toolTip.ShowAlways = true;
            // 
            // _lblClass
            // 
            this._lblClass.AutoSize = true;
            this._lblClass.Location = new System.Drawing.Point(300, 48);
            this._lblClass.Name = "_lblClass";
            this._lblClass.Size = new System.Drawing.Size(200, 15);
            this._lblClass.TabIndex = 5;
            this._lblClass.Text = "零件分类表单(object_id)：";
            // 
            // _cmbClass
            // 
            this._cmbClass.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._cmbClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbClass.Location = new System.Drawing.Point(300, 68);
            this._cmbClass.Name = "_cmbClass";
            this._cmbClass.Size = new System.Drawing.Size(400, 26);
            this._cmbClass.TabIndex = 6;
            // 
            // _chkStrict
            // 
            this._chkStrict.AutoSize = true;
            this._chkStrict.Location = new System.Drawing.Point(300, 100);
            this._chkStrict.Name = "_chkStrict";
            this._chkStrict.Size = new System.Drawing.Size(255, 19);
            this._chkStrict.TabIndex = 7;
            this._chkStrict.Text = "严格匹配（strict_match_flag）";
            // 
            // _lblSearch
            // 
            this._lblSearch.AutoSize = true;
            this._lblSearch.Location = new System.Drawing.Point(300, 132);
            this._lblSearch.Name = "_lblSearch";
            this._lblSearch.Size = new System.Drawing.Size(82, 15);
            this._lblSearch.TabIndex = 8;
            this._lblSearch.Text = "搜索属性：";
            // 
            // _txtSearch
            // 
            this._txtSearch.Location = new System.Drawing.Point(375, 130);
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.Size = new System.Drawing.Size(325, 25);
            this._txtSearch.TabIndex = 9;
            // 
            // lblFields
            // 
            this.lblFields.AutoSize = true;
            this.lblFields.Location = new System.Drawing.Point(300, 162);
            this.lblFields.Name = "lblFields";
            this.lblFields.Size = new System.Drawing.Size(256, 15);
            this.lblFields.TabIndex = 10;
            this.lblFields.Text = "匹配字段（勾选，17 个以内为佳）：";
            // 
            // _clbFields
            // 
            this._clbFields.CheckOnClick = true;
            this._clbFields.Location = new System.Drawing.Point(300, 182);
            this._clbFields.Name = "_clbFields";
            this._clbFields.Size = new System.Drawing.Size(400, 244);
            this._clbFields.TabIndex = 11;
            // 
            // _btnSelectAll
            // 
            this._btnSelectAll.Location = new System.Drawing.Point(300, 468);
            this._btnSelectAll.Name = "_btnSelectAll";
            this._btnSelectAll.Size = new System.Drawing.Size(90, 28);
            this._btnSelectAll.TabIndex = 12;
            this._btnSelectAll.Text = "全选(当前)";
            // 
            // _btnClearFields
            // 
            this._btnClearFields.Location = new System.Drawing.Point(396, 468);
            this._btnClearFields.Name = "_btnClearFields";
            this._btnClearFields.Size = new System.Drawing.Size(90, 28);
            this._btnClearFields.TabIndex = 13;
            this._btnClearFields.Text = "清空(当前)";
            // 
            // _btnDelete
            // 
            this._btnDelete.Location = new System.Drawing.Point(20, 496);
            this._btnDelete.Name = "_btnDelete";
            this._btnDelete.Size = new System.Drawing.Size(90, 30);
            this._btnDelete.TabIndex = 14;
            this._btnDelete.Text = "删除";
            // 
            // _btnPreSave
            // 
            this._btnPreSave.Location = new System.Drawing.Point(300, 432);
            this._btnPreSave.Name = "_btnPreSave";
            this._btnPreSave.Size = new System.Drawing.Size(90, 30);
            this._btnPreSave.TabIndex = 15;
            this._btnPreSave.Text = "预保存";
            // 
            // _btnLoad
            // 
            this._btnLoad.Location = new System.Drawing.Point(300, 588);
            this._btnLoad.Name = "_btnLoad";
            this._btnLoad.Size = new System.Drawing.Size(100, 32);
            this._btnLoad.TabIndex = 19;
            this._btnLoad.Text = "重新加载";
            // 
            // _btnSave
            // 
            this._btnSave.Location = new System.Drawing.Point(420, 588);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(130, 32);
            this._btnSave.TabIndex = 20;
            this._btnSave.Text = "保存到数据库";
            // 
            // _btnClose
            // 
            this._btnClose.Location = new System.Drawing.Point(566, 588);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(80, 32);
            this._btnClose.TabIndex = 21;
            this._btnClose.Text = "关闭";
            // 
            // _btnExcludeFields
            // 
            this._btnExcludeFields.Location = new System.Drawing.Point(300, 550);
            this._btnExcludeFields.Name = "_btnExcludeFields";
            this._btnExcludeFields.Size = new System.Drawing.Size(120, 30);
            this._btnExcludeFields.TabIndex = 22;
            this._btnExcludeFields.Text = "默认排除字段";
            // 
            // MatchRuleConfigForm
            // 
            this.ClientSize = new System.Drawing.Size(720, 640);
            this.Controls.Add(this._lblSourceType);
            this.Controls.Add(this._cmbSourceType);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this.lblRules);
            this.Controls.Add(this._lstRules);
            this.Controls.Add(this._lblClass);
            this.Controls.Add(this._cmbClass);
            this.Controls.Add(this._chkStrict);
            this.Controls.Add(this._lblSearch);
            this.Controls.Add(this._txtSearch);
            this.Controls.Add(this.lblFields);
            this.Controls.Add(this._clbFields);
            this.Controls.Add(this._btnSelectAll);
            this.Controls.Add(this._btnClearFields);
            this.Controls.Add(this._btnDelete);
            this.Controls.Add(this._btnPreSave);
            this.Controls.Add(this._btnLoad);
            this.Controls.Add(this._btnSave);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._btnExcludeFields);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MatchRuleConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "零件匹配规则配置（match_rule / part）";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
