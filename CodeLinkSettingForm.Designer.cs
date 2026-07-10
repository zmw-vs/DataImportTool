namespace DataImportTool
{
    partial class CodeLinkSettingForm
    {
        /// <summary>必需的设计器变量。</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>清理所有正在使用的资源。</summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>设计器支持所需的方法 - 不要修改使用代码编辑器修改此方法的内容。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._lblProp = new System.Windows.Forms.Label();
            this._cmbProp = new System.Windows.Forms.ComboBox();
            this._lblStatus = new System.Windows.Forms.Label();
            this._lblClass = new System.Windows.Forms.Label();
            this._chkSelectAll = new System.Windows.Forms.CheckBox();
            this._btnSelectUnchecked = new System.Windows.Forms.Button();
            this._tvClass = new System.Windows.Forms.TreeView();
            this._lblRule = new System.Windows.Forms.Label();
            this._lblCodeName = new System.Windows.Forms.Label();
            this._txtCodeName = new System.Windows.Forms.TextBox();
            this._lblCodeExample = new System.Windows.Forms.Label();
            this._txtCodeExample = new System.Windows.Forms.TextBox();
            this._btnRegenExample = new System.Windows.Forms.Button();
            this._lblMerge = new System.Windows.Forms.Label();
            this._dgvMerge = new System.Windows.Forms.DataGridView();
            this.type = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.property_internal_name = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.property_internal_name_zh = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.object_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.property_merged_length = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.property_merged_fill_value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.property_merged_fill_back_flag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.json_array_connect_string = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.next_empty_clear_flag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.prev_empty_clear_flag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._btnApply = new System.Windows.Forms.Button();
            this._btnRemoveClass = new System.Windows.Forms.Button();
            this._btnLoad = new System.Windows.Forms.Button();
            this._btnSave = new System.Windows.Forms.Button();
            this._btnClose = new System.Windows.Forms.Button();
            this._btnCopy = new System.Windows.Forms.Button();
            this._ctxMerge = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._ctxDelete = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this._dgvMerge)).BeginInit();
            this._ctxMerge.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblProp
            // 
            this._lblProp.AutoSize = true;
            this._lblProp.Location = new System.Drawing.Point(20, 14);
            this._lblProp.Name = "_lblProp";
            this._lblProp.Size = new System.Drawing.Size(232, 15);
            this._lblProp.TabIndex = 0;
            this._lblProp.Text = "目标属性（输入关键字可搜索）：";
            // 
            // _cmbProp
            // 
            this._cmbProp.Location = new System.Drawing.Point(238, 11);
            this._cmbProp.Name = "_cmbProp";
            this._cmbProp.Size = new System.Drawing.Size(180, 23);
            this._cmbProp.TabIndex = 1;
            // 
            // _lblStatus
            // 
            this._lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._lblStatus.AutoEllipsis = true;
            this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this._lblStatus.Location = new System.Drawing.Point(16, 560);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(818, 30);
            this._lblStatus.TabIndex = 2;
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblClass
            // 
            this._lblClass.AutoSize = true;
            this._lblClass.Location = new System.Drawing.Point(20, 46);
            this._lblClass.Name = "_lblClass";
            this._lblClass.Size = new System.Drawing.Size(241, 15);
            this._lblClass.TabIndex = 3;
            this._lblClass.Text = "分类（勾选 = 纳入该属性领码）：";
            // 
            // _chkSelectAll
            // 
            this._chkSelectAll.AutoSize = true;
            this._chkSelectAll.Location = new System.Drawing.Point(20, 64);
            this._chkSelectAll.Name = "_chkSelectAll";
            this._chkSelectAll.Size = new System.Drawing.Size(143, 19);
            this._chkSelectAll.TabIndex = 4;
            this._chkSelectAll.Text = "全选 / 取消全选";
            this._chkSelectAll.ThreeState = true;
            this._chkSelectAll.UseVisualStyleBackColor = true;
            // 
            // _btnSelectUnchecked
            // 
            this._btnSelectUnchecked.Location = new System.Drawing.Point(148, 60);
            this._btnSelectUnchecked.Name = "_btnSelectUnchecked";
            this._btnSelectUnchecked.Size = new System.Drawing.Size(148, 26);
            this._btnSelectUnchecked.TabIndex = 4;
            this._btnSelectUnchecked.Text = "全选未勾选的分类";
            this._btnSelectUnchecked.UseVisualStyleBackColor = true;
            // 
            // _tvClass
            // 
            this._tvClass.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._tvClass.HideSelection = false;
            this._tvClass.Location = new System.Drawing.Point(20, 90);
            this._tvClass.Name = "_tvClass";
            this._tvClass.Size = new System.Drawing.Size(250, 300);
            this._tvClass.TabIndex = 5;
            // 
            // _lblRule
            // 
            this._lblRule.AutoSize = true;
            this._lblRule.Location = new System.Drawing.Point(300, 46);
            this._lblRule.Name = "_lblRule";
            this._lblRule.Size = new System.Drawing.Size(112, 15);
            this._lblRule.TabIndex = 5;
            this._lblRule.Text = "编码规则模板：";
            // 
            // _lblCodeName
            // 
            this._lblCodeName.AutoSize = true;
            this._lblCodeName.Location = new System.Drawing.Point(300, 72);
            this._lblCodeName.Name = "_lblCodeName";
            this._lblCodeName.Size = new System.Drawing.Size(97, 15);
            this._lblCodeName.TabIndex = 6;
            this._lblCodeName.Text = "编码内部名：";
            // 
            // _txtCodeName
            // 
            this._txtCodeName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtCodeName.Location = new System.Drawing.Point(470, 70);
            this._txtCodeName.Name = "_txtCodeName";
            this._txtCodeName.Size = new System.Drawing.Size(270, 25);
            this._txtCodeName.TabIndex = 7;
            // 
            // _lblCodeExample
            // 
            this._lblCodeExample.AutoSize = true;
            this._lblCodeExample.Location = new System.Drawing.Point(300, 102);
            this._lblCodeExample.Name = "_lblCodeExample";
            this._lblCodeExample.Size = new System.Drawing.Size(82, 15);
            this._lblCodeExample.TabIndex = 8;
            this._lblCodeExample.Text = "编码示例：";
            // 
            // _txtCodeExample
            // 
            this._txtCodeExample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtCodeExample.Location = new System.Drawing.Point(470, 100);
            this._txtCodeExample.Name = "_txtCodeExample";
            this._txtCodeExample.Size = new System.Drawing.Size(270, 25);
            this._txtCodeExample.TabIndex = 9;
            // 
            // _btnRegenExample
            // 
            this._btnRegenExample.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnRegenExample.Location = new System.Drawing.Point(746, 99);
            this._btnRegenExample.Name = "_btnRegenExample";
            this._btnRegenExample.Size = new System.Drawing.Size(96, 25);
            this._btnRegenExample.TabIndex = 10;
            this._btnRegenExample.Text = "重新生成示例";
            this._btnRegenExample.UseVisualStyleBackColor = true;
            // 
            // _lblMerge
            // 
            this._lblMerge.AutoSize = true;
            this._lblMerge.Location = new System.Drawing.Point(300, 132);
            this._lblMerge.Name = "_lblMerge";
            this._lblMerge.Size = new System.Drawing.Size(360, 15);
            this._lblMerge.TabIndex = 10;
            this._lblMerge.Text = "合并属性段（merge_properties，按段顺序拼接）：";
            // 
            // _dgvMerge
            // 
            this._dgvMerge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dgvMerge.ColumnHeadersHeight = 29;
            this._dgvMerge.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.type,
            this.value,
            this.property_internal_name,
            this.property_internal_name_zh,
            this.object_id,
            this.property_merged_length,
            this.property_merged_fill_value,
            this.property_merged_fill_back_flag,
            this.json_array_connect_string,
            this.next_empty_clear_flag,
            this.prev_empty_clear_flag});
            this._dgvMerge.Location = new System.Drawing.Point(300, 152);
            this._dgvMerge.Name = "_dgvMerge";
            this._dgvMerge.RowHeadersVisible = false;
            this._dgvMerge.RowHeadersWidth = 51;
            this._dgvMerge.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._dgvMerge.Size = new System.Drawing.Size(542, 283);
            this._dgvMerge.TabIndex = 11;
            // 
            // type
            // 
            this.type.HeaderText = "类型";
            this.type.MinimumWidth = 6;
            this.type.Name = "type";
            this.type.Width = 78;
            // 
            // value
            // 
            this.value.HeaderText = "固定值(value)";
            this.value.MinimumWidth = 6;
            this.value.Name = "value";
            this.value.Width = 82;
            // 
            // property_internal_name
            // 
            this.property_internal_name.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.property_internal_name.HeaderText = "属性名(下拉选)";
            this.property_internal_name.MinimumWidth = 6;
            this.property_internal_name.Name = "property_internal_name";
            this.property_internal_name.Width = 150;
            // 
            // property_internal_name_zh
            // 
            this.property_internal_name_zh.HeaderText = "属性中文名";
            this.property_internal_name_zh.MinimumWidth = 6;
            this.property_internal_name_zh.Name = "property_internal_name_zh";
            this.property_internal_name_zh.ReadOnly = true;
            this.property_internal_name_zh.Width = 150;
            // 
            // object_id
            // 
            this.object_id.HeaderText = "码段ID(object_id)";
            this.object_id.MinimumWidth = 6;
            this.object_id.Name = "object_id";
            this.object_id.Width = 86;
            // 
            // property_merged_length
            // 
            this.property_merged_length.HeaderText = "截取长度(len)";
            this.property_merged_length.MinimumWidth = 6;
            this.property_merged_length.Name = "property_merged_length";
            this.property_merged_length.Width = 92;
            // 
            // property_merged_fill_value
            // 
            this.property_merged_fill_value.HeaderText = "补位值(fill)";
            this.property_merged_fill_value.MinimumWidth = 6;
            this.property_merged_fill_value.Name = "property_merged_fill_value";
            this.property_merged_fill_value.Width = 80;
            // 
            // property_merged_fill_back_flag
            // 
            this.property_merged_fill_back_flag.HeaderText = "尾补位(back)";
            this.property_merged_fill_back_flag.MinimumWidth = 6;
            this.property_merged_fill_back_flag.Name = "property_merged_fill_back_flag";
            this.property_merged_fill_back_flag.Width = 90;
            // 
            // json_array_connect_string
            // 
            this.json_array_connect_string.HeaderText = "连接符(conn)";
            this.json_array_connect_string.MinimumWidth = 6;
            this.json_array_connect_string.Name = "json_array_connect_string";
            this.json_array_connect_string.Width = 90;
            // 
            // next_empty_clear_flag
            // 
            this.next_empty_clear_flag.HeaderText = "后段空清(next)";
            this.next_empty_clear_flag.MinimumWidth = 6;
            this.next_empty_clear_flag.Name = "next_empty_clear_flag";
            this.next_empty_clear_flag.Width = 125;
            // 
            // prev_empty_clear_flag
            // 
            this.prev_empty_clear_flag.HeaderText = "前段空清(prev)";
            this.prev_empty_clear_flag.MinimumWidth = 6;
            this.prev_empty_clear_flag.Name = "prev_empty_clear_flag";
            this.prev_empty_clear_flag.Width = 125;
            // 
            // _btnApply
            // 
            this._btnApply.Location = new System.Drawing.Point(300, 441);
            this._btnApply.Name = "_btnApply";
            this._btnApply.Size = new System.Drawing.Size(150, 32);
            this._btnApply.TabIndex = 12;
            this._btnApply.Text = "应用到勾选分类";
            // 
            // _btnRemoveClass
            // 
            this._btnRemoveClass.Location = new System.Drawing.Point(460, 441);
            this._btnRemoveClass.Name = "_btnRemoveClass";
            this._btnRemoveClass.Size = new System.Drawing.Size(130, 32);
            this._btnRemoveClass.TabIndex = 13;
            this._btnRemoveClass.Text = "移除选中分类";
            // 
            // _btnLoad
            // 
            this._btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._btnLoad.Location = new System.Drawing.Point(300, 612);
            this._btnLoad.Name = "_btnLoad";
            this._btnLoad.Size = new System.Drawing.Size(100, 32);
            this._btnLoad.TabIndex = 14;
            this._btnLoad.Text = "重新加载";
            // 
            // _btnSave
            // 
            this._btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._btnSave.Location = new System.Drawing.Point(420, 612);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(150, 32);
            this._btnSave.TabIndex = 15;
            this._btnSave.Text = "保存到数据库";
            // 
            // _btnClose
            // 
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._btnClose.Location = new System.Drawing.Point(590, 612);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(90, 32);
            this._btnClose.TabIndex = 16;
            this._btnClose.Text = "关闭";
            // 
            // _btnCopy
            // 
            this._btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnCopy.Location = new System.Drawing.Point(700, 612);
            this._btnCopy.Name = "_btnCopy";
            this._btnCopy.Size = new System.Drawing.Size(130, 32);
            this._btnCopy.TabIndex = 17;
            this._btnCopy.Text = "跨属性复制";
            // 
            // _ctxMerge
            // 
            this._ctxMerge.ImageScalingSize = new System.Drawing.Size(20, 20);
            this._ctxMerge.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._ctxDelete});
            this._ctxMerge.Name = "_ctxMerge";
            this._ctxMerge.Size = new System.Drawing.Size(154, 28);
            // 
            // _ctxDelete
            // 
            this._ctxDelete.Name = "_ctxDelete";
            this._ctxDelete.Size = new System.Drawing.Size(153, 24);
            this._ctxDelete.Text = "删除该码段";
            // 
            // CodeLinkSettingForm
            // 
            this.ClientSize = new System.Drawing.Size(850, 670);
            this.Controls.Add(this._lblProp);
            this.Controls.Add(this._cmbProp);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._lblClass);
            this.Controls.Add(this._chkSelectAll);
            this.Controls.Add(this._btnSelectUnchecked);
            this.Controls.Add(this._tvClass);
            this.Controls.Add(this._lblRule);
            this.Controls.Add(this._lblCodeName);
            this.Controls.Add(this._txtCodeName);
            this.Controls.Add(this._lblCodeExample);
            this.Controls.Add(this._txtCodeExample);
            this.Controls.Add(this._btnRegenExample);
            this.Controls.Add(this._lblMerge);
            this.Controls.Add(this._dgvMerge);
            this.Controls.Add(this._btnApply);
            this.Controls.Add(this._btnRemoveClass);
            this.Controls.Add(this._btnLoad);
            this.Controls.Add(this._btnSave);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._btnCopy);
            this.MinimumSize = new System.Drawing.Size(850, 670);
            this.Name = "CodeLinkSettingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "领码设置（class_link_code_setting / part）";
            ((System.ComponentModel.ISupportInitialize)(this._dgvMerge)).EndInit();
            this._ctxMerge.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _lblProp;
        private System.Windows.Forms.ComboBox _cmbProp;
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.Label _lblClass;
        private System.Windows.Forms.CheckBox _chkSelectAll;
        private System.Windows.Forms.Button _btnSelectUnchecked;
        private System.Windows.Forms.TreeView _tvClass;
        private System.Windows.Forms.Label _lblRule;
        private System.Windows.Forms.Label _lblCodeName;
        private System.Windows.Forms.TextBox _txtCodeName;
        private System.Windows.Forms.Label _lblCodeExample;
        private System.Windows.Forms.TextBox _txtCodeExample;
        private System.Windows.Forms.Button _btnRegenExample;
        private System.Windows.Forms.Label _lblMerge;
        private System.Windows.Forms.DataGridView _dgvMerge;
        private System.Windows.Forms.Button _btnApply;
        private System.Windows.Forms.Button _btnRemoveClass;
        private System.Windows.Forms.Button _btnLoad;
        private System.Windows.Forms.Button _btnSave;
        private System.Windows.Forms.DataGridViewComboBoxColumn type;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.DataGridViewComboBoxColumn property_internal_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn property_internal_name_zh;
        private System.Windows.Forms.DataGridViewTextBoxColumn object_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn property_merged_length;
        private System.Windows.Forms.DataGridViewTextBoxColumn property_merged_fill_value;
        private System.Windows.Forms.DataGridViewCheckBoxColumn property_merged_fill_back_flag;
        private System.Windows.Forms.DataGridViewTextBoxColumn json_array_connect_string;
        private System.Windows.Forms.DataGridViewCheckBoxColumn next_empty_clear_flag;
        private System.Windows.Forms.DataGridViewCheckBoxColumn prev_empty_clear_flag;
        private System.Windows.Forms.Button _btnClose;
        private System.Windows.Forms.Button _btnCopy;
        private System.Windows.Forms.ContextMenuStrip _ctxMerge;
        private System.Windows.Forms.ToolStripMenuItem _ctxDelete;
    }
}
