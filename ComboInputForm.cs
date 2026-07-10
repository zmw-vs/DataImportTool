using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DataImportTool
{
    public partial class ComboInputForm : Form
    {
        // 字段在 Designer.cs 中声明，InitializeComponent() 也在 Designer.cs 中
        // partial class 的字段声明只能出现在一个文件中！

        /// <summary>用户选择的分类组（取消时为 null）</summary>
        public ClassificationGroup SelectedGroup { get; private set; }

        /// <summary>
        /// 三参数构造函数：供业务代码调用
        /// </summary>
        /// <summary>
        /// 无参构造函数：供 VS 设计器实例化
        /// </summary>
        public ComboInputForm()
        {
            InitializeComponent();
        }

        public ComboInputForm(string title, List<ClassificationGroup> groups, string prompt)
        {
            InitializeComponent();
            this.Text = title;
            InitControls(groups);
        }

        /// <summary>
        /// 业务逻辑初始化（绑定数据、事件）
        /// </summary>
        private void InitControls(List<ClassificationGroup> groups)
        {
            if (groups != null && groups.Count > 0)
            {
                _cmbGroup.DataSource = groups;
                _cmbGroup.DisplayMember = "Name";
                _cmbGroup.ValueMember = "ObjectId";
            }

            _btnOK.Click += (s, e) =>
            {
                if (_cmbGroup.SelectedItem == null)
                {
                    MessageBox.Show("请先选择一个分类组。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                SelectedGroup = (ClassificationGroup)_cmbGroup.SelectedItem;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            _btnCancel.Click += (s, e) =>
            {
                SelectedGroup = null;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
        }
    }
}
