using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class WftScriptRemoveForm
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
            this.lblSource = new System.Windows.Forms.Label();
            this.txtSourcePath = new System.Windows.Forms.TextBox();
            this.btnSelectSource = new System.Windows.Forms.Button();
            this.lblDest = new System.Windows.Forms.Label();
            this.txtDestPath = new System.Windows.Forms.TextBox();
            this.btnSelectDest = new System.Windows.Forms.Button();
            this.btnProcess = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.listBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.lblNodeName = new System.Windows.Forms.Label();
            this.txtNodeName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(23, 18);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(105, 15);
            this.lblSource.TabIndex = 0;
            this.lblSource.Text = "📁 源文件夹：";
            // 
            // txtSourcePath
            // 
            this.txtSourcePath.Location = new System.Drawing.Point(23, 40);
            this.txtSourcePath.Name = "txtSourcePath";
            this.txtSourcePath.ReadOnly = true;
            this.txtSourcePath.Size = new System.Drawing.Size(479, 25);
            this.txtSourcePath.TabIndex = 1;
            // 
            // btnSelectSource
            // 
            this.btnSelectSource.Location = new System.Drawing.Point(520, 39);
            this.btnSelectSource.Name = "btnSelectSource";
            this.btnSelectSource.Size = new System.Drawing.Size(114, 26);
            this.btnSelectSource.TabIndex = 2;
            this.btnSelectSource.Text = "选择...";
            this.btnSelectSource.UseVisualStyleBackColor = true;
            this.btnSelectSource.Click += new System.EventHandler(this.btnSelectSource_Click);
            // 
            // lblDest
            // 
            this.lblDest.AutoSize = true;
            this.lblDest.Location = new System.Drawing.Point(23, 75);
            this.lblDest.Name = "lblDest";
            this.lblDest.Size = new System.Drawing.Size(104, 15);
            this.lblDest.TabIndex = 3;
            this.lblDest.Text = "💾 保存位置：";
            // 
            // txtDestPath
            // 
            this.txtDestPath.Location = new System.Drawing.Point(23, 97);
            this.txtDestPath.Name = "txtDestPath";
            this.txtDestPath.ReadOnly = true;
            this.txtDestPath.Size = new System.Drawing.Size(479, 25);
            this.txtDestPath.TabIndex = 4;
            // 
            // btnSelectDest
            // 
            this.btnSelectDest.Location = new System.Drawing.Point(520, 96);
            this.btnSelectDest.Name = "btnSelectDest";
            this.btnSelectDest.Size = new System.Drawing.Size(114, 26);
            this.btnSelectDest.TabIndex = 5;
            this.btnSelectDest.Text = "选择...";
            this.btnSelectDest.UseVisualStyleBackColor = true;
            this.btnSelectDest.Click += new System.EventHandler(this.btnSelectDest_Click);
            // 
            // btnProcess
            // 
            this.btnProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProcess.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnProcess.ForeColor = System.Drawing.Color.White;
            this.btnProcess.Location = new System.Drawing.Point(23, 197);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(611, 35);
            this.btnProcess.TabIndex = 8;
            this.btnProcess.Text = "🚀 开始处理";
            this.btnProcess.UseVisualStyleBackColor = false;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(23, 241);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(611, 18);
            this.progressBar.TabIndex = 9;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblStatus.Location = new System.Drawing.Point(23, 263);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(66, 20);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "等待中...";
            // 
            // listBox
            // 
            this.listBox.FormattingEnabled = true;
            this.listBox.ItemHeight = 15;
            this.listBox.Location = new System.Drawing.Point(23, 285);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(611, 124);
            this.listBox.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(10, 413);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(322, 15);
            this.label1.TabIndex = 12;
            this.label1.Text = "说明：删除工作流文件中的指定脚本名称，全名";
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
            // 
            // lblNodeName
            // 
            this.lblNodeName.AutoSize = true;
            this.lblNodeName.Location = new System.Drawing.Point(23, 135);
            this.lblNodeName.Name = "lblNodeName";
            this.lblNodeName.Size = new System.Drawing.Size(550, 15);
            this.lblNodeName.TabIndex = 6;
            this.lblNodeName.Text = "🔍 要删除的节点全名称(例如：服务端提交_签名 多个脚本请用英文分号;隔开)：";
            // 
            // txtNodeName
            // 
            this.txtNodeName.Location = new System.Drawing.Point(23, 157);
            this.txtNodeName.Name = "txtNodeName";
            this.txtNodeName.Size = new System.Drawing.Size(611, 25);
            this.txtNodeName.TabIndex = 7;
            this.txtNodeName.Text = "请输入节点Name属性值";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(11, 435);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(322, 15);
            this.label2.TabIndex = 13;
            this.label2.Text = "切记：一定备份！一定备份！！一定备份！！！";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // WftScriptRemoveForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 484);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBox);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.txtNodeName);
            this.Controls.Add(this.lblNodeName);
            this.Controls.Add(this.btnSelectDest);
            this.Controls.Add(this.txtDestPath);
            this.Controls.Add(this.lblDest);
            this.Controls.Add(this.btnSelectSource);
            this.Controls.Add(this.txtSourcePath);
            this.Controls.Add(this.lblSource);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "WftScriptRemoveForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "去除工作流模板脚本";
            this.Load += new System.EventHandler(this.WftScriptRemoveForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.TextBox txtSourcePath;
        private System.Windows.Forms.Button btnSelectSource;
        private System.Windows.Forms.Label lblDest;
        private System.Windows.Forms.TextBox txtDestPath;
        private System.Windows.Forms.Button btnSelectDest;
        private System.Windows.Forms.Label lblNodeName;
        private System.Windows.Forms.TextBox txtNodeName;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.Label label1;
        private System.ComponentModel.BackgroundWorker backgroundWorker;

        #endregion

        private System.Windows.Forms.Label label2;
    }
}