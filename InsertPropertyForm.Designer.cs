using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class InsertPropertyForm
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
            this.lblMode = new System.Windows.Forms.Label();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnSelectNone = new System.Windows.Forms.Button();
            this._rbClassification = new System.Windows.Forms.RadioButton();
            this._rbRoot = new System.Windows.Forms.RadioButton();
            this._lblSelectCls = new System.Windows.Forms.Label();
            this._cmbClassification = new System.Windows.Forms.ComboBox();
            this._btnLoad = new System.Windows.Forms.Button();
            this._lblSelectProp = new System.Windows.Forms.Label();
            this._txtSearch = new System.Windows.Forms.TextBox();
            this._lblSearchCount = new System.Windows.Forms.Label();
            this._clbProperties = new System.Windows.Forms.CheckedListBox();
            this._pnlSelected = new System.Windows.Forms.Panel();
            this._lblSelectedHeader = new System.Windows.Forms.Label();
            this._lblSelectedCount = new System.Windows.Forms.Label();
            this._pnlSelectedList = new System.Windows.Forms.Panel();
            this._btnClearAll = new System.Windows.Forms.Button();
            this._btnImport = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._grpResult = new System.Windows.Forms.GroupBox();
            this._lblResult = new System.Windows.Forms.Label();
            this._txtErrors = new System.Windows.Forms.TextBox();
            this._lblStatus = new System.Windows.Forms.Label();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._lblProgress = new System.Windows.Forms.Label();
            this._pnlExisting = new System.Windows.Forms.Panel();
            this._lblExistingHeader = new System.Windows.Forms.Label();
            this._lblExistingCount = new System.Windows.Forms.Label();
            this._pnlExistingList = new System.Windows.Forms.Panel();
            this._pnlSelected.SuspendLayout();
            this._pnlExisting.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMode.Location = new System.Drawing.Point(21, 18);
            this.lblMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(89, 20);
            this.lblMode.TabIndex = 0;
            this.lblMode.Text = "插入模式：";
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectAll.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnSelectAll.Location = new System.Drawing.Point(21, 455);
            this.btnSelectAll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(92, 30);
            this.btnSelectAll.TabIndex = 10;
            this.btnSelectAll.Text = "☑ 全选";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            // 
            // btnSelectNone
            // 
            this.btnSelectNone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectNone.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnSelectNone.Location = new System.Drawing.Point(124, 455);
            this.btnSelectNone.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSelectNone.Name = "btnSelectNone";
            this.btnSelectNone.Size = new System.Drawing.Size(92, 30);
            this.btnSelectNone.TabIndex = 11;
            this.btnSelectNone.Text = "☐ 全不选";
            this.btnSelectNone.UseVisualStyleBackColor = true;
            // 
            // _rbClassification
            // 
            this._rbClassification.AutoSize = true;
            this._rbClassification.Checked = true;
            this._rbClassification.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._rbClassification.Location = new System.Drawing.Point(21, 48);
            this._rbClassification.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._rbClassification.Name = "_rbClassification";
            this._rbClassification.Size = new System.Drawing.Size(190, 24);
            this._rbClassification.TabIndex = 1;
            this._rbClassification.TabStop = true;
            this._rbClassification.Text = "在指定分类下插入属性";
            // 
            // _rbRoot
            // 
            this._rbRoot.AutoSize = true;
            this._rbRoot.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._rbRoot.Location = new System.Drawing.Point(21, 78);
            this._rbRoot.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._rbRoot.Name = "_rbRoot";
            this._rbRoot.Size = new System.Drawing.Size(158, 24);
            this._rbRoot.TabIndex = 2;
            this._rbRoot.Text = "在根目录插入属性";
            // 
            // _lblSelectCls
            // 
            this._lblSelectCls.AutoSize = true;
            this._lblSelectCls.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblSelectCls.Location = new System.Drawing.Point(40, 115);
            this._lblSelectCls.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblSelectCls.Name = "_lblSelectCls";
            this._lblSelectCls.Size = new System.Drawing.Size(121, 20);
            this._lblSelectCls.TabIndex = 3;
            this._lblSelectCls.Text = "选择目标分类：";
            // 
            // _cmbClassification
            // 
            this._cmbClassification.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbClassification.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbClassification.FormattingEnabled = true;
            this._cmbClassification.Location = new System.Drawing.Point(40, 143);
            this._cmbClassification.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cmbClassification.Name = "_cmbClassification";
            this._cmbClassification.Size = new System.Drawing.Size(176, 28);
            this._cmbClassification.TabIndex = 4;
            this._cmbClassification.SelectedIndexChanged += new System.EventHandler(this._cmbClassification_SelectedIndexChanged);
            // 
            // _btnLoad
            // 
            this._btnLoad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnLoad.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnLoad.ForeColor = System.Drawing.Color.White;
            this._btnLoad.Location = new System.Drawing.Point(235, 143);
            this._btnLoad.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnLoad.Name = "_btnLoad";
            this._btnLoad.Size = new System.Drawing.Size(186, 31);
            this._btnLoad.TabIndex = 5;
            this._btnLoad.Text = "🔄 加载可用属性";
            this._btnLoad.UseVisualStyleBackColor = false;
            // 
            // _lblSelectProp
            // 
            this._lblSelectProp.AutoSize = true;
            this._lblSelectProp.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblSelectProp.Location = new System.Drawing.Point(21, 189);
            this._lblSelectProp.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblSelectProp.Name = "_lblSelectProp";
            this._lblSelectProp.Size = new System.Drawing.Size(121, 20);
            this._lblSelectProp.TabIndex = 6;
            this._lblSelectProp.Text = "可用属性列表：";
            // 
            // _txtSearch
            // 
            this._txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._txtSearch.ImeMode = System.Windows.Forms.ImeMode.On;
            this._txtSearch.Location = new System.Drawing.Point(21, 215);
            this._txtSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.Size = new System.Drawing.Size(195, 27);
            this._txtSearch.TabIndex = 7;
            // 
            // _lblSearchCount
            // 
            this._lblSearchCount.AutoSize = true;
            this._lblSearchCount.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblSearchCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblSearchCount.Location = new System.Drawing.Point(135, 191);
            this._lblSearchCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblSearchCount.Name = "_lblSearchCount";
            this._lblSearchCount.Size = new System.Drawing.Size(57, 19);
            this._lblSearchCount.TabIndex = 8;
            this._lblSearchCount.Text = "已选择..";
            // 
            // _clbProperties
            // 
            this._clbProperties.CheckOnClick = true;
            this._clbProperties.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._clbProperties.FormattingEnabled = true;
            this._clbProperties.Location = new System.Drawing.Point(21, 245);
            this._clbProperties.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._clbProperties.Name = "_clbProperties";
            this._clbProperties.Size = new System.Drawing.Size(195, 202);
            this._clbProperties.TabIndex = 9;
            // 
            // _pnlSelected
            // 
            this._pnlSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._pnlSelected.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pnlSelected.Controls.Add(this._lblSelectedHeader);
            this._pnlSelected.Controls.Add(this._lblSelectedCount);
            this._pnlSelected.Controls.Add(this._pnlSelectedList);
            this._pnlSelected.Controls.Add(this._btnClearAll);
            this._pnlSelected.Location = new System.Drawing.Point(555, 215);
            this._pnlSelected.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._pnlSelected.Name = "_pnlSelected";
            this._pnlSelected.Size = new System.Drawing.Size(335, 342);
            this._pnlSelected.TabIndex = 12;
            // 
            // _lblSelectedHeader
            // 
            this._lblSelectedHeader.AutoSize = true;
            this._lblSelectedHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblSelectedHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._lblSelectedHeader.Location = new System.Drawing.Point(3, 2);
            this._lblSelectedHeader.Name = "_lblSelectedHeader";
            this._lblSelectedHeader.Size = new System.Drawing.Size(179, 20);
            this._lblSelectedHeader.TabIndex = 0;
            this._lblSelectedHeader.Text = "本次插入完整属性预览";
            // 
            // _lblSelectedCount
            // 
            this._lblSelectedCount.AutoSize = true;
            this._lblSelectedCount.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblSelectedCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblSelectedCount.Location = new System.Drawing.Point(204, 3);
            this._lblSelectedCount.Name = "_lblSelectedCount";
            this._lblSelectedCount.Size = new System.Drawing.Size(35, 19);
            this._lblSelectedCount.TabIndex = 1;
            this._lblSelectedCount.Text = "0 项";
            // 
            // _pnlSelectedList
            // 
            this._pnlSelectedList.AutoScroll = true;
            this._pnlSelectedList.BackColor = System.Drawing.Color.White;
            this._pnlSelectedList.Location = new System.Drawing.Point(7, 29);
            this._pnlSelectedList.Name = "_pnlSelectedList";
            this._pnlSelectedList.Size = new System.Drawing.Size(296, 268);
            this._pnlSelectedList.TabIndex = 2;
            // 
            // _btnClearAll
            // 
            this._btnClearAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(235)))), ((int)(((byte)(238)))));
            this._btnClearAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnClearAll.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._btnClearAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this._btnClearAll.Location = new System.Drawing.Point(4, 303);
            this._btnClearAll.Name = "_btnClearAll";
            this._btnClearAll.Size = new System.Drawing.Size(298, 34);
            this._btnClearAll.TabIndex = 3;
            this._btnClearAll.Text = "清空新勾选";
            this._btnClearAll.UseVisualStyleBackColor = false;
            // 
            // _btnImport
            // 
            this._btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnImport.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._btnImport.ForeColor = System.Drawing.Color.White;
            this._btnImport.Location = new System.Drawing.Point(555, 565);
            this._btnImport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnImport.Name = "_btnImport";
            this._btnImport.Size = new System.Drawing.Size(150, 38);
            this._btnImport.TabIndex = 13;
            this._btnImport.Text = "✅ 开始插入";
            this._btnImport.UseVisualStyleBackColor = false;
            // 
            // _btnCancel
            // 
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnCancel.Location = new System.Drawing.Point(720, 565);
            this._btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(90, 38);
            this._btnCancel.TabIndex = 14;
            this._btnCancel.Text = "取消";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _grpResult
            // 
            this._grpResult.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._grpResult.Location = new System.Drawing.Point(21, 615);
            this._grpResult.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._grpResult.Name = "_grpResult";
            this._grpResult.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._grpResult.Size = new System.Drawing.Size(856, 118);
            this._grpResult.TabIndex = 15;
            this._grpResult.TabStop = false;
            this._grpResult.Text = "  导入结果  ";
            // 
            // _lblResult
            // 
            this._lblResult.AutoSize = true;
            this._lblResult.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblResult.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._lblResult.Location = new System.Drawing.Point(10, 22);
            this._lblResult.Name = "_lblResult";
            this._lblResult.Size = new System.Drawing.Size(79, 15);
            this._lblResult.TabIndex = 0;
            this._lblResult.Text = "成功：0  失败：0";
            // 
            // _txtErrors
            // 
            this._txtErrors.Font = new System.Drawing.Font("Consolas", 9F);
            this._txtErrors.Location = new System.Drawing.Point(8, 50);
            this._txtErrors.Multiline = true;
            this._txtErrors.Name = "_txtErrors";
            this._txtErrors.ReadOnly = true;
            this._txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtErrors.Size = new System.Drawing.Size(840, 44);
            this._txtErrors.TabIndex = 1;
            this._txtErrors.Text = "加载属性后，勾选要插入的属性（可在右侧预览最终属性），然后点击「开始插入」...";
            // 
            // _lblStatus
            // 
            this._lblStatus.AutoSize = true;
            this._lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._lblStatus.Location = new System.Drawing.Point(21, 640);
            this._lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(41, 20);
            this._lblStatus.TabIndex = 16;
            this._lblStatus.Text = "就绪";
            // 
            // _progressBar
            // 
            this._progressBar.Location = new System.Drawing.Point(21, 714);
            this._progressBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(869, 23);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 17;
            // 
            // _lblProgress
            // 
            this._lblProgress.AutoSize = true;
            this._lblProgress.Font = new System.Drawing.Font("Consolas", 9F);
            this._lblProgress.Location = new System.Drawing.Point(630, 668);
            this._lblProgress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblProgress.Name = "_lblProgress";
            this._lblProgress.Size = new System.Drawing.Size(0, 18);
            this._lblProgress.TabIndex = 18;
            // 
            // _pnlExisting
            // 
            this._pnlExisting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this._pnlExisting.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pnlExisting.Controls.Add(this._lblExistingHeader);
            this._pnlExisting.Controls.Add(this._lblExistingCount);
            this._pnlExisting.Controls.Add(this._pnlExistingList);
            this._pnlExisting.Location = new System.Drawing.Point(235, 215);
            this._pnlExisting.Name = "_pnlExisting";
            this._pnlExisting.Size = new System.Drawing.Size(305, 342);
            this._pnlExisting.TabIndex = 19;
            // 
            // _lblExistingHeader
            // 
            this._lblExistingHeader.AutoSize = true;
            this._lblExistingHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblExistingHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this._lblExistingHeader.Location = new System.Drawing.Point(3, 2);
            this._lblExistingHeader.Name = "_lblExistingHeader";
            this._lblExistingHeader.Size = new System.Drawing.Size(128, 20);
            this._lblExistingHeader.TabIndex = 0;
            this._lblExistingHeader.Text = "该分类已有属性";
            // 
            // _lblExistingCount
            // 
            this._lblExistingCount.AutoSize = true;
            this._lblExistingCount.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblExistingCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this._lblExistingCount.Location = new System.Drawing.Point(100, 3);
            this._lblExistingCount.Name = "_lblExistingCount";
            this._lblExistingCount.Size = new System.Drawing.Size(35, 19);
            this._lblExistingCount.TabIndex = 1;
            this._lblExistingCount.Text = "0 项";
            // 
            // _pnlExistingList
            // 
            this._pnlExistingList.AutoScroll = true;
            this._pnlExistingList.BackColor = System.Drawing.Color.White;
            this._pnlExistingList.Location = new System.Drawing.Point(7, 29);
            this._pnlExistingList.Name = "_pnlExistingList";
            this._pnlExistingList.Size = new System.Drawing.Size(279, 268);
            this._pnlExistingList.TabIndex = 2;
            // 
            // InsertPropertyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(918, 751);
            this.Controls.Add(this._lblProgress);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._grpResult);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnImport);
            this.Controls.Add(this._pnlSelected);
            this.Controls.Add(this._pnlExisting);
            this.Controls.Add(this.btnSelectNone);
            this.Controls.Add(this.btnSelectAll);
            this.Controls.Add(this._clbProperties);
            this.Controls.Add(this._lblSearchCount);
            this.Controls.Add(this._txtSearch);
            this.Controls.Add(this._lblSelectProp);
            this.Controls.Add(this._btnLoad);
            this.Controls.Add(this._cmbClassification);
            this.Controls.Add(this._lblSelectCls);
            this.Controls.Add(this._rbRoot);
            this.Controls.Add(this._rbClassification);
            this.Controls.Add(this.lblMode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InsertPropertyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "插入属性";
            this._pnlSelected.ResumeLayout(false);
            this._pnlSelected.PerformLayout();
            this._pnlExisting.ResumeLayout(false);
            this._pnlExisting.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton _rbClassification;
        private System.Windows.Forms.RadioButton _rbRoot;
        private System.Windows.Forms.Label _lblSelectCls;
        private System.Windows.Forms.ComboBox _cmbClassification;
        private System.Windows.Forms.Label _lblSelectProp;
        private System.Windows.Forms.TextBox _txtSearch;
        private System.Windows.Forms.Label _lblSearchCount;
        private System.Windows.Forms.CheckedListBox _clbProperties;
        private System.Windows.Forms.Button _btnLoad;
        private System.Windows.Forms.Panel _pnlSelected;
        private System.Windows.Forms.Label _lblSelectedHeader;
        private System.Windows.Forms.Label _lblSelectedCount;
        private System.Windows.Forms.Panel _pnlSelectedList;
        private System.Windows.Forms.Button _btnClearAll;
        private System.Windows.Forms.Button _btnImport;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.GroupBox _grpResult;
        private System.Windows.Forms.Label _lblResult;
        private System.Windows.Forms.TextBox _txtErrors;
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Label _lblProgress;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnSelectNone;
        private System.Windows.Forms.Panel _pnlExisting;
        private System.Windows.Forms.Label _lblExistingHeader;
        private System.Windows.Forms.Label _lblExistingCount;
        private System.Windows.Forms.Panel _pnlExistingList;
    }
}
