using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class SuperBatchPreviewForm
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
            this.components = new System.ComponentModel.Container();
            this._splitMain = new System.Windows.Forms.SplitContainer();
            this._treeView = new System.Windows.Forms.TreeView();
            this._ctxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._ctxExpand = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxCollapse = new System.Windows.Forms.ToolStripMenuItem();
            this._pnlValidation = new System.Windows.Forms.Panel();
            this._lblValidationTitle = new System.Windows.Forms.Label();
            this._lstErrors = new System.Windows.Forms.TextBox();
            this._btnExportErrors = new System.Windows.Forms.Button();
            this._lblTitle = new System.Windows.Forms.Label();
            this._btnClose = new System.Windows.Forms.Button();
            this._btnExpandAll = new System.Windows.Forms.Button();
            this._btnCollapseAll = new System.Windows.Forms.Button();
            this._btnImport = new System.Windows.Forms.Button();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._txtLog = new System.Windows.Forms.TextBox();
            this._lblResult = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._splitMain)).BeginInit();
            this._splitMain.Panel1.SuspendLayout();
            this._splitMain.Panel2.SuspendLayout();
            this._splitMain.SuspendLayout();
            this._ctxMenu.SuspendLayout();
            this._pnlValidation.SuspendLayout();
            this.SuspendLayout();
            // 
            // _splitMain
            // 
            this._splitMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._splitMain.Location = new System.Drawing.Point(12, 44);
            this._splitMain.Name = "_splitMain";
            // 
            // _splitMain.Panel1
            // 
            this._splitMain.Panel1.Controls.Add(this._treeView);
            // 
            // _splitMain.Panel2
            // 
            this._splitMain.Panel2.Controls.Add(this._pnlValidation);
            this._splitMain.Size = new System.Drawing.Size(960, 340);
            this._splitMain.SplitterDistance = 600;
            this._splitMain.TabIndex = 1;
            // 
            // _treeView
            // 
            this._treeView.ContextMenuStrip = this._ctxMenu;
            this._treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._treeView.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._treeView.Location = new System.Drawing.Point(0, 0);
            this._treeView.Name = "_treeView";
            this._treeView.Size = new System.Drawing.Size(600, 340);
            this._treeView.TabIndex = 0;
            // 
            // _ctxMenu
            // 
            this._ctxMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this._ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._ctxExpand,
            this._ctxCollapse});
            this._ctxMenu.Name = "_ctxMenu";
            this._ctxMenu.Size = new System.Drawing.Size(109, 52);
            // 
            // _ctxExpand
            // 
            this._ctxExpand.Name = "_ctxExpand";
            this._ctxExpand.Size = new System.Drawing.Size(108, 24);
            this._ctxExpand.Text = "展开";
            this._ctxExpand.Click += new System.EventHandler(this.CtxExpand_Click);
            // 
            // _ctxCollapse
            // 
            this._ctxCollapse.Name = "_ctxCollapse";
            this._ctxCollapse.Size = new System.Drawing.Size(108, 24);
            this._ctxCollapse.Text = "折叠";
            this._ctxCollapse.Click += new System.EventHandler(this.CtxCollapse_Click);
            // 
            // _pnlValidation
            // 
            this._pnlValidation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(252)))));
            this._pnlValidation.Controls.Add(this._lblValidationTitle);
            this._pnlValidation.Controls.Add(this._lstErrors);
            this._pnlValidation.Controls.Add(this._btnExportErrors);
            this._pnlValidation.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlValidation.Location = new System.Drawing.Point(0, 0);
            this._pnlValidation.Name = "_pnlValidation";
            this._pnlValidation.Size = new System.Drawing.Size(356, 340);
            this._pnlValidation.TabIndex = 0;
            // 
            // _lblValidationTitle
            // 
            this._lblValidationTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblValidationTitle.Location = new System.Drawing.Point(8, 8);
            this._lblValidationTitle.Name = "_lblValidationTitle";
            this._lblValidationTitle.Size = new System.Drawing.Size(340, 22);
            this._lblValidationTitle.TabIndex = 0;
            this._lblValidationTitle.Text = "校验结果";
            // 
            // _lstErrors
            // 
            this._lstErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._lstErrors.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(245)))));
            this._lstErrors.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lstErrors.Location = new System.Drawing.Point(8, 36);
            this._lstErrors.Multiline = true;
            this._lstErrors.Name = "_lstErrors";
            this._lstErrors.ReadOnly = true;
            this._lstErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._lstErrors.Size = new System.Drawing.Size(340, 260);
            this._lstErrors.TabIndex = 1;
            // 
            // _btnExportErrors
            // 
            this._btnExportErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnExportErrors.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._btnExportErrors.Location = new System.Drawing.Point(248, 304);
            this._btnExportErrors.Name = "_btnExportErrors";
            this._btnExportErrors.Size = new System.Drawing.Size(100, 28);
            this._btnExportErrors.TabIndex = 2;
            this._btnExportErrors.Text = "导出错误 Excel";
            this._btnExportErrors.UseVisualStyleBackColor = true;
            this._btnExportErrors.Click += new System.EventHandler(this.BtnExportErrors_Click);
            // 
            // _lblTitle
            // 
            this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblTitle.Location = new System.Drawing.Point(12, 12);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new System.Drawing.Size(320, 24);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "导入数据预览";
            // 
            // _btnClose
            // 
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnClose.Location = new System.Drawing.Point(892, 392);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(80, 28);
            this._btnClose.TabIndex = 2;
            this._btnClose.Text = "关闭";
            this._btnClose.UseVisualStyleBackColor = true;
            this._btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // _btnExpandAll
            // 
            this._btnExpandAll.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._btnExpandAll.Location = new System.Drawing.Point(12, 392);
            this._btnExpandAll.Name = "_btnExpandAll";
            this._btnExpandAll.Size = new System.Drawing.Size(80, 26);
            this._btnExpandAll.TabIndex = 3;
            this._btnExpandAll.Text = "全部展开";
            this._btnExpandAll.UseVisualStyleBackColor = true;
            this._btnExpandAll.Click += new System.EventHandler(this.BtnExpandAll_Click);
            // 
            // _btnCollapseAll
            // 
            this._btnCollapseAll.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._btnCollapseAll.Location = new System.Drawing.Point(98, 392);
            this._btnCollapseAll.Name = "_btnCollapseAll";
            this._btnCollapseAll.Size = new System.Drawing.Size(80, 26);
            this._btnCollapseAll.TabIndex = 4;
            this._btnCollapseAll.Text = "全部折叠";
            this._btnCollapseAll.UseVisualStyleBackColor = true;
            this._btnCollapseAll.Click += new System.EventHandler(this.BtnCollapseAll_Click);
            // 
            // _btnImport
            // 
            this._btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnImport.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnImport.ForeColor = System.Drawing.Color.White;
            this._btnImport.Location = new System.Drawing.Point(790, 392);
            this._btnImport.Name = "_btnImport";
            this._btnImport.Size = new System.Drawing.Size(96, 28);
            this._btnImport.TabIndex = 5;
            this._btnImport.Text = "开始导入";
            this._btnImport.UseVisualStyleBackColor = false;
            this._btnImport.Click += new System.EventHandler(this._btnImport_Click);
            // 
            // _progressBar
            // 
            this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._progressBar.Location = new System.Drawing.Point(12, 428);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(960, 16);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 6;
            // 
            // _txtLog
            // 
            this._txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this._txtLog.Font = new System.Drawing.Font("Consolas", 8.25F);
            this._txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this._txtLog.Location = new System.Drawing.Point(12, 450);
            this._txtLog.Multiline = true;
            this._txtLog.Name = "_txtLog";
            this._txtLog.ReadOnly = true;
            this._txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtLog.Size = new System.Drawing.Size(960, 96);
            this._txtLog.TabIndex = 7;
            // 
            // _lblResult
            // 
            this._lblResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._lblResult.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._lblResult.Location = new System.Drawing.Point(12, 550);
            this._lblResult.Name = "_lblResult";
            this._lblResult.Size = new System.Drawing.Size(960, 18);
            this._lblResult.TabIndex = 8;
            // 
            // SuperBatchPreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(984, 578);
            this.Controls.Add(this._lblResult);
            this.Controls.Add(this._txtLog);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._btnImport);
            this.Controls.Add(this._btnCollapseAll);
            this.Controls.Add(this._btnExpandAll);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._splitMain);
            this.Controls.Add(this._lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "SuperBatchPreviewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "导入数据预览";
            this._splitMain.Panel1.ResumeLayout(false);
            this._splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitMain)).EndInit();
            this._splitMain.ResumeLayout(false);
            this._ctxMenu.ResumeLayout(false);
            this._pnlValidation.ResumeLayout(false);
            this._pnlValidation.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.SplitContainer _splitMain;
        private System.Windows.Forms.TreeView _treeView;
        private System.Windows.Forms.ContextMenuStrip _ctxMenu;
        private System.Windows.Forms.ToolStripMenuItem _ctxExpand;
        private System.Windows.Forms.ToolStripMenuItem _ctxCollapse;
        private System.Windows.Forms.Label _lblTitle;
        private System.Windows.Forms.Button _btnClose;
        private System.Windows.Forms.Button _btnExpandAll;
        private System.Windows.Forms.Button _btnCollapseAll;
        private System.Windows.Forms.Button _btnImport;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.TextBox _txtLog;
        private System.Windows.Forms.Label _lblResult;
        private System.Windows.Forms.Panel _pnlValidation;
        private System.Windows.Forms.Label _lblValidationTitle;
        private System.Windows.Forms.TextBox _lstErrors;
        private System.Windows.Forms.Button _btnExportErrors;
    }
}
