using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class ComboInputForm
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
            this._lblPrompt = new System.Windows.Forms.Label();
            this._cmbGroup = new System.Windows.Forms.ComboBox();
            this._btnOK = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _lblPrompt
            // 
            this._lblPrompt.AutoSize = true;
            this._lblPrompt.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._lblPrompt.Location = new System.Drawing.Point(21, 18);
            this._lblPrompt.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblPrompt.Name = "_lblPrompt";
            this._lblPrompt.Size = new System.Drawing.Size(121, 20);
            this._lblPrompt.TabIndex = 0;
            this._lblPrompt.Text = "选择目标分类组";
            // 
            // _cmbGroup
            // 
            this._cmbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbGroup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._cmbGroup.FormattingEnabled = true;
            this._cmbGroup.Location = new System.Drawing.Point(21, 51);
            this._cmbGroup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cmbGroup.Name = "_cmbGroup";
            this._cmbGroup.Size = new System.Drawing.Size(585, 28);
            this._cmbGroup.TabIndex = 1;
            // 
            // _btnOK
            // 
            this._btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOK.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnOK.ForeColor = System.Drawing.Color.White;
            this._btnOK.Location = new System.Drawing.Point(341, 104);
            this._btnOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnOK.Name = "_btnOK";
            this._btnOK.Size = new System.Drawing.Size(120, 35);
            this._btnOK.TabIndex = 2;
            this._btnOK.Text = "确定";
            this._btnOK.UseVisualStyleBackColor = false;
            // 
            // _btnCancel
            // 
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._btnCancel.Location = new System.Drawing.Point(483, 104);
            this._btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(120, 35);
            this._btnCancel.TabIndex = 3;
            this._btnCancel.Text = "取消";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // ComboInputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 208);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOK);
            this.Controls.Add(this._cmbGroup);
            this.Controls.Add(this._lblPrompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ComboInputForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _lblPrompt;
        private System.Windows.Forms.ComboBox _cmbGroup;
        private System.Windows.Forms.Button _btnOK;
        private System.Windows.Forms.Button _btnCancel;
    }
}
