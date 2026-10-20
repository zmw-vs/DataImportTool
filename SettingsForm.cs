using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataImportTool
{
    public partial class SettingsForm : Form
    {
        /// <summary>
        /// 保存后的配置对象（供调用方读取）
        /// </summary>
        public AppConfig Config { get; private set; }

        /// <summary>
        /// 无参构造函数：供 VS 设计器实例化
        /// </summary>
        public SettingsForm()
        {
            InitializeComponent();
        }

        public SettingsForm(AppConfig currentConfig)
        {
            InitializeComponent();

            if (currentConfig == null)
                currentConfig = new AppConfig();

            // 填充表单
            txtServer.Text = currentConfig.Server;
            txtDatabase.Text = currentConfig.Database;
            txtUsername.Text = currentConfig.UserId;
            txtPassword.Text = currentConfig.Password;

            UpdateAuthControls();
        }

        private void rbAuth_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAuthControls();
        }

        private void UpdateAuthControls()
        {
            txtUsername.Enabled = rbSqlAuth.Checked;
            txtPassword.Enabled = rbSqlAuth.Checked;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            var cfg = BuildConfig();
            btnTest.Enabled = false;
            lblTestResult.Text = "测试中...";
            lblTestResult.ForeColor = System.Drawing.Color.Gray;

            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(cfg.BuildConnectionString()))
                {
                    conn.Open();
                    lblTestResult.Text = "连接成功！服务器版本：" + conn.ServerVersion;
                    lblTestResult.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblTestResult.Text = "连接失败：" + ex.Message;
                lblTestResult.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                btnTest.Enabled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show("请输入服务器地址。", "验证",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServer.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("请输入数据库名称。", "验证",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDatabase.Focus();
                return;
            }
            Config = BuildConfig();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private AppConfig BuildConfig()
        {
            return new AppConfig
            {
                Server = txtServer.Text.Trim(),
                Database = txtDatabase.Text.Trim(),
                UserId = rbSqlAuth.Checked ? txtUsername.Text.Trim() : string.Empty,
                Password = rbSqlAuth.Checked ? txtPassword.Text.Trim() : string.Empty
            };
        }
    }
}
