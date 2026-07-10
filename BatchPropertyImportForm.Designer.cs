using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class BatchPropertyImportForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this._lblTitle = new System.Windows.Forms.Label();
            this._lblClassification = new System.Windows.Forms.Label();
            this._cmbClassification = new System.Windows.Forms.ComboBox();
            this._lblPropertyCount = new System.Windows.Forms.Label();
            this._txtPropertyList = new System.Windows.Forms.TextBox();
            this._lblExcelHint = new System.Windows.Forms.Label();
            this._btnSelectExcel = new System.Windows.Forms.Button();
            this._btnGenerateTemplate = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._lblProgress = new System.Windows.Forms.Label();
            this._grpResult = new System.Windows.Forms.GroupBox();
            this._txtErrors = new System.Windows.Forms.TextBox();
            this._lblResult = new System.Windows.Forms.Label();
            this._lblColumnFormat = new System.Windows.Forms.Label();
            this._grpResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblTitle
            // 
            this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this._lblTitle.Location = new System.Drawing.Point(20, 20);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new System.Drawing.Size(560, 32);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "批量导入属性值";
            this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblClassification
            // 
            this._lblClassification.AutoSize = true;
            this._lblClassification.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._lblClassification.Location = new System.Drawing.Point(20, 68);
            this._lblClassification.Name = "_lblClassification";
            this._lblClassification.Size = new System.Drawing.Size(95, 23);
            this._lblClassification.TabIndex = 1;
            this._lblClassification.Text = "选择分类：";
            // 
            // _cmbClassification
            // 
            this._cmbClassification.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbClassification.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._cmbClassification.FormattingEnabled = true;
            this._cmbClassification.Location = new System.Drawing.Point(96, 65);
            this._cmbClassification.Name = "_cmbClassification";
            this._cmbClassification.Size = new System.Drawing.Size(360, 31);
            this._cmbClassification.TabIndex = 2;
            // 
            // _lblPropertyCount
            // 
            this._lblPropertyCount.AutoSize = true;
            this._lblPropertyCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblPropertyCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._lblPropertyCount.Location = new System.Drawing.Point(20, 104);
            this._lblPropertyCount.Name = "_lblPropertyCount";
            this._lblPropertyCount.Size = new System.Drawing.Size(121, 20);
            this._lblPropertyCount.TabIndex = 3;
            this._lblPropertyCount.Text = "该分类下属性：";
            // 
            // _txtPropertyList
            // 
            this._txtPropertyList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._txtPropertyList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._txtPropertyList.Font = new System.Drawing.Font("Consolas", 9F);
            this._txtPropertyList.Location = new System.Drawing.Point(96, 129);
            this._txtPropertyList.Multiline = true;
            this._txtPropertyList.Name = "_txtPropertyList";
            this._txtPropertyList.ReadOnly = true;
            this._txtPropertyList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtPropertyList.Size = new System.Drawing.Size(360, 518);
            this._txtPropertyList.TabIndex = 5;
            this._txtPropertyList.Text = "（选择分类后显示该分类下的属性列表及对应 Excel 列顺序）";
            // 
            // _lblExcelHint
            // 
            this._lblExcelHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblExcelHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblExcelHint.Location = new System.Drawing.Point(462, 171);
            this._lblExcelHint.Name = "_lblExcelHint";
            this._lblExcelHint.Size = new System.Drawing.Size(502, 47);
            this._lblExcelHint.TabIndex = 7;
            this._lblExcelHint.Text = "首行为属性名（如 B1B2特征、B3特征），第2行为编号列和属性值列说明。第三行为数据开始行，支持用户自行导入，只要遵循此格式即可导入。";
            // 
            // _btnSelectExcel
            // 
            this._btnSelectExcel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnSelectExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSelectExcel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._btnSelectExcel.ForeColor = System.Drawing.Color.White;
            this._btnSelectExcel.Location = new System.Drawing.Point(462, 129);
            this._btnSelectExcel.Name = "_btnSelectExcel";
            this._btnSelectExcel.Size = new System.Drawing.Size(220, 38);
            this._btnSelectExcel.TabIndex = 8;
            this._btnSelectExcel.Text = "选择 Excel 并导入";
            this._btnSelectExcel.UseVisualStyleBackColor = false;
            // 
            // _btnGenerateTemplate
            // 
            this._btnGenerateTemplate.BackColor = System.Drawing.Color.Gray;
            this._btnGenerateTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnGenerateTemplate.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._btnGenerateTemplate.ForeColor = System.Drawing.Color.White;
            this._btnGenerateTemplate.Location = new System.Drawing.Point(688, 130);
            this._btnGenerateTemplate.Name = "_btnGenerateTemplate";
            this._btnGenerateTemplate.Size = new System.Drawing.Size(180, 38);
            this._btnGenerateTemplate.TabIndex = 8;
            this._btnGenerateTemplate.Text = "生成 Excel 模板";
            this._btnGenerateTemplate.UseVisualStyleBackColor = false;
            // 
            // _btnCancel
            // 
            this._btnCancel.BackColor = System.Drawing.Color.Red;
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnCancel.Location = new System.Drawing.Point(874, 129);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(90, 38);
            this._btnCancel.TabIndex = 9;
            this._btnCancel.Text = "取消";
            this._btnCancel.UseVisualStyleBackColor = false;
            // 
            // _lblStatus
            // 
            this._lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblStatus.Location = new System.Drawing.Point(462, 270);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(400, 20);
            this._lblStatus.TabIndex = 10;
            this._lblStatus.Text = "就绪";
            // 
            // _progressBar
            // 
            this._progressBar.Location = new System.Drawing.Point(466, 302);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(520, 20);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 11;
            // 
            // _lblProgress
            // 
            this._lblProgress.Font = new System.Drawing.Font("Consolas", 9F);
            this._lblProgress.Location = new System.Drawing.Point(529, 272);
            this._lblProgress.Name = "_lblProgress";
            this._lblProgress.Size = new System.Drawing.Size(90, 16);
            this._lblProgress.TabIndex = 12;
            this._lblProgress.Text = "...";
            // 
            // _grpResult
            // 
            this._grpResult.Controls.Add(this._txtErrors);
            this._grpResult.Controls.Add(this._lblResult);
            this._grpResult.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._grpResult.Location = new System.Drawing.Point(466, 382);
            this._grpResult.Name = "_grpResult";
            this._grpResult.Size = new System.Drawing.Size(530, 166);
            this._grpResult.TabIndex = 13;
            this._grpResult.TabStop = false;
            this._grpResult.Text = "  导入结果  ";
            // 
            // _txtErrors
            // 
            this._txtErrors.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._txtErrors.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._txtErrors.Font = new System.Drawing.Font("Consolas", 9F);
            this._txtErrors.Location = new System.Drawing.Point(8, 50);
            this._txtErrors.Multiline = true;
            this._txtErrors.Name = "_txtErrors";
            this._txtErrors.ReadOnly = true;
            this._txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtErrors.Size = new System.Drawing.Size(512, 108);
            this._txtErrors.TabIndex = 1;
            this._txtErrors.Text = "请先选择分类，然后点击「选择 Excel 并导入」或「生成 Excel 模板」...";
            // 
            // _lblResult
            // 
            this._lblResult.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblResult.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._lblResult.Location = new System.Drawing.Point(10, 22);
            this._lblResult.Name = "_lblResult";
            this._lblResult.Size = new System.Drawing.Size(400, 20);
            this._lblResult.TabIndex = 0;
            this._lblResult.Text = "成功：0  失败：0";
            // 
            // _lblColumnFormat
            // 
            this._lblColumnFormat.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblColumnFormat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this._lblColumnFormat.Location = new System.Drawing.Point(462, 218);
            this._lblColumnFormat.Name = "_lblColumnFormat";
            this._lblColumnFormat.Size = new System.Drawing.Size(502, 52);
            this._lblColumnFormat.TabIndex = 6;
            this._lblColumnFormat.Text = "Excel 列格式：A列=属性编号(PROPERTY_NUMBER)，B列=属性值(PROPERTY_VALUE)，每属性占2列";
            // 
            // BatchPropertyImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1026, 659);
            this.Controls.Add(this._txtPropertyList);
            this.Controls.Add(this._grpResult);
            this.Controls.Add(this._lblProgress);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnGenerateTemplate);
            this.Controls.Add(this._btnSelectExcel);
            this.Controls.Add(this._lblExcelHint);
            this.Controls.Add(this._lblColumnFormat);
            this.Controls.Add(this._lblPropertyCount);
            this.Controls.Add(this._cmbClassification);
            this.Controls.Add(this._lblClassification);
            this.Controls.Add(this._lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BatchPropertyImportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "批量导入属性值";
            this._grpResult.ResumeLayout(false);
            this._grpResult.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        // 控件字段
        private Label _lblTitle;
        private Label _lblClassification;
        private ComboBox _cmbClassification;
        private Label _lblPropertyCount;
        private TextBox _txtPropertyList;
        private Label _lblColumnFormat;
        private Label _lblExcelHint;
        private Button _btnSelectExcel;
        private Button _btnGenerateTemplate;
        private Button _btnCancel;
        private Label _lblStatus;
        private ProgressBar _progressBar;
        private Label _lblProgress;
        private GroupBox _grpResult;
        private Label _lblResult;
        private TextBox _txtErrors;
    }
}
