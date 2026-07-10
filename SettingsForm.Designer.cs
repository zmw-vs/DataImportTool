using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.grpDb = new System.Windows.Forms.GroupBox();
            this.lblTestResult = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rbSqlAuth = new System.Windows.Forms.RadioButton();
            this.rbWinAuth = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpDb.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpDb
            // 
            this.grpDb.Controls.Add(this.lblTestResult);
            this.grpDb.Controls.Add(this.btnTest);
            this.grpDb.Controls.Add(this.label6);
            this.grpDb.Controls.Add(this.txtDatabase);
            this.grpDb.Controls.Add(this.label5);
            this.grpDb.Controls.Add(this.txtServer);
            this.grpDb.Controls.Add(this.label4);
            this.grpDb.Controls.Add(this.txtPassword);
            this.grpDb.Controls.Add(this.label3);
            this.grpDb.Controls.Add(this.txtUsername);
            this.grpDb.Controls.Add(this.label2);
            this.grpDb.Controls.Add(this.rbSqlAuth);
            this.grpDb.Controls.Add(this.rbWinAuth);
            this.grpDb.Controls.Add(this.label1);
            this.grpDb.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.grpDb.Location = new System.Drawing.Point(10, 8);
            this.grpDb.Margin = new System.Windows.Forms.Padding(2);
            this.grpDb.Name = "grpDb";
            this.grpDb.Padding = new System.Windows.Forms.Padding(6, 4, 6, 5);
            this.grpDb.Size = new System.Drawing.Size(540, 248);
            this.grpDb.TabIndex = 0;
            this.grpDb.TabStop = false;
            this.grpDb.Text = "  数据库连接  ";
            // 
            // lblTestResult
            // 
            this.lblTestResult.AutoSize = true;
            this.lblTestResult.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblTestResult.ForeColor = System.Drawing.Color.Gray;
            this.lblTestResult.Location = new System.Drawing.Point(90, 204);
            this.lblTestResult.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTestResult.Name = "lblTestResult";
            this.lblTestResult.Size = new System.Drawing.Size(54, 20);
            this.lblTestResult.TabIndex = 15;
            this.lblTestResult.Text = "未测试";
            // 
            // btnTest
            // 
            this.btnTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTest.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnTest.ForeColor = System.Drawing.Color.White;
            this.btnTest.Location = new System.Drawing.Point(444, 210);
            this.btnTest.Margin = new System.Windows.Forms.Padding(2);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(88, 31);
            this.btnTest.TabIndex = 14;
            this.btnTest.Text = "测试连接";
            this.btnTest.UseVisualStyleBackColor = false;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label6.Location = new System.Drawing.Point(18, 204);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 20);
            this.label6.TabIndex = 13;
            this.label6.Text = "状态：";
            // 
            // txtDatabase
            // 
            this.txtDatabase.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtDatabase.Location = new System.Drawing.Point(94, 66);
            this.txtDatabase.Margin = new System.Windows.Forms.Padding(2);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(185, 27);
            this.txtDatabase.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label5.Location = new System.Drawing.Point(14, 70);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 20);
            this.label5.TabIndex = 11;
            this.label5.Text = "数据库：";
            // 
            // txtServer
            // 
            this.txtServer.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtServer.Location = new System.Drawing.Point(94, 28);
            this.txtServer.Margin = new System.Windows.Forms.Padding(2);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(185, 27);
            this.txtServer.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label4.Location = new System.Drawing.Point(18, 176);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 20);
            this.label4.TabIndex = 9;
            this.label4.Text = "密码：";
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtPassword.Location = new System.Drawing.Point(94, 169);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(2);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(181, 27);
            this.txtPassword.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label3.Location = new System.Drawing.Point(14, 138);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 20);
            this.label3.TabIndex = 7;
            this.label3.Text = "用户名：";
            // 
            // txtUsername
            // 
            this.txtUsername.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.txtUsername.Location = new System.Drawing.Point(94, 134);
            this.txtUsername.Margin = new System.Windows.Forms.Padding(2);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(181, 27);
            this.txtUsername.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label2.Location = new System.Drawing.Point(14, 32);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "服务器：";
            // 
            // rbSqlAuth
            // 
            this.rbSqlAuth.AutoSize = true;
            this.rbSqlAuth.Checked = true;
            this.rbSqlAuth.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.rbSqlAuth.Location = new System.Drawing.Point(102, 97);
            this.rbSqlAuth.Margin = new System.Windows.Forms.Padding(2);
            this.rbSqlAuth.Name = "rbSqlAuth";
            this.rbSqlAuth.Size = new System.Drawing.Size(123, 24);
            this.rbSqlAuth.TabIndex = 4;
            this.rbSqlAuth.TabStop = true;
            this.rbSqlAuth.Text = "SQL 身份验证";
            this.rbSqlAuth.UseVisualStyleBackColor = true;
            this.rbSqlAuth.CheckedChanged += new System.EventHandler(this.rbAuth_CheckedChanged);
            // 
            // rbWinAuth
            // 
            this.rbWinAuth.AutoSize = true;
            this.rbWinAuth.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.rbWinAuth.Location = new System.Drawing.Point(245, 97);
            this.rbWinAuth.Margin = new System.Windows.Forms.Padding(2);
            this.rbWinAuth.Name = "rbWinAuth";
            this.rbWinAuth.Size = new System.Drawing.Size(161, 24);
            this.rbWinAuth.TabIndex = 3;
            this.rbWinAuth.Text = "Windows 身份验证";
            this.rbWinAuth.UseVisualStyleBackColor = true;
            this.rbWinAuth.CheckedChanged += new System.EventHandler(this.rbAuth_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label1.Location = new System.Drawing.Point(14, 101);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "认证方式：";
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(376, 260);
            this.btnSave.Margin = new System.Windows.Forms.Padding(2);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(93, 41);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(181)))), ((int)(((byte)(189)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(473, 260);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(77, 41);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 307);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.grpDb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "设置";
            this.grpDb.ResumeLayout(false);
            this.grpDb.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpDb;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rbSqlAuth;
        private System.Windows.Forms.RadioButton rbWinAuth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Label lblTestResult;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
