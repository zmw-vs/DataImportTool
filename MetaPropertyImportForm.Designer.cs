using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class MetaPropertyImportForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblPropertySet = new System.Windows.Forms.Label();
            this.cmbPropertySet = new System.Windows.Forms.ComboBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.btnGenerateTemplate = new System.Windows.Forms.Button();
            this.lblLog = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblPropertySet
            // 
            this.lblPropertySet.AutoSize = true;
            this.lblPropertySet.Location = new System.Drawing.Point(15, 23);
            this.lblPropertySet.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPropertySet.Name = "lblPropertySet";
            this.lblPropertySet.Size = new System.Drawing.Size(97, 15);
            this.lblPropertySet.TabIndex = 0;
            this.lblPropertySet.Text = "目标属性集：";
            // 
            // cmbPropertySet
            // 
            this.cmbPropertySet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPropertySet.FormattingEnabled = true;
            this.cmbPropertySet.Location = new System.Drawing.Point(109, 20);
            this.cmbPropertySet.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cmbPropertySet.Name = "cmbPropertySet";
            this.cmbPropertySet.Size = new System.Drawing.Size(465, 23);
            this.cmbPropertySet.TabIndex = 1;
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(15, 63);
            this.lblFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(62, 15);
            this.lblFile.TabIndex = 2;
            this.lblFile.Text = "Excel：";
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(109, 60);
            this.txtFilePath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(465, 25);
            this.txtFilePath.TabIndex = 3;
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(584, 53);
            this.btnSelectFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(107, 32);
            this.btnSelectFile.TabIndex = 4;
            this.btnSelectFile.Text = "选择文件";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // btnGenerateTemplate
            // 
            this.btnGenerateTemplate.Location = new System.Drawing.Point(584, 20);
            this.btnGenerateTemplate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnGenerateTemplate.Name = "btnGenerateTemplate";
            this.btnGenerateTemplate.Size = new System.Drawing.Size(107, 27);
            this.btnGenerateTemplate.TabIndex = 5;
            this.btnGenerateTemplate.Text = "生成导入模板";
            this.btnGenerateTemplate.UseVisualStyleBackColor = true;
            this.btnGenerateTemplate.Click += new System.EventHandler(this.btnGenerateTemplate_Click);
            // 
            // lblLog
            // 
            this.lblLog.AutoSize = true;
            this.lblLog.Location = new System.Drawing.Point(27, 104);
            this.lblLog.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLog.Name = "lblLog";
            this.lblLog.Size = new System.Drawing.Size(82, 15);
            this.lblLog.TabIndex = 7;
            this.lblLog.Text = "导入日志：";
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.BackColor = System.Drawing.SystemColors.Window;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(27, 127);
            this.txtLog.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(938, 392);
            this.txtLog.TabIndex = 8;
            // 
            // btnPreview
            // 
            this.btnPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnPreview.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPreview.ForeColor = System.Drawing.Color.White;
            this.btnPreview.Location = new System.Drawing.Point(733, 83);
            this.btnPreview.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(128, 42);
            this.btnPreview.TabIndex = 7;
            this.btnPreview.Text = "开始导入";
            this.btnPreview.UseVisualStyleBackColor = false;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // MetaPropertyImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(993, 542);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblLog);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnGenerateTemplate);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.cmbPropertySet);
            this.Controls.Add(this.lblPropertySet);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(661, 397);
            this.Name = "MetaPropertyImportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "元模型属性导入";
            this.Load += new System.EventHandler(this.MetaPropertyImportForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPropertySet;
        private System.Windows.Forms.ComboBox cmbPropertySet;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Button btnGenerateTemplate;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Label lblLog;
        private System.Windows.Forms.TextBox txtLog;
    }
}
