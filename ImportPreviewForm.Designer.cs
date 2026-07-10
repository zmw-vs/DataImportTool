using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class ImportPreviewForm
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
            this._lblTitle = new System.Windows.Forms.Label();
            this._pnlErrors = new System.Windows.Forms.Panel();
            this._lblErrorsTitle = new System.Windows.Forms.Label();
            this._txtErrors = new System.Windows.Forms.TextBox();
            this._btnExportErrors = new System.Windows.Forms.Button();
            this._btnImport = new System.Windows.Forms.Button();
            this._btnClose = new System.Windows.Forms.Button();
            this._btnExpandAll = new System.Windows.Forms.Button();
            this._btnCollapseAll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._splitMain)).BeginInit();
            this._splitMain.Panel1.SuspendLayout();
            this._splitMain.Panel2.SuspendLayout();
            this._splitMain.SuspendLayout();
            this._pnlErrors.SuspendLayout();
            this.SuspendLayout();
            //
            // _lblTitle
            //
            this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
            this._lblTitle.Location = new System.Drawing.Point(12, 12);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new System.Drawing.Size(320, 24);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "导入数据预览";
            //
            // _splitMain
            //
            this._splitMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._splitMain.Location = new System.Drawing.Point(12, 44);
            this._splitMain.Name = "_splitMain";
            this._splitMain.Panel1.Controls.Add(this._treeView);
            this._splitMain.Panel2.Controls.Add(this._pnlErrors);
            this._splitMain.Size = new System.Drawing.Size(760, 340);
            this._splitMain.SplitterDistance = 480;
            this._splitMain.TabIndex = 1;
            //
            // _treeView
            //
            this._treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._treeView.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._treeView.Location = new System.Drawing.Point(0, 0);
            this._treeView.Name = "_treeView";
            this._treeView.Size = new System.Drawing.Size(480, 340);
            this._treeView.TabIndex = 0;
            //
            // _pnlErrors
            //
            this._pnlErrors.BackColor = System.Drawing.Color.FromArgb(248, 249, 252);
            this._pnlErrors.Controls.Add(this._lblErrorsTitle);
            this._pnlErrors.Controls.Add(this._txtErrors);
            this._pnlErrors.Controls.Add(this._btnExportErrors);
            this._pnlErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlErrors.Location = new System.Drawing.Point(0, 0);
            this._pnlErrors.Name = "_pnlErrors";
            this._pnlErrors.Size = new System.Drawing.Size(276, 340);
            //
            // _lblErrorsTitle
            //
            this._lblErrorsTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._lblErrorsTitle.Location = new System.Drawing.Point(8, 8);
            this._lblErrorsTitle.Name = "_lblErrorsTitle";
            this._lblErrorsTitle.Size = new System.Drawing.Size(260, 22);
            this._lblErrorsTitle.TabIndex = 0;
            this._lblErrorsTitle.Text = "校验结果";
            //
            // _txtErrors
            //
            this._txtErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtErrors.BackColor = System.Drawing.Color.FromArgb(252, 252, 245);
            this._txtErrors.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._txtErrors.Location = new System.Drawing.Point(8, 36);
            this._txtErrors.Multiline = true;
            this._txtErrors.Name = "_txtErrors";
            this._txtErrors.ReadOnly = true;
            this._txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtErrors.Size = new System.Drawing.Size(260, 260);
            this._txtErrors.TabIndex = 1;
            this._txtErrors.WordWrap = true;
            //
            // _btnExportErrors
            //
            this._btnExportErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnExportErrors.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._btnExportErrors.Location = new System.Drawing.Point(168, 304);
            this._btnExportErrors.Name = "_btnExportErrors";
            this._btnExportErrors.Size = new System.Drawing.Size(100, 28);
            this._btnExportErrors.TabIndex = 2;
            this._btnExportErrors.Text = "导出错误 Excel";
            this._btnExportErrors.UseVisualStyleBackColor = true;
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
            //
            // _btnImport
            //
            this._btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnImport.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this._btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnImport.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnImport.ForeColor = System.Drawing.Color.White;
            this._btnImport.Location = new System.Drawing.Point(580, 392);
            this._btnImport.Name = "_btnImport";
            this._btnImport.Size = new System.Drawing.Size(96, 28);
            this._btnImport.TabIndex = 5;
            this._btnImport.Text = "确认导入";
            this._btnImport.UseVisualStyleBackColor = false;
            //
            // _btnClose
            //
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnClose.Location = new System.Drawing.Point(684, 392);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(80, 28);
            this._btnClose.TabIndex = 6;
            this._btnClose.Text = "关闭";
            this._btnClose.UseVisualStyleBackColor = true;
            //
            // ImportPreviewForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.ClientSize = new System.Drawing.Size(784, 430);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._btnImport);
            this.Controls.Add(this._btnCollapseAll);
            this.Controls.Add(this._btnExpandAll);
            this.Controls.Add(this._splitMain);
            this.Controls.Add(this._lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 350);
            this.Name = "ImportPreviewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "导入数据预览";
            this._splitMain.Panel1.ResumeLayout(false);
            this._splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitMain)).EndInit();
            this._splitMain.ResumeLayout(false);
            this._pnlErrors.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.SplitContainer _splitMain;
        private System.Windows.Forms.TreeView _treeView;
        private System.Windows.Forms.Label _lblTitle;
        private System.Windows.Forms.Panel _pnlErrors;
        private System.Windows.Forms.Label _lblErrorsTitle;
        private System.Windows.Forms.TextBox _txtErrors;
        private System.Windows.Forms.Button _btnExportErrors;
        private System.Windows.Forms.Button _btnImport;
        private System.Windows.Forms.Button _btnClose;
        private System.Windows.Forms.Button _btnExpandAll;
        private System.Windows.Forms.Button _btnCollapseAll;
    }
}
