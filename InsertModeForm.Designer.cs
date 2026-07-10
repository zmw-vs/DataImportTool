using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class InsertModeForm
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
            this._lblTitle = new System.Windows.Forms.Label();
            this._lblHint = new System.Windows.Forms.Label();
            this._rbGroup = new System.Windows.Forms.RadioButton();
            this._rbRoot = new System.Windows.Forms.RadioButton();
            this._lblGroupHint = new System.Windows.Forms.Label();
            this._cmbGroup = new System.Windows.Forms.ComboBox();
            this._btnSelectFile = new System.Windows.Forms.Button();
            this._lblResult = new System.Windows.Forms.Label();
            this._txtErrors = new System.Windows.Forms.TextBox();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._lblProgress = new System.Windows.Forms.Label();
            this._grpMode = new System.Windows.Forms.Panel();
            this._lblModeTitle = new System.Windows.Forms.Label();
            this._grpOp = new System.Windows.Forms.Panel();
            this._lblOpTitle = new System.Windows.Forms.Label();
            this._grpForm = new System.Windows.Forms.Panel();
            this._lblFormTitle = new System.Windows.Forms.Label();
            this._rbNoForm = new System.Windows.Forms.RadioButton();
            this._rbGenForm = new System.Windows.Forms.RadioButton();
            this._lblTemplate = new System.Windows.Forms.Label();
            this._cmbTemplate = new System.Windows.Forms.ComboBox();
            this._grpMode.SuspendLayout();
            this._grpOp.SuspendLayout();
            this._grpForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblTitle
            // 
            this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblTitle.Location = new System.Drawing.Point(24, 20);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new System.Drawing.Size(532, 32);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "分类插入";
            // 
            // _lblHint
            // 
            this._lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblHint.Location = new System.Drawing.Point(24, 56);
            this._lblHint.Name = "_lblHint";
            this._lblHint.Size = new System.Drawing.Size(532, 20);
            this._lblHint.TabIndex = 1;
            // 
            // _rbGroup
            // 
            this._rbGroup.Checked = true;
            this._rbGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._rbGroup.Location = new System.Drawing.Point(14, 28);
            this._rbGroup.Name = "_rbGroup";
            this._rbGroup.Size = new System.Drawing.Size(220, 20);
            this._rbGroup.TabIndex = 1;
            this._rbGroup.TabStop = true;
            this._rbGroup.Text = "插入到指定分类组";
            this._rbGroup.CheckedChanged += new System.EventHandler(this.OnModeChanged);
            // 
            // _rbRoot
            // 
            this._rbRoot.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._rbRoot.Location = new System.Drawing.Point(14, 52);
            this._rbRoot.Name = "_rbRoot";
            this._rbRoot.Size = new System.Drawing.Size(180, 20);
            this._rbRoot.TabIndex = 2;
            this._rbRoot.Text = "插入为根节点";
            // 
            // _lblGroupHint
            // 
            this._lblGroupHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblGroupHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblGroupHint.Location = new System.Drawing.Point(30, 74);
            this._lblGroupHint.Name = "_lblGroupHint";
            this._lblGroupHint.Size = new System.Drawing.Size(140, 16);
            this._lblGroupHint.TabIndex = 3;
            this._lblGroupHint.Text = "选择目标分类组：";
            // 
            // _cmbGroup
            // 
            this._cmbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbGroup.FormattingEnabled = true;
            this._cmbGroup.Location = new System.Drawing.Point(170, 71);
            this._cmbGroup.Name = "_cmbGroup";
            this._cmbGroup.Size = new System.Drawing.Size(346, 28);
            this._cmbGroup.TabIndex = 4;
            // 
            // _btnSelectFile
            // 
            this._btnSelectFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnSelectFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSelectFile.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._btnSelectFile.ForeColor = System.Drawing.Color.White;
            this._btnSelectFile.Location = new System.Drawing.Point(14, 24);
            this._btnSelectFile.Name = "_btnSelectFile";
            this._btnSelectFile.Size = new System.Drawing.Size(200, 32);
            this._btnSelectFile.TabIndex = 1;
            this._btnSelectFile.Text = "📂 选择 Excel 文件...";
            this._btnSelectFile.UseVisualStyleBackColor = false;
            this._btnSelectFile.Click += new System.EventHandler(this.BtnSelectFile_Click);
            // 
            // _lblResult
            // 
            this._lblResult.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblResult.Location = new System.Drawing.Point(24, 420);
            this._lblResult.Name = "_lblResult";
            this._lblResult.Size = new System.Drawing.Size(532, 22);
            this._lblResult.TabIndex = 4;
            // 
            // _txtErrors
            // 
            this._txtErrors.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._txtErrors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtErrors.Font = new System.Drawing.Font("Consolas", 8F);
            this._txtErrors.Location = new System.Drawing.Point(24, 446);
            this._txtErrors.Multiline = true;
            this._txtErrors.Name = "_txtErrors";
            this._txtErrors.ReadOnly = true;
            this._txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtErrors.Size = new System.Drawing.Size(532, 68);
            this._txtErrors.TabIndex = 5;
            // 
            // _progressBar
            // 
            this._progressBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._progressBar.Location = new System.Drawing.Point(24, 546);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(532, 22);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 6;
            // 
            // _lblProgress
            // 
            this._lblProgress.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblProgress.Location = new System.Drawing.Point(24, 522);
            this._lblProgress.Name = "_lblProgress";
            this._lblProgress.Size = new System.Drawing.Size(532, 20);
            this._lblProgress.TabIndex = 7;
            // 
            // _grpMode
            // 
            this._grpMode.BackColor = System.Drawing.Color.White;
            this._grpMode.Controls.Add(this._lblModeTitle);
            this._grpMode.Controls.Add(this._rbGroup);
            this._grpMode.Controls.Add(this._rbRoot);
            this._grpMode.Controls.Add(this._lblGroupHint);
            this._grpMode.Controls.Add(this._cmbGroup);
            this._grpMode.Location = new System.Drawing.Point(24, 84);
            this._grpMode.Name = "_grpMode";
            this._grpMode.Size = new System.Drawing.Size(532, 100);
            this._grpMode.TabIndex = 2;
            // 
            // _lblModeTitle
            // 
            this._lblModeTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblModeTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblModeTitle.Location = new System.Drawing.Point(0, 0);
            this._lblModeTitle.Name = "_lblModeTitle";
            this._lblModeTitle.Size = new System.Drawing.Size(532, 20);
            this._lblModeTitle.TabIndex = 0;
            this._lblModeTitle.Text = "步骤 1：选择插入位置";
            // 
            // _grpOp
            // 
            this._grpOp.BackColor = System.Drawing.Color.White;
            this._grpOp.Controls.Add(this._lblOpTitle);
            this._grpOp.Controls.Add(this._btnSelectFile);
            this._grpOp.Controls.Add(this._lblHint);
            this._grpOp.Location = new System.Drawing.Point(24, 314);
            this._grpOp.Name = "_grpOp";
            this._grpOp.Size = new System.Drawing.Size(532, 100);
            this._grpOp.TabIndex = 3;
            // 
            // _lblOpTitle
            // 
            this._lblOpTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblOpTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblOpTitle.Location = new System.Drawing.Point(0, 0);
            this._lblOpTitle.Name = "_lblOpTitle";
            this._lblOpTitle.Size = new System.Drawing.Size(532, 20);
            this._lblOpTitle.TabIndex = 0;
            this._lblOpTitle.Text = "步骤 3：选择文件并导入";
            // 
            // _grpForm
            // 
            this._grpForm.BackColor = System.Drawing.Color.White;
            this._grpForm.Controls.Add(this._lblFormTitle);
            this._grpForm.Controls.Add(this._rbNoForm);
            this._grpForm.Controls.Add(this._rbGenForm);
            this._grpForm.Controls.Add(this._lblTemplate);
            this._grpForm.Controls.Add(this._cmbTemplate);
            this._grpForm.Location = new System.Drawing.Point(24, 194);
            this._grpForm.Name = "_grpForm";
            this._grpForm.Size = new System.Drawing.Size(532, 110);
            this._grpForm.TabIndex = 5;
            // 
            // _lblFormTitle
            // 
            this._lblFormTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblFormTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblFormTitle.Location = new System.Drawing.Point(0, 0);
            this._lblFormTitle.Name = "_lblFormTitle";
            this._lblFormTitle.Size = new System.Drawing.Size(532, 20);
            this._lblFormTitle.TabIndex = 0;
            this._lblFormTitle.Text = "步骤 2：分类表单生成配置";
            // 
            // _rbNoForm
            // 
            this._rbNoForm.Checked = true;
            this._rbNoForm.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._rbNoForm.Location = new System.Drawing.Point(14, 28);
            this._rbNoForm.Name = "_rbNoForm";
            this._rbNoForm.Size = new System.Drawing.Size(240, 20);
            this._rbNoForm.TabIndex = 1;
            this._rbNoForm.TabStop = true;
            this._rbNoForm.Text = "不生成分类表单";
            this._rbNoForm.CheckedChanged += new System.EventHandler(this.OnFormGenModeChanged);
            // 
            // _rbGenForm
            // 
            this._rbGenForm.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._rbGenForm.Location = new System.Drawing.Point(14, 52);
            this._rbGenForm.Name = "_rbGenForm";
            this._rbGenForm.Size = new System.Drawing.Size(240, 20);
            this._rbGenForm.TabIndex = 2;
            this._rbGenForm.Text = "同时生成分类表单";
            this._rbGenForm.CheckedChanged += new System.EventHandler(this.OnFormGenModeChanged);
            // 
            // _lblTemplate
            // 
            this._lblTemplate.Enabled = false;
            this._lblTemplate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblTemplate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblTemplate.Location = new System.Drawing.Point(30, 76);
            this._lblTemplate.Name = "_lblTemplate";
            this._lblTemplate.Size = new System.Drawing.Size(120, 16);
            this._lblTemplate.TabIndex = 3;
            this._lblTemplate.Text = "选择表单模板：";
            // 
            // _cmbTemplate
            // 
            this._cmbTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbTemplate.Enabled = false;
            this._cmbTemplate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbTemplate.FormattingEnabled = true;
            this._cmbTemplate.Location = new System.Drawing.Point(150, 73);
            this._cmbTemplate.Name = "_cmbTemplate";
            this._cmbTemplate.Size = new System.Drawing.Size(366, 28);
            this._cmbTemplate.TabIndex = 4;
            this._cmbTemplate.SelectedIndexChanged += new System.EventHandler(this.OnTemplateSelected);
            // 
            // InsertModeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(580, 669);
            this.Controls.Add(this._lblProgress);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._txtErrors);
            this.Controls.Add(this._lblResult);
            this.Controls.Add(this._grpOp);
            this.Controls.Add(this._grpForm);
            this.Controls.Add(this._grpMode);
            this.Controls.Add(this._lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InsertModeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "分类插入";
            this._grpMode.ResumeLayout(false);
            this._grpOp.ResumeLayout(false);
            this._grpForm.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        // 控件字段（从 .cs 移入 Designer.cs）
        private System.Windows.Forms.Label _lblTitle;
        private System.Windows.Forms.Label _lblHint;
        private System.Windows.Forms.RadioButton _rbGroup;
        private System.Windows.Forms.RadioButton _rbRoot;
        private System.Windows.Forms.Label _lblGroupHint;
        private System.Windows.Forms.ComboBox _cmbGroup;
        private System.Windows.Forms.Button _btnSelectFile;
        private System.Windows.Forms.Label _lblResult;
        private System.Windows.Forms.TextBox _txtErrors;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Label _lblProgress;
        // 内部面板（替代 MakeFlatPanel）
        private System.Windows.Forms.Panel _grpMode;
        private System.Windows.Forms.Panel _grpOp;
        private System.Windows.Forms.Label _lblModeTitle;
        private System.Windows.Forms.Label _lblOpTitle;
        // 表单生成配置面板（PRD v2.2）
        private System.Windows.Forms.Panel _grpForm;
        private System.Windows.Forms.Label _lblFormTitle;
        private System.Windows.Forms.RadioButton _rbNoForm;
        private System.Windows.Forms.RadioButton _rbGenForm;
        private System.Windows.Forms.Label _lblTemplate;
        private System.Windows.Forms.ComboBox _cmbTemplate;
    }
}
