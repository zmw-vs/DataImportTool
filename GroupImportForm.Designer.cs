using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class GroupImportForm
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
            this._txtFilePath = new System.Windows.Forms.TextBox();
            this._btnSelectFile = new System.Windows.Forms.Button();
            this._lblResult = new System.Windows.Forms.Label();
            this._txtErrors = new System.Windows.Forms.TextBox();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._lblProgress = new System.Windows.Forms.Label();
            this._grpFile = new System.Windows.Forms.Panel();
            this._lblGrpFileTitle = new System.Windows.Forms.Label();
            this._btnTemplate = new System.Windows.Forms.Button();
            this._grpFile.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblTitle
            // 
            this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblTitle.Location = new System.Drawing.Point(24, 16);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new System.Drawing.Size(532, 36);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "分类组导入";
            // 
            // _lblHint
            // 
            this._lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblHint.Location = new System.Drawing.Point(24, 61);
            this._lblHint.Name = "_lblHint";
            this._lblHint.Size = new System.Drawing.Size(532, 32);
            this._lblHint.TabIndex = 1;
            // 
            // _txtFilePath
            // 
            this._txtFilePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._txtFilePath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._txtFilePath.Location = new System.Drawing.Point(12, 47);
            this._txtFilePath.Name = "_txtFilePath";
            this._txtFilePath.ReadOnly = true;
            this._txtFilePath.Size = new System.Drawing.Size(310, 27);
            this._txtFilePath.TabIndex = 1;
            // 
            // _btnSelectFile
            // 
            this._btnSelectFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnSelectFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSelectFile.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnSelectFile.ForeColor = System.Drawing.Color.White;
            this._btnSelectFile.Location = new System.Drawing.Point(328, 46);
            this._btnSelectFile.Name = "_btnSelectFile";
            this._btnSelectFile.Size = new System.Drawing.Size(115, 28);
            this._btnSelectFile.TabIndex = 2;
            this._btnSelectFile.Text = "选择文件...";
            this._btnSelectFile.UseVisualStyleBackColor = false;
            this._btnSelectFile.Click += new System.EventHandler(this.BtnSelectFile_Click);
            // 
            // _lblResult
            // 
            this._lblResult.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblResult.Location = new System.Drawing.Point(24, 200);
            this._lblResult.Name = "_lblResult";
            this._lblResult.Size = new System.Drawing.Size(532, 22);
            this._lblResult.TabIndex = 4;
            // 
            // _txtErrors
            // 
            this._txtErrors.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._txtErrors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtErrors.Font = new System.Drawing.Font("Consolas", 8F);
            this._txtErrors.Location = new System.Drawing.Point(24, 226);
            this._txtErrors.Multiline = true;
            this._txtErrors.Name = "_txtErrors";
            this._txtErrors.ReadOnly = true;
            this._txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtErrors.Size = new System.Drawing.Size(596, 116);
            this._txtErrors.TabIndex = 5;
            // 
            // _progressBar
            // 
            this._progressBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._progressBar.Location = new System.Drawing.Point(24, 376);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(596, 22);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 6;
            // 
            // _lblProgress
            // 
            this._lblProgress.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblProgress.Location = new System.Drawing.Point(24, 352);
            this._lblProgress.Name = "_lblProgress";
            this._lblProgress.Size = new System.Drawing.Size(532, 20);
            this._lblProgress.TabIndex = 7;
            // 
            // _grpFile
            // 
            this._grpFile.BackColor = System.Drawing.Color.White;
            this._grpFile.Controls.Add(this._lblGrpFileTitle);
            this._grpFile.Controls.Add(this._txtFilePath);
            this._grpFile.Controls.Add(this._btnSelectFile);
            this._grpFile.Controls.Add(this._btnTemplate);
            this._grpFile.Location = new System.Drawing.Point(24, 88);
            this._grpFile.Name = "_grpFile";
            this._grpFile.Size = new System.Drawing.Size(596, 84);
            this._grpFile.TabIndex = 2;
            // 
            // _lblGrpFileTitle
            // 
            this._lblGrpFileTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblGrpFileTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblGrpFileTitle.Location = new System.Drawing.Point(8, 2);
            this._lblGrpFileTitle.Name = "_lblGrpFileTitle";
            this._lblGrpFileTitle.Size = new System.Drawing.Size(532, 20);
            this._lblGrpFileTitle.TabIndex = 0;
            this._lblGrpFileTitle.Text = "步骤 1：选择 Excel 文件";
            // 
            // _btnTemplate
            // 
            this._btnTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnTemplate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnTemplate.Location = new System.Drawing.Point(449, 46);
            this._btnTemplate.Name = "_btnTemplate";
            this._btnTemplate.Size = new System.Drawing.Size(125, 28);
            this._btnTemplate.TabIndex = 11;
            this._btnTemplate.Text = "生成导入模板";
            this._btnTemplate.UseVisualStyleBackColor = true;
            this._btnTemplate.Click += new System.EventHandler(this.BtnTemplate_Click);
            // 
            // GroupImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(654, 460);
            this.Controls.Add(this._lblProgress);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._txtErrors);
            this.Controls.Add(this._lblResult);
            this.Controls.Add(this._grpFile);
            this.Controls.Add(this._lblHint);
            this.Controls.Add(this._lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GroupImportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "分类组导入";
            this._grpFile.ResumeLayout(false);
            this._grpFile.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        // 控件字段（从 .cs 移入 Designer.cs）
        private System.Windows.Forms.Label _lblTitle;
        private System.Windows.Forms.Label _lblHint;
        private System.Windows.Forms.TextBox _txtFilePath;
        private System.Windows.Forms.Button _btnSelectFile;
        private System.Windows.Forms.Label _lblResult;
        private System.Windows.Forms.TextBox _txtErrors;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Label _lblProgress;
        private System.Windows.Forms.Button _btnTemplate;
        // 内部面板（替代 MakeFlatPanel）
        private System.Windows.Forms.Panel _grpFile;
        private System.Windows.Forms.Label _lblGrpFileTitle;
    }
}
