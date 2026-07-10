using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class SuperBatchImportForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblTitle = new System.Windows.Forms.Label();
            this._lblTemplate = new System.Windows.Forms.Label();
            this._cmbTemplate = new System.Windows.Forms.ComboBox();
            this._chkGenerateForm = new System.Windows.Forms.CheckBox();
            this._btnSelectFile = new System.Windows.Forms.Button();
            this._txtFilePath = new System.Windows.Forms.TextBox();
            this._btnPreview = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._txtLog = new System.Windows.Forms.TextBox();
            this._lblResult = new System.Windows.Forms.Label();
            this._btnGenTemplate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _lblTitle
            // 
            this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblTitle.Location = new System.Drawing.Point(16, 12);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new System.Drawing.Size(560, 28);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "超级批量导入";
            // 
            // _lblTemplate
            // 
            this._lblTemplate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblTemplate.Location = new System.Drawing.Point(8, 80);
            this._lblTemplate.Name = "_lblTemplate";
            this._lblTemplate.Size = new System.Drawing.Size(257, 24);
            this._lblTemplate.TabIndex = 1;
            this._lblTemplate.Text = "选择要生成的分类零件表单模板：";
            this._lblTemplate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _cmbTemplate
            // 
            this._cmbTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbTemplate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbTemplate.Location = new System.Drawing.Point(227, 79);
            this._cmbTemplate.Name = "_cmbTemplate";
            this._cmbTemplate.Size = new System.Drawing.Size(196, 28);
            this._cmbTemplate.TabIndex = 2;
            this._cmbTemplate.SelectedIndexChanged += new System.EventHandler(this.CmbTemplate_SelectedIndexChanged);
            // 
            // _chkGenerateForm
            // 
            this._chkGenerateForm.AutoSize = true;
            this._chkGenerateForm.Checked = true;
            this._chkGenerateForm.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkGenerateForm.Location = new System.Drawing.Point(16, 48);
            this._chkGenerateForm.Name = "_chkGenerateForm";
            this._chkGenerateForm.Size = new System.Drawing.Size(115, 23);
            this._chkGenerateForm.TabIndex = 1;
            this._chkGenerateForm.Text = "生成分类表单";
            this._chkGenerateForm.UseVisualStyleBackColor = true;
            this._chkGenerateForm.CheckedChanged += new System.EventHandler(this.ChkGenerateForm_CheckedChanged);
            // 
            // _btnSelectFile
            // 
            this._btnSelectFile.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnSelectFile.Location = new System.Drawing.Point(8, 116);
            this._btnSelectFile.Name = "_btnSelectFile";
            this._btnSelectFile.Size = new System.Drawing.Size(120, 30);
            this._btnSelectFile.TabIndex = 3;
            this._btnSelectFile.Text = "选择 Excel 文件";
            this._btnSelectFile.UseVisualStyleBackColor = true;
            this._btnSelectFile.Click += new System.EventHandler(this.BtnSelectFile_Click);
            // 
            // _txtFilePath
            // 
            this._txtFilePath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._txtFilePath.Location = new System.Drawing.Point(139, 118);
            this._txtFilePath.Name = "_txtFilePath";
            this._txtFilePath.ReadOnly = true;
            this._txtFilePath.Size = new System.Drawing.Size(485, 27);
            this._txtFilePath.TabIndex = 4;
            // 
            // _btnPreview
            // 
            this._btnPreview.AutoSize = true;
            this._btnPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnPreview.Enabled = false;
            this._btnPreview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnPreview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnPreview.ForeColor = System.Drawing.Color.White;
            this._btnPreview.Location = new System.Drawing.Point(272, 153);
            this._btnPreview.Name = "_btnPreview";
            this._btnPreview.Size = new System.Drawing.Size(127, 32);
            this._btnPreview.TabIndex = 6;
            this._btnPreview.Text = "开始导入校验";
            this._btnPreview.UseVisualStyleBackColor = false;
            this._btnPreview.Click += new System.EventHandler(this.BtnPreview_Click);
            // 
            // _btnCancel
            // 
            this._btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnCancel.Location = new System.Drawing.Point(139, 153);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(127, 29);
            this._btnCancel.TabIndex = 8;
            this._btnCancel.Text = "取消";
            this._btnCancel.UseVisualStyleBackColor = true;
            this._btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // _lblStatus
            // 
            this._lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(80)))), ((int)(((byte)(87)))));
            this._lblStatus.Location = new System.Drawing.Point(16, 236);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(249, 21);
            this._lblStatus.TabIndex = 9;
            this._lblStatus.Text = "就绪";
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _progressBar
            // 
            this._progressBar.Location = new System.Drawing.Point(16, 260);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(560, 22);
            this._progressBar.TabIndex = 10;
            // 
            // _txtLog
            // 
            this._txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this._txtLog.Location = new System.Drawing.Point(16, 288);
            this._txtLog.Multiline = true;
            this._txtLog.Name = "_txtLog";
            this._txtLog.ReadOnly = true;
            this._txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtLog.Size = new System.Drawing.Size(560, 256);
            this._txtLog.TabIndex = 11;
            // 
            // _lblResult
            // 
            this._lblResult.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblResult.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(80)))), ((int)(((byte)(87)))));
            this._lblResult.Location = new System.Drawing.Point(16, 552);
            this._lblResult.Name = "_lblResult";
            this._lblResult.Size = new System.Drawing.Size(560, 20);
            this._lblResult.TabIndex = 12;
            this._lblResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _btnGenTemplate
            // 
            this._btnGenTemplate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnGenTemplate.Location = new System.Drawing.Point(8, 152);
            this._btnGenTemplate.Name = "_btnGenTemplate";
            this._btnGenTemplate.Size = new System.Drawing.Size(120, 30);
            this._btnGenTemplate.TabIndex = 5;
            this._btnGenTemplate.Text = "生成导入模板";
            this._btnGenTemplate.UseVisualStyleBackColor = true;
            this._btnGenTemplate.Click += new System.EventHandler(this.BtnGenTemplate_Click);
            // 
            // SuperBatchImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(639, 584);
            this.Controls.Add(this._lblResult);
            this.Controls.Add(this._txtLog);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnPreview);
            this.Controls.Add(this._btnGenTemplate);
            this.Controls.Add(this._txtFilePath);
            this.Controls.Add(this._btnSelectFile);
            this.Controls.Add(this._cmbTemplate);
            this.Controls.Add(this._lblTemplate);
            this.Controls.Add(this._chkGenerateForm);
            this.Controls.Add(this._lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "SuperBatchImportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "超级批量导入";
            this.Load += new System.EventHandler(this.SuperBatchImportForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #region 控件字段声明
        private System.Windows.Forms.Label _lblTitle;
        private System.Windows.Forms.Label _lblTemplate;
        private System.Windows.Forms.ComboBox _cmbTemplate;
        private System.Windows.Forms.CheckBox _chkGenerateForm;
        private System.Windows.Forms.Button _btnSelectFile;
        private System.Windows.Forms.TextBox _txtFilePath;
        private System.Windows.Forms.Button _btnGenTemplate;
        private System.Windows.Forms.Button _btnPreview;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.TextBox _txtLog;
        private System.Windows.Forms.Label _lblResult;
        #endregion
    }
}
