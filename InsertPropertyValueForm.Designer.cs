using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class InsertPropertyValueForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._rbRoot = new System.Windows.Forms.RadioButton();
            this._rbClassification = new System.Windows.Forms.RadioButton();
            this._lblProperty = new System.Windows.Forms.Label();
            this._cmbProperty = new System.Windows.Forms.ComboBox();
            this._lblClassification = new System.Windows.Forms.Label();
            this._cmbClassification = new System.Windows.Forms.ComboBox();
            this._btnImport = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._lblProgress = new System.Windows.Forms.Label();
            this._lblStatus = new System.Windows.Forms.Label();
            this._grpResult = new System.Windows.Forms.GroupBox();
            this._txtErrors = new System.Windows.Forms.TextBox();
            this._lblResult = new System.Windows.Forms.Label();
            this._lblMode = new System.Windows.Forms.Label();
            this._lblExcelHint = new System.Windows.Forms.Label();
            this._grpResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // _rbRoot
            // 
            this._rbRoot.Checked = true;
            this._rbRoot.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._rbRoot.Location = new System.Drawing.Point(16, 42);
            this._rbRoot.Name = "_rbRoot";
            this._rbRoot.Size = new System.Drawing.Size(220, 20);
            this._rbRoot.TabIndex = 1;
            this._rbRoot.TabStop = true;
            this._rbRoot.Text = "插入根目录的属性值";
            this._rbRoot.CheckedChanged += new System.EventHandler(this._rbRoot_CheckedChanged);
            // 
            // _rbClassification
            // 
            this._rbClassification.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._rbClassification.Location = new System.Drawing.Point(16, 68);
            this._rbClassification.Name = "_rbClassification";
            this._rbClassification.Size = new System.Drawing.Size(220, 20);
            this._rbClassification.TabIndex = 2;
            this._rbClassification.Text = "插入分类目录下的属性值";
            this._rbClassification.CheckedChanged += new System.EventHandler(this._rbClassification_CheckedChanged);
            // 
            // _lblProperty
            // 
            this._lblProperty.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblProperty.Location = new System.Drawing.Point(30, 164);
            this._lblProperty.Name = "_lblProperty";
            this._lblProperty.Size = new System.Drawing.Size(98, 21);
            this._lblProperty.TabIndex = 5;
            this._lblProperty.Text = "选择属性：";
            // 
            // _cmbProperty
            // 
            this._cmbProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbProperty.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbProperty.FormattingEnabled = true;
            this._cmbProperty.Location = new System.Drawing.Point(30, 188);
            this._cmbProperty.Name = "_cmbProperty";
            this._cmbProperty.Size = new System.Drawing.Size(460, 28);
            this._cmbProperty.TabIndex = 6;
            // 
            // _lblClassification
            // 
            this._lblClassification.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblClassification.Location = new System.Drawing.Point(30, 100);
            this._lblClassification.Name = "_lblClassification";
            this._lblClassification.Size = new System.Drawing.Size(98, 21);
            this._lblClassification.TabIndex = 3;
            this._lblClassification.Text = "所属分类：";
            this._lblClassification.Visible = false;
            // 
            // _cmbClassification
            // 
            this._cmbClassification.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbClassification.Enabled = false;
            this._cmbClassification.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbClassification.FormattingEnabled = true;
            this._cmbClassification.Location = new System.Drawing.Point(30, 124);
            this._cmbClassification.Name = "_cmbClassification";
            this._cmbClassification.Size = new System.Drawing.Size(460, 28);
            this._cmbClassification.TabIndex = 4;
            this._cmbClassification.Visible = false;
            this._cmbClassification.SelectedIndexChanged += new System.EventHandler(this._cmbClassification_SelectedIndexChanged);
            // 
            // _btnImport
            // 
            this._btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnImport.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._btnImport.ForeColor = System.Drawing.Color.White;
            this._btnImport.Location = new System.Drawing.Point(30, 267);
            this._btnImport.Name = "_btnImport";
            this._btnImport.Size = new System.Drawing.Size(206, 38);
            this._btnImport.TabIndex = 8;
            this._btnImport.Text = "📂 选择 Excel 并导入";
            this._btnImport.UseVisualStyleBackColor = false;
            this._btnImport.Click += new System.EventHandler(this.BtnImport_Click);
            // 
            // _btnCancel
            // 
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnCancel.Location = new System.Drawing.Point(256, 267);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(90, 38);
            this._btnCancel.TabIndex = 9;
            this._btnCancel.Text = "取消";
            this._btnCancel.UseVisualStyleBackColor = true;
            this._btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // _progressBar
            // 
            this._progressBar.Location = new System.Drawing.Point(16, 334);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(520, 20);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 11;
            // 
            // _lblProgress
            // 
            this._lblProgress.Font = new System.Drawing.Font("Consolas", 9F);
            this._lblProgress.Location = new System.Drawing.Point(546, 337);
            this._lblProgress.Name = "_lblProgress";
            this._lblProgress.Size = new System.Drawing.Size(100, 16);
            this._lblProgress.TabIndex = 12;
            // 
            // _lblStatus
            // 
            this._lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblStatus.Location = new System.Drawing.Point(16, 310);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(200, 20);
            this._lblStatus.TabIndex = 10;
            this._lblStatus.Text = "就绪";
            // 
            // _grpResult
            // 
            this._grpResult.Controls.Add(this._txtErrors);
            this._grpResult.Controls.Add(this._lblResult);
            this._grpResult.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._grpResult.Location = new System.Drawing.Point(16, 362);
            this._grpResult.Name = "_grpResult";
            this._grpResult.Size = new System.Drawing.Size(620, 174);
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
            this._txtErrors.Size = new System.Drawing.Size(604, 116);
            this._txtErrors.TabIndex = 1;
            this._txtErrors.Text = "请先选择属性，然后点击「选择 Excel 并导入属性值」...";
            // 
            // _lblResult
            // 
            this._lblResult.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblResult.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._lblResult.Location = new System.Drawing.Point(10, 22);
            this._lblResult.Name = "_lblResult";
            this._lblResult.Size = new System.Drawing.Size(200, 20);
            this._lblResult.TabIndex = 0;
            this._lblResult.Text = "成功：0  失败：0";
            // 
            // _lblMode
            // 
            this._lblMode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblMode.Location = new System.Drawing.Point(16, 16);
            this._lblMode.Name = "_lblMode";
            this._lblMode.Size = new System.Drawing.Size(80, 20);
            this._lblMode.TabIndex = 0;
            this._lblMode.Text = "插入模式：";
            // 
            // _lblExcelHint
            // 
            this._lblExcelHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblExcelHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblExcelHint.Location = new System.Drawing.Point(30, 220);
            this._lblExcelHint.Name = "_lblExcelHint";
            this._lblExcelHint.Size = new System.Drawing.Size(620, 40);
            this._lblExcelHint.TabIndex = 7;
            this._lblExcelHint.Text = "Excel 格式：A列=属性编号(PROPERTY_NUMBER)，B列=属性值(PROPERTY_VALUE)，（第1行为标题，数据从第2行开始）";
            // 
            // InsertPropertyValueForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(680, 560);
            this.Controls.Add(this._lblProgress);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._grpResult);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnImport);
            this.Controls.Add(this._lblExcelHint);
            this.Controls.Add(this._cmbProperty);
            this.Controls.Add(this._lblProperty);
            this.Controls.Add(this._cmbClassification);
            this.Controls.Add(this._lblClassification);
            this.Controls.Add(this._rbClassification);
            this.Controls.Add(this._rbRoot);
            this.Controls.Add(this._lblMode);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InsertPropertyValueForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "插入属性值";
            this._grpResult.ResumeLayout(false);
            this._grpResult.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        // 控件字段（从 .cs 移入 Designer.cs）
        private System.Windows.Forms.RadioButton _rbRoot;
        private System.Windows.Forms.RadioButton _rbClassification;
        private System.Windows.Forms.Label _lblProperty;
        private System.Windows.Forms.ComboBox _cmbProperty;
        private System.Windows.Forms.Label _lblClassification;
        private System.Windows.Forms.ComboBox _cmbClassification;
        private System.Windows.Forms.Button _btnImport;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Label _lblProgress;
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.GroupBox _grpResult;
        private System.Windows.Forms.Label _lblResult;
        private System.Windows.Forms.TextBox _txtErrors;
        // 局部变量提升为字段（在 .cs InitControls 中作为局部变量使用）
        private System.Windows.Forms.Label _lblMode;
        private System.Windows.Forms.Label _lblExcelHint;
    }
}
