using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class MetaPropertyPreviewForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSummary = new System.Windows.Forms.Label();
            this.lblInsertCount = new System.Windows.Forms.Label();
            this.lblUpdateCount = new System.Windows.Forms.Label();
            this.lblSkipCount = new System.Windows.Forms.Label();
            this.dgvPreview = new System.Windows.Forms.DataGridView();
            this._btnUpdateAll = new System.Windows.Forms.Button();
            this._btnIgnoreAll = new System.Windows.Forms.Button();
            this._btnImport = new System.Windows.Forms.Button();
            this._btnConfirm = new System.Windows.Forms.Button();
            this._btnAbort = new System.Windows.Forms.Button();
            this._lblImportStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(16, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(60, 19);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "导入预览";
            // 
            // lblSummary
            // 
            this.lblSummary.AutoSize = true;
            this.lblSummary.Location = new System.Drawing.Point(16, 42);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(100, 13);
            this.lblSummary.TabIndex = 1;
            this.lblSummary.Text = "共 N 行...";
            // 
            // lblInsertCount
            // 
            this.lblInsertCount.AutoSize = true;
            this.lblInsertCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblInsertCount.Location = new System.Drawing.Point(16, 62);
            this.lblInsertCount.Name = "lblInsertCount";
            this.lblInsertCount.Size = new System.Drawing.Size(60, 15);
            this.lblInsertCount.TabIndex = 2;
            this.lblInsertCount.Text = "新增：0";
            // 
            // lblUpdateCount
            // 
            this.lblUpdateCount.AutoSize = true;
            this.lblUpdateCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUpdateCount.Location = new System.Drawing.Point(120, 62);
            this.lblUpdateCount.Name = "lblUpdateCount";
            this.lblUpdateCount.Size = new System.Drawing.Size(60, 15);
            this.lblUpdateCount.TabIndex = 3;
            this.lblUpdateCount.Text = "将更新：0";
            // 
            // lblSkipCount
            // 
            this.lblSkipCount.AutoSize = true;
            this.lblSkipCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSkipCount.Location = new System.Drawing.Point(340, 62);
            this.lblSkipCount.Name = "lblSkipCount";
            this.lblSkipCount.Size = new System.Drawing.Size(60, 15);
            this.lblSkipCount.TabIndex = 4;
            this.lblSkipCount.Text = "将忽略：0";
            // 
            // dgvPreview
            // 
            this.dgvPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvPreview.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgvPreview.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.dgvPreview.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.dgvPreview.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.dgvPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPreview.Location = new System.Drawing.Point(16, 88);
            this.dgvPreview.Name = "dgvPreview";
            this.dgvPreview.ReadOnly = false;
            this.dgvPreview.RowHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.dgvPreview.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPreview.Size = new System.Drawing.Size(1340, 440);
            this.dgvPreview.TabIndex = 5;
            // 
            // _btnUpdateAll
            // 
            this._btnUpdateAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._btnUpdateAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(118)))), ((int)(((byte)(210)))));
            this._btnUpdateAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnUpdateAll.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnUpdateAll.ForeColor = System.Drawing.Color.White;
            this._btnUpdateAll.Location = new System.Drawing.Point(16, 540);
            this._btnUpdateAll.Name = "_btnUpdateAll";
            this._btnUpdateAll.Size = new System.Drawing.Size(100, 30);
            this._btnUpdateAll.TabIndex = 0;
            this._btnUpdateAll.Text = "全部更新";
            this._btnUpdateAll.UseVisualStyleBackColor = false;
            this._btnUpdateAll.Click += new System.EventHandler(this._btnUpdateAll_Click);
            // 
            // _btnIgnoreAll
            // 
            this._btnIgnoreAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._btnIgnoreAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._btnIgnoreAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnIgnoreAll.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnIgnoreAll.ForeColor = System.Drawing.Color.White;
            this._btnIgnoreAll.Location = new System.Drawing.Point(122, 540);
            this._btnIgnoreAll.Name = "_btnIgnoreAll";
            this._btnIgnoreAll.Size = new System.Drawing.Size(100, 30);
            this._btnIgnoreAll.TabIndex = 1;
            this._btnIgnoreAll.Text = "全部忽略";
            this._btnIgnoreAll.UseVisualStyleBackColor = false;
            this._btnIgnoreAll.Click += new System.EventHandler(this._btnIgnoreAll_Click);
            // 
            // _lblImportStatus
            // 
            this._lblImportStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._lblImportStatus.AutoSize = true;
            this._lblImportStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblImportStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._lblImportStatus.Location = new System.Drawing.Point(240, 548);
            this._lblImportStatus.Name = "_lblImportStatus";
            this._lblImportStatus.Size = new System.Drawing.Size(0, 15);
            this._lblImportStatus.TabIndex = 2;
            this._lblImportStatus.Visible = false;
            // 
            // _btnImport
            // 
            this._btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnImport.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnImport.ForeColor = System.Drawing.Color.White;
            this._btnImport.Location = new System.Drawing.Point(1130, 540);
            this._btnImport.Name = "_btnImport";
            this._btnImport.Size = new System.Drawing.Size(110, 30);
            this._btnImport.TabIndex = 3;
            this._btnImport.Text = "开始导入";
            this._btnImport.UseVisualStyleBackColor = false;
            this._btnImport.Click += new System.EventHandler(this._btnImport_Click);
            // 
            // _btnAbort
            // 
            this._btnAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnAbort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this._btnAbort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnAbort.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnAbort.ForeColor = System.Drawing.Color.White;
            this._btnAbort.Location = new System.Drawing.Point(1014, 540);
            this._btnAbort.Name = "_btnAbort";
            this._btnAbort.Size = new System.Drawing.Size(110, 30);
            this._btnAbort.TabIndex = 4;
            this._btnAbort.Text = "终止导入";
            this._btnAbort.UseVisualStyleBackColor = false;
            this._btnAbort.Click += new System.EventHandler(this._btnAbort_Click);
            // 
            // _btnConfirm
            // 
            this._btnConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnConfirm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this._btnConfirm.Enabled = false;
            this._btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnConfirm.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnConfirm.ForeColor = System.Drawing.Color.White;
            this._btnConfirm.Location = new System.Drawing.Point(1246, 540);
            this._btnConfirm.Name = "_btnConfirm";
            this._btnConfirm.Size = new System.Drawing.Size(100, 30);
            this._btnConfirm.TabIndex = 5;
            this._btnConfirm.Text = "确认并继续导入";
            this._btnConfirm.UseVisualStyleBackColor = false;
            this._btnConfirm.Click += new System.EventHandler(this._btnConfirm_Click);
            // 
            // MetaPropertyPreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1362, 588);
            this.Controls.Add(this._lblImportStatus);
            this.Controls.Add(this._btnUpdateAll);
            this.Controls.Add(this._btnIgnoreAll);
            this.Controls.Add(this._btnAbort);
            this.Controls.Add(this._btnConfirm);
            this.Controls.Add(this._btnImport);
            this.Controls.Add(this.dgvPreview);
            this.Controls.Add(this.lblSkipCount);
            this.Controls.Add(this.lblUpdateCount);
            this.Controls.Add(this.lblInsertCount);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "MetaPropertyPreviewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "导入预览";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MetaPropertyPreviewForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Label lblInsertCount;
        private System.Windows.Forms.Label lblUpdateCount;
        private System.Windows.Forms.Label lblSkipCount;
        private System.Windows.Forms.DataGridView dgvPreview;
        private System.Windows.Forms.Button _btnUpdateAll;
        private System.Windows.Forms.Button _btnIgnoreAll;
        private System.Windows.Forms.Button _btnImport;
        private System.Windows.Forms.Button _btnConfirm;
        private System.Windows.Forms.Button _btnAbort;
        private System.Windows.Forms.Label _lblImportStatus;
    }
}
