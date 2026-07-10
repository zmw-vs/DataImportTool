using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataImportTool
{
    /// <summary>
    /// 跨属性复制领码规则：左边是源属性的分类和规则，右边是目标属性。
    /// 勾选分类复制，目标里已有该分类就覆盖它的 code_rule，没有就新建。
    /// 只动内存里的 _rules JArray，落库交给主窗体「保存到数据库」写进 config_item.content。
    /// 不动 version / owner_type / owner_object_id，也不删记录。
    /// </summary>
    public class CodeLinkCopyForm : Form
    {
        private readonly JObject _root;
        private readonly JArray _rules;
        private readonly List<MetaPropertyOption> _allProps;

        // 属性中英文映射（仅前端展示，后台一律 InternalName）
        private Dictionary<string, MetaPropertyOption> _propByInternal = new Dictionary<string, MetaPropertyOption>();
        private Dictionary<string, string> _propDisplayToInternal = new Dictionary<string, string>();
        private Dictionary<string, string> _propInternalToDisplay = new Dictionary<string, string>();

        private string _srcProp = string.Empty;
        private string _tgtProp = string.Empty;
        private JObject _srcRule = null;
        private JObject _tgtRule = null;

        // 源分类列表（与 _clbSrc 一一对应）
        private List<long> _srcOids = new List<long>();
        private int _selSrcIndex = -1;
        // 每个源分类「将要复制过去」的 code_rule（初始化为源自带规则的深拷贝，可在编辑器中改写）
        private Dictionary<long, JObject> _copyRules = new Dictionary<long, JObject>();

        private bool _refreshing = false;

        // ---- 源视图控件 ----
        private Label _lblSrc;
        private ComboBox _cmbSrc;
        private Label _lblTgt;
        private ComboBox _cmbTgt;
        private Label _lblHint;
        private Label _lblSrcClass;
        private CheckedListBox _clbSrc;
        private Label _lblSrcRule;
        private Label _lblSrcCodeName;
        private TextBox _txtSrcCodeName;
        private Label _lblSrcCodeExample;
        private TextBox _txtSrcCodeExample;
        private Label _lblSrcMerge;
        private DataGridView _dgvSrcMerge;
        private DataGridViewComboBoxColumn _cType;
        private DataGridViewTextBoxColumn _cValue;
        private DataGridViewTextBoxColumn _cPropName;
        private DataGridViewTextBoxColumn _cPropNameZh;
        private DataGridViewTextBoxColumn _cObjId;
        private DataGridViewTextBoxColumn _cLen;
        private DataGridViewTextBoxColumn _cFill;
        private DataGridViewCheckBoxColumn _cFillBack;
        private DataGridViewTextBoxColumn _cConn;
        private DataGridViewCheckBoxColumn _cNextClear;
        private DataGridViewCheckBoxColumn _cPrevClear;
        // ---- 目标视图控件 ----
        private Label _lblTgtInfo;
        private ListBox _lstTgt;
        private Label _lblSummary;
        // ---- 按钮 ----
        private Button _btnExec;
        private Button _btnClose;
        private Button _btnSelectAll;

        public CodeLinkCopyForm(JObject root, JArray rules, List<MetaPropertyOption> allProps)
        {
            _root = root;
            _rules = rules;
            _allProps = allProps ?? new List<MetaPropertyOption>();
            BuildPropMaps();
            InitializeComponent();
            InitCombos();
        }

        private void BuildPropMaps()
        {
            _propByInternal.Clear();
            _propDisplayToInternal.Clear();
            _propInternalToDisplay.Clear();
            foreach (var p in _allProps)
            {
                string disp = string.IsNullOrWhiteSpace(p.DisplayName)
                    ? p.InternalName
                    : p.InternalName + "（" + p.DisplayName + "）";
                _propByInternal[p.InternalName] = p;
                _propDisplayToInternal[disp] = p.InternalName;
                _propInternalToDisplay[p.InternalName] = disp;
            }
        }

        private void InitializeComponent()
        {
            this._lblSrc = new System.Windows.Forms.Label();
            this._cmbSrc = new System.Windows.Forms.ComboBox();
            this._lblTgt = new System.Windows.Forms.Label();
            this._cmbTgt = new System.Windows.Forms.ComboBox();
            this._lblHint = new System.Windows.Forms.Label();
            this._lblSrcClass = new System.Windows.Forms.Label();
            this._clbSrc = new System.Windows.Forms.CheckedListBox();
            this._lblSrcRule = new System.Windows.Forms.Label();
            this._lblSrcCodeName = new System.Windows.Forms.Label();
            this._txtSrcCodeName = new System.Windows.Forms.TextBox();
            this._lblSrcCodeExample = new System.Windows.Forms.Label();
            this._txtSrcCodeExample = new System.Windows.Forms.TextBox();
            this._lblSrcMerge = new System.Windows.Forms.Label();
            this._dgvSrcMerge = new System.Windows.Forms.DataGridView();
            this._cType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this._cValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._cPropName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._cPropNameZh = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._cObjId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._cLen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._cFill = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._cFillBack = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._cConn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._cNextClear = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._cPrevClear = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._lblTgtInfo = new System.Windows.Forms.Label();
            this._lstTgt = new System.Windows.Forms.ListBox();
            this._lblSummary = new System.Windows.Forms.Label();
            this._btnExec = new System.Windows.Forms.Button();
            this._btnClose = new System.Windows.Forms.Button();
            this._btnSelectAll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._dgvSrcMerge)).BeginInit();
            this.SuspendLayout();
            // 
            // _lblSrc
            // 
            this._lblSrc.AutoSize = true;
            this._lblSrc.Location = new System.Drawing.Point(20, 14);
            this._lblSrc.Name = "_lblSrc";
            this._lblSrc.Size = new System.Drawing.Size(60, 15);
            this._lblSrc.TabIndex = 0;
            this._lblSrc.Text = "源属性：";
            // 
            // _cmbSrc
            // 
            this._cmbSrc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbSrc.Location = new System.Drawing.Point(80, 11);
            this._cmbSrc.Name = "_cmbSrc";
            this._cmbSrc.Size = new System.Drawing.Size(200, 23);
            this._cmbSrc.TabIndex = 1;
            // 
            // _lblTgt
            // 
            this._lblTgt.AutoSize = true;
            this._lblTgt.Location = new System.Drawing.Point(300, 14);
            this._lblTgt.Name = "_lblTgt";
            this._lblTgt.Size = new System.Drawing.Size(60, 15);
            this._lblTgt.TabIndex = 2;
            this._lblTgt.Text = "目标属性：";
            // 
            // _cmbTgt
            // 
            this._cmbTgt.Location = new System.Drawing.Point(380, 11);
            this._cmbTgt.Name = "_cmbTgt";
            this._cmbTgt.Size = new System.Drawing.Size(220, 23);
            this._cmbTgt.TabIndex = 3;
            // 
            // _lblHint
            // 
            this._lblHint.AutoSize = true;
            this._lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this._lblHint.Location = new System.Drawing.Point(610, 16);
            this._lblHint.Name = "_lblHint";
            this._lblHint.Size = new System.Drawing.Size(200, 15);
            this._lblHint.TabIndex = 4;
            this._lblHint.Text = "（目标可输入新属性名以新建规则）";
            // 
            // _lblSrcClass
            // 
            this._lblSrcClass.AutoSize = true;
            this._lblSrcClass.Location = new System.Drawing.Point(20, 46);
            this._lblSrcClass.Name = "_lblSrcClass";
            this._lblSrcClass.Size = new System.Drawing.Size(240, 15);
            this._lblSrcClass.TabIndex = 5;
            this._lblSrcClass.Text = "源分类（勾选=复制，选中=编辑下侧规则）：";
            // 
            // _clbSrc
            // 
            this._clbSrc.CheckOnClick = true;
            this._clbSrc.Location = new System.Drawing.Point(20, 66);
            this._clbSrc.Name = "_clbSrc";
            this._clbSrc.Size = new System.Drawing.Size(250, 250);
            this._clbSrc.TabIndex = 6;
            // 
            // _lblSrcRule
            // 
            this._lblSrcRule.AutoSize = true;
            this._lblSrcRule.Location = new System.Drawing.Point(20, 358);
            this._lblSrcRule.Name = "_lblSrcRule";
            this._lblSrcRule.Size = new System.Drawing.Size(360, 15);
            this._lblSrcRule.TabIndex = 7;
            this._lblSrcRule.Text = "复制规则（可自定义，套用到上方选中分类）：";
            // 
            // _lblSrcCodeName
            // 
            this._lblSrcCodeName.AutoSize = true;
            this._lblSrcCodeName.Location = new System.Drawing.Point(20, 384);
            this._lblSrcCodeName.Name = "_lblSrcCodeName";
            this._lblSrcCodeName.Size = new System.Drawing.Size(90, 15);
            this._lblSrcCodeName.TabIndex = 8;
            this._lblSrcCodeName.Text = "编码内部名：";
            // 
            // _txtSrcCodeName
            // 
            this._txtSrcCodeName.Location = new System.Drawing.Point(110, 382);
            this._txtSrcCodeName.Name = "_txtSrcCodeName";
            this._txtSrcCodeName.Size = new System.Drawing.Size(180, 23);
            this._txtSrcCodeName.TabIndex = 9;
            // 
            // _lblSrcCodeExample
            // 
            this._lblSrcCodeExample.AutoSize = true;
            this._lblSrcCodeExample.Location = new System.Drawing.Point(300, 384);
            this._lblSrcCodeExample.Name = "_lblSrcCodeExample";
            this._lblSrcCodeExample.Size = new System.Drawing.Size(70, 15);
            this._lblSrcCodeExample.TabIndex = 10;
            this._lblSrcCodeExample.Text = "编码示例：";
            // 
            // _txtSrcCodeExample
            // 
            this._txtSrcCodeExample.Location = new System.Drawing.Point(380, 382);
            this._txtSrcCodeExample.Name = "_txtSrcCodeExample";
            this._txtSrcCodeExample.Size = new System.Drawing.Size(250, 23);
            this._txtSrcCodeExample.TabIndex = 11;
            // 
            // _lblSrcMerge
            // 
            this._lblSrcMerge.AutoSize = true;
            this._lblSrcMerge.Location = new System.Drawing.Point(20, 416);
            this._lblSrcMerge.Name = "_lblSrcMerge";
            this._lblSrcMerge.Size = new System.Drawing.Size(360, 15);
            this._lblSrcMerge.TabIndex = 12;
            this._lblSrcMerge.Text = "合并属性段（merge_properties，按段顺序拼接）：";
            // 
            // _dgvSrcMerge
            // 
            this._dgvSrcMerge.ColumnHeadersHeight = 29;
            this._dgvSrcMerge.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._cType,
            this._cValue,
            this._cPropName,
            this._cPropNameZh,
            this._cObjId,
            this._cLen,
            this._cFill,
            this._cFillBack,
            this._cConn,
            this._cNextClear,
            this._cPrevClear});
            this._dgvSrcMerge.Location = new System.Drawing.Point(20, 436);
            this._dgvSrcMerge.Name = "_dgvSrcMerge";
            this._dgvSrcMerge.RowHeadersVisible = false;
            this._dgvSrcMerge.Size = new System.Drawing.Size(640, 230);
            this._dgvSrcMerge.TabIndex = 13;
            // 
            // _cType
            // 
            this._cType.HeaderText = "类型(type)";
            this._cType.Items.AddRange(new object[] {
            "fixed",
            "property_number",
            "property_value",
            "serial_code"});
            this._cType.Name = "type";
            this._cType.Width = 78;
            // 
            // _cValue
            // 
            this._cValue.HeaderText = "固定值(value)";
            this._cValue.Name = "value";
            this._cValue.Width = 82;
            // 
            // _cPropName
            // 
            this._cPropName.HeaderText = "属性名(英文)";
            this._cPropName.Name = "property_internal_name";
            this._cPropName.Width = 130;
            // 
            // _cPropNameZh
            // 
            this._cPropNameZh.HeaderText = "属性中文名";
            this._cPropNameZh.Name = "property_internal_name_zh";
            this._cPropNameZh.ReadOnly = true;
            this._cPropNameZh.Width = 150;
            // 
            // _cObjId
            // 
            this._cObjId.HeaderText = "码段ID(object_id)";
            this._cObjId.Name = "object_id";
            this._cObjId.Width = 86;
            // 
            // _cLen
            // 
            this._cLen.HeaderText = "截取长度(len)";
            this._cLen.Name = "property_merged_length";
            this._cLen.Width = 92;
            // 
            // _cFill
            // 
            this._cFill.HeaderText = "补位值(fill)";
            this._cFill.Name = "property_merged_fill_value";
            this._cFill.Width = 80;
            // 
            // _cFillBack
            // 
            this._cFillBack.HeaderText = "尾补位(back)";
            this._cFillBack.Name = "property_merged_fill_back_flag";
            this._cFillBack.Width = 90;
            // 
            // _cConn
            // 
            this._cConn.HeaderText = "连接符(conn)";
            this._cConn.Name = "json_array_connect_string";
            this._cConn.Width = 90;
            // 
            // _cNextClear
            // 
            this._cNextClear.HeaderText = "后段空清(next)";
            this._cNextClear.Name = "next_empty_clear_flag";
            // 
            // _cPrevClear
            // 
            this._cPrevClear.HeaderText = "前段空清(prev)";
            this._cPrevClear.Name = "prev_empty_clear_flag";
            // 
            // _lblTgtInfo
            // 
            this._lblTgtInfo.AutoSize = true;
            this._lblTgtInfo.Location = new System.Drawing.Point(680, 46);
            this._lblTgtInfo.Name = "_lblTgtInfo";
            this._lblTgtInfo.Size = new System.Drawing.Size(240, 15);
            this._lblTgtInfo.TabIndex = 14;
            this._lblTgtInfo.Text = "目标属性现状（分类 + 领码）：";
            // 
            // _lstTgt
            // 
            this._lstTgt.FormattingEnabled = true;
            this._lstTgt.Location = new System.Drawing.Point(680, 66);
            this._lstTgt.Name = "_lstTgt";
            this._lstTgt.Size = new System.Drawing.Size(340, 470);
            this._lstTgt.TabIndex = 15;
            // 
            // _lblSummary
            // 
            this._lblSummary.AutoSize = true;
            this._lblSummary.Location = new System.Drawing.Point(680, 544);
            this._lblSummary.Name = "_lblSummary";
            this._lblSummary.Size = new System.Drawing.Size(340, 30);
            this._lblSummary.TabIndex = 16;
            this._lblSummary.Text = "操作预览：请先在左侧勾选要复制的分类。";
            // 
            // _btnExec
            // 
            this._btnExec.Location = new System.Drawing.Point(20, 676);
            this._btnExec.Name = "_btnExec";
            this._btnExec.Size = new System.Drawing.Size(180, 32);
            this._btnExec.TabIndex = 17;
            this._btnExec.Text = "执行复制（批量迁移）";
            // 
            // _btnClose
            // 
            this._btnClose.Location = new System.Drawing.Point(220, 676);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(120, 32);
            this._btnClose.TabIndex = 18;
            this._btnClose.Text = "关闭";
            // 
            // _btnSelectAll
            // 
            this._btnSelectAll.Location = new System.Drawing.Point(20, 320);
            this._btnSelectAll.Name = "_btnSelectAll";
            this._btnSelectAll.Size = new System.Drawing.Size(96, 24);
            this._btnSelectAll.TabIndex = 19;
            this._btnSelectAll.Text = "全选";
            this._btnSelectAll.UseVisualStyleBackColor = true;
            // 
            // CodeLinkCopyForm
            // 
            this.ClientSize = new System.Drawing.Size(1040, 730);
            this.Controls.Add(this._lblSrc);
            this.Controls.Add(this._cmbSrc);
            this.Controls.Add(this._lblTgt);
            this.Controls.Add(this._cmbTgt);
            this.Controls.Add(this._lblHint);
            this.Controls.Add(this._lblSrcClass);
            this.Controls.Add(this._clbSrc);
            this.Controls.Add(this._lblSrcRule);
            this.Controls.Add(this._lblSrcCodeName);
            this.Controls.Add(this._txtSrcCodeName);
            this.Controls.Add(this._lblSrcCodeExample);
            this.Controls.Add(this._txtSrcCodeExample);
            this.Controls.Add(this._lblSrcMerge);
            this.Controls.Add(this._dgvSrcMerge);
            this.Controls.Add(this._lblTgtInfo);
            this.Controls.Add(this._lstTgt);
            this.Controls.Add(this._lblSummary);
            this.Controls.Add(this._btnExec);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._btnSelectAll);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "CodeLinkCopyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "领码设置 - 跨属性分类规则复制";
            ((System.ComponentModel.ISupportInitialize)(this._dgvSrcMerge)).EndInit();
            // 事件订阅：务必在 ResumeLayout 前完成
            this._cmbSrc.SelectedIndexChanged += _cmbSrc_SelectedIndexChanged;
            this._cmbTgt.TextChanged += _cmbTgt_TextChanged;
            this._clbSrc.ItemCheck += _clbSrc_ItemCheck;
            this._clbSrc.SelectedIndexChanged += _clbSrc_SelectedIndexChanged;
            this._dgvSrcMerge.CellFormatting += _dgvSrcMerge_CellFormatting;
            this._btnExec.Click += _btnExec_Click;
            this._btnClose.Click += _btnClose_Click;
            this._btnSelectAll.Click += _btnSelectAll_Click;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitCombos()
        {
            // 源属性：必须已在配置中存在（有规则才能复制）
            _refreshing = true;
            _cmbSrc.Items.Clear();
            foreach (JObject r in _rules)
            {
                string p = (string)r["target_property"];
                if (string.IsNullOrEmpty(p)) continue;
                string disp = DisplayOf(p);
                if (!_cmbSrc.Items.Contains(disp))
                    _cmbSrc.Items.Add(disp);
            }
            // 目标属性：候选属性全量 + 已有配置属性（可输入新属性名）
            _cmbTgt.Items.Clear();
            foreach (var p in _allProps)
            {
                if (_propInternalToDisplay.ContainsKey(p.InternalName))
                    _cmbTgt.Items.Add(_propInternalToDisplay[p.InternalName]);
            }
            foreach (JObject r in _rules)
            {
                string p = (string)r["target_property"];
                if (!string.IsNullOrEmpty(p)) _cmbTgt.Items.Add(DisplayOf(p));
            }
            _refreshing = false;

            if (_cmbSrc.Items.Count > 0)
                _cmbSrc.SelectedIndex = 0;
            else
                MessageBox.Show("当前配置中没有任何属性的领码规则，无法作为复制源。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string DisplayOf(string internalName)
        {
            if (_propInternalToDisplay.ContainsKey(internalName))
                return _propInternalToDisplay[internalName];
            _propInternalToDisplay[internalName] = internalName;
            _propDisplayToInternal[internalName] = internalName;
            return internalName;
        }

        private string ResolveInternalName(ComboBox cmb)
        {
            string t = cmb.Text.Trim();
            if (_propDisplayToInternal.TryGetValue(t, out string internalName))
                return internalName;
            return t;
        }

        private JObject GetRule(string prop)
        {
            if (string.IsNullOrEmpty(prop)) return null;
            foreach (JObject r in _rules)
                if ((string)r["target_property"] == prop) return r;
            return null;
        }

        private JArray GetClassifications(JObject rule)
        {
            if (rule == null) return new JArray();
            var oc = rule["object_classification"] as JArray;
            return oc ?? new JArray();
        }

        private void _cmbSrc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_refreshing) return;
            WriteBackEditor(); // 保存上一次编辑的分类
            _srcProp = ResolveInternalName(_cmbSrc);
            _srcRule = GetRule(_srcProp);
            LoadSource();
            RefreshRightView();
        }

        private void LoadSource()
        {
            _refreshing = true;
            _clbSrc.Items.Clear();
            _srcOids.Clear();
            _copyRules.Clear();
            _selSrcIndex = -1;
            if (_srcRule != null)
            {
                foreach (var it in GetClassifications(_srcRule))
                {
                    long oid = (long)it["object_id"];
                    string name = it["name"] != null ? (string)it["name"] : string.Empty;
                    _clbSrc.Items.Add(name + " (" + oid + ")");
                    _srcOids.Add(oid);
                    // 初始化「待复制规则」= 源分类自带的 code_rule 深拷贝（保留各分类差异）
                    var cr = it["code_rule"] as JObject;
                    _copyRules[oid] = cr != null ? (JObject)cr.DeepClone() : DefaultCodeRule();
                }
            }
            ClearEditor();
            _refreshing = false;
        }

        private JObject DefaultCodeRule()
        {
            var def = new JObject();
            def["code_internal_name"] = "item_code";
            def["code_example"] = string.Empty;
            def["merge_properties"] = new JArray();
            return def;
        }

        private void _clbSrc_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // 仅改变勾选态，复制时统一读取；同时刷新右侧覆盖/新增预览
            if (_refreshing) return;
            // ItemCheck 事件在勾选状态真正落盘前触发，此时 GetItemChecked 仍读旧值。
            // 用 BeginInvoke 延迟到状态落盘后再刷新，统计/预览才与界面一致。
            BeginInvoke(new Action(() => RefreshRightView()));
        }

        private void _clbSrc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_refreshing) return;
            WriteBackEditor(); // 保存上一次选中分类的编辑
            int idx = _clbSrc.SelectedIndex;
            _selSrcIndex = idx;
            if (idx >= 0 && idx < _srcOids.Count)
            {
                long oid = _srcOids[idx];
                if (_copyRules.ContainsKey(oid))
                    LoadEditor(_copyRules[oid]);
                else
                    ClearEditor();
            }
            else
            {
                ClearEditor();
            }
        }

        private void _cmbTgt_TextChanged(object sender, EventArgs e)
        {
            if (_refreshing) return;
            _tgtProp = ResolveInternalName(_cmbTgt);
            _tgtRule = GetRule(_tgtProp);
            RefreshRightView();
        }

        /// <summary>把编辑器当前内容写回「当前选中源分类」的待复制规则。</summary>
        private void WriteBackEditor()
        {
            if (_selSrcIndex >= 0 && _selSrcIndex < _srcOids.Count)
            {
                long oid = _srcOids[_selSrcIndex];
                if (_copyRules.ContainsKey(oid))
                    _copyRules[oid] = BuildTemplateFromUI();
            }
        }

        private JObject BuildTemplateFromUI()
        {
            var cr = new JObject();
            cr["code_internal_name"] = _txtSrcCodeName.Text.Trim();
            cr["code_example"] = _txtSrcCodeExample.Text.Trim();
            var mp = new JArray();
            foreach (DataGridViewRow row in _dgvSrcMerge.Rows)
            {
                if (row.IsNewRow) continue;
                string type = GetCell(row, "type");
                if (string.IsNullOrWhiteSpace(type)) continue;
                var seg = new JObject();
                seg["type"] = type;
                seg["json_array_connect_string"] = GetCell(row, "json_array_connect_string");
                seg["property_merged_fill_back_flag"] = GetBoolCell(row, "property_merged_fill_back_flag");
                seg["property_merged_fill_value"] = GetCell(row, "property_merged_fill_value");
                seg["property_merged_length"] = GetCell(row, "property_merged_length");
                seg["next_empty_clear_flag"] = GetBoolCell(row, "next_empty_clear_flag");
                seg["prev_empty_clear_flag"] = GetBoolCell(row, "prev_empty_clear_flag");
                if (type == "fixed") seg["value"] = GetCell(row, "value");
                if (type == "property_number" || type == "property_value") seg["property_internal_name"] = GetCell(row, "property_internal_name");
                if (type == "serial_code") { seg["internal_name"] = "code_rule_segment"; seg["object_id"] = GetLongCell(row, "object_id"); }
                mp.Add(seg);
            }
            cr["merge_properties"] = mp;
            return cr;
        }

        private void LoadEditor(JObject cr)
        {
            _refreshing = true;
            _dgvSrcMerge.Rows.Clear();
            if (cr == null)
            {
                _txtSrcCodeName.Text = string.Empty;
                _txtSrcCodeExample.Text = string.Empty;
                _refreshing = false;
                return;
            }
            _txtSrcCodeName.Text = cr["code_internal_name"] != null ? (string)cr["code_internal_name"] : string.Empty;
            _txtSrcCodeExample.Text = cr["code_example"] != null ? (string)cr["code_example"] : string.Empty;
            var mp = cr["merge_properties"] as JArray;
            if (mp != null)
            {
                foreach (var seg in mp)
                {
                    int i = _dgvSrcMerge.Rows.Add();
                    DataGridViewRow row = _dgvSrcMerge.Rows[i];
                    SetCell(row, "type", seg["type"] != null ? (string)seg["type"] : string.Empty);
                    SetCell(row, "value", seg["value"] != null ? (string)seg["value"] : string.Empty);
                    SetCell(row, "property_internal_name", seg["property_internal_name"] != null ? (string)seg["property_internal_name"] : string.Empty);
                    SetCell(row, "object_id", seg["object_id"] != null ? seg["object_id"].ToString() : string.Empty);
                    SetCell(row, "property_merged_length", seg["property_merged_length"] != null ? (string)seg["property_merged_length"] : string.Empty);
                    SetCell(row, "property_merged_fill_value", seg["property_merged_fill_value"] != null ? (string)seg["property_merged_fill_value"] : string.Empty);
                    SetCell(row, "property_merged_fill_back_flag", seg["property_merged_fill_back_flag"] != null ? (bool)seg["property_merged_fill_back_flag"] : false);
                    SetCell(row, "json_array_connect_string", seg["json_array_connect_string"] != null ? (string)seg["json_array_connect_string"] : string.Empty);
                    SetCell(row, "next_empty_clear_flag", seg["next_empty_clear_flag"] != null ? (bool)seg["next_empty_clear_flag"] : false);
                    SetCell(row, "prev_empty_clear_flag", seg["prev_empty_clear_flag"] != null ? (bool)seg["prev_empty_clear_flag"] : false);
                }
            }
            _refreshing = false;
        }

        private void ClearEditor()
        {
            LoadEditor(null);
        }

        /// <summary>右侧目标视图：展示目标属性现状，并按勾选情况标注「将覆盖」；底部汇总批量/差异覆盖计划。
        /// 同时刷新左侧「已勾选 N 个」计数，让用户实时看到勾选结果。</summary>
        private void RefreshRightView()
        {
            int checkedCount = CheckedSrcOids().Count;
            _lblSrcClass.Text = "源分类（勾选=复制，选中=编辑下侧规则）：已勾选 " + checkedCount + " 个";
            bool allChecked = _clbSrc.Items.Count > 0 && _clbSrc.CheckedItems.Count == _clbSrc.Items.Count;
            _btnSelectAll.Text = allChecked ? "取消全选" : "全选";
            _lstTgt.Items.Clear();
            if (string.IsNullOrEmpty(_tgtProp))
            {
                _lblSummary.Text = "操作预览：请选择或输入目标属性。";
                return;
            }
            if (_tgtRule == null)
            {
                _lstTgt.Items.Add("（目标属性暂无规则，复制后将新建）");
            }
            else
            {
                foreach (var it in GetClassifications(_tgtRule))
                {
                    long oid = (long)it["object_id"];
                    string name = it["name"] != null ? (string)it["name"] : string.Empty;
                    string cin = string.Empty;
                    var cr = it["code_rule"] as JObject;
                    if (cr != null && cr["code_internal_name"] != null) cin = (string)cr["code_internal_name"];
                    bool willOverride = IsSrcChecked(oid);
                    string prefix = willOverride ? "▶ 将覆盖 " : "· ";
                    _lstTgt.Items.Add(prefix + name + " (" + oid + ") · " + cin);
                }
            }

            // 汇总
            int n = 0, m = 0, k = 0;
            var checkedOids = CheckedSrcOids();
            foreach (long oid in checkedOids)
            {
                n++;
                if (_tgtRule != null && ClassificationExists(_tgtRule, oid)) m++;
                else k++;
            }
            if (n == 0)
                _lblSummary.Text = "操作预览：请在左侧勾选要复制的分类。";
            else
                _lblSummary.Text = string.Format("将向「{0}」复制 {1} 个分类：{2} 覆盖 / {3} 新增（差异覆盖）。",
                    _tgtProp, n, m, k);
        }

        private bool IsSrcChecked(long oid)
        {
            int idx = _srcOids.IndexOf(oid);
            return idx >= 0 && _clbSrc.GetItemChecked(idx);
        }

        private List<long> CheckedSrcOids()
        {
            var list = new List<long>();
            for (int i = 0; i < _srcOids.Count; i++)
                if (_clbSrc.GetItemChecked(i)) list.Add(_srcOids[i]);
            return list;
        }

        private bool ClassificationExists(JObject rule, long oid)
        {
            foreach (var it in GetClassifications(rule))
                if ((long)it["object_id"] == oid) return true;
            return false;
        }

        private void _btnExec_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_srcProp) || _srcRule == null)
                {
                    MessageBox.Show("请先选择源属性。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _tgtProp = ResolveInternalName(_cmbTgt);
                if (string.IsNullOrEmpty(_tgtProp))
                {
                    MessageBox.Show("请选择或输入目标属性。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (_tgtProp == _srcProp)
                {
                    MessageBox.Show("源属性与目标属性不能相同。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var checkedOids = CheckedSrcOids();
                if (checkedOids.Count == 0)
                {
                    MessageBox.Show("请先在左侧勾选要复制的分类。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                WriteBackEditor(); // 保存当前编辑分类的改动

                bool srcIsByRule = (string)_srcRule["type"] == "by_rule";
                _tgtRule = GetRule(_tgtProp);
                bool createdNew = false;
                if (_tgtRule == null)
                {
                    createdNew = true;
                    _tgtRule = new JObject();
                    _tgtRule["target_property"] = _tgtProp;
                    _tgtRule["type"] = srcIsByRule ? "by_rule" : "merge_property";
                    _tgtRule["object_classification"] = new JArray();
                    // 规则级 code_rule：by_rule 克隆源的；merge_property 先放空，循环后统一置为首个复制规则
                    if (srcIsByRule && _srcRule["code_rule"] != null)
                        _tgtRule["code_rule"] = (JObject)_srcRule["code_rule"].DeepClone();
                    else
                        _tgtRule["code_rule"] = new JObject();
                    _rules.Add(_tgtRule);
                }

                int overrideCount = 0, addCount = 0;
                JObject firstCopied = null;
                foreach (long oid in checkedOids)
                {
                    if (!_copyRules.ContainsKey(oid)) continue;
                    JObject ruleToCopy = (JObject)_copyRules[oid].DeepClone();
                    if (firstCopied == null) firstCopied = ruleToCopy;

                    // 取源分类的 name / type（用于新增目标分类）
                    string srcName = string.Empty;
                    string srcClsType = "merge_property";
                    foreach (var it in GetClassifications(_srcRule))
                    {
                        if ((long)it["object_id"] == oid)
                        {
                            srcName = it["name"] != null ? (string)it["name"] : string.Empty;
                            if (it["type"] != null) srcClsType = (string)it["type"];
                            break;
                        }
                    }

                    var ocArr = _tgtRule["object_classification"] as JArray;
                    if (ocArr == null) { ocArr = new JArray(); _tgtRule["object_classification"] = ocArr; }

                    JObject existing = null;
                    foreach (var it in ocArr)
                        if ((long)it["object_id"] == oid) { existing = it as JObject; break; }

                    if (existing != null)
                    {
                        // 差异覆盖：仅覆盖该分类的 code_rule，其它字段保留
                        existing["code_rule"] = ruleToCopy;
                        overrideCount++;
                    }
                    else
                    {
                        var neo = new JObject();
                        neo["object_id"] = oid;
                        neo["name"] = srcName;
                        neo["type"] = srcClsType;
                        neo["code_rule"] = ruleToCopy;
                        ocArr.Add(neo);
                        addCount++;
                    }
                }

                // 仅当「新建」merge_property 目标时才置规则级 code_rule（与首个复制规则一致），
                // 已存在的 merge_property 目标保留其原有规则级模板，避免影响未复制分类的显示。
                if (createdNew && (string)_tgtRule["type"] != "by_rule" && firstCopied != null)
                    _tgtRule["code_rule"] = (JObject)firstCopied.DeepClone();

                string msg = string.Format("复制完成 → 目标属性「{0}」：{1} 个分类覆盖，{2} 个分类新增{3}。",
                    _tgtProp, overrideCount, addCount, createdNew ? "（目标规则为新建）" : string.Empty);
                MessageBox.Show(msg, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("复制失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>全选/取消全选：一键勾选（或取消）左侧所有源分类，随后刷新右侧覆盖预览。</summary>
        private void _btnSelectAll_Click(object sender, EventArgs e)
        {
            if (_srcOids.Count == 0) return;
            bool allChecked = _clbSrc.Items.Count > 0 && _clbSrc.CheckedItems.Count == _clbSrc.Items.Count;
            bool turnOn = !allChecked;
            _refreshing = true;   // 抑制循环期间 ItemCheck 的重复刷新
            for (int i = 0; i < _clbSrc.Items.Count; i++)
                _clbSrc.SetItemChecked(i, turnOn);
            _refreshing = false;
            RefreshRightView();
        }

        // ---- 编辑器单元格辅助 ----
        private string GetCell(DataGridViewRow row, string name)
        {
            var val = row.Cells[name].Value;
            return val == null ? string.Empty : val.ToString();
        }
        private bool GetBoolCell(DataGridViewRow row, string name)
        {
            var val = row.Cells[name].Value;
            if (val is bool) return (bool)val;
            return false;
        }
        private long GetLongCell(DataGridViewRow row, string name)
        {
            var val = row.Cells[name].Value;
            long r;
            if (val != null && long.TryParse(val.ToString(), out r)) return r;
            return 0L;
        }
        private void SetCell(DataGridViewRow row, string name, object value)
        {
            if (row.Cells[name] != null) row.Cells[name].Value = value;
        }

        /// <summary>合并属性段中文列实时显示：按 property_internal_name 反查中文名（纯展示）。</summary>
        private void _dgvSrcMerge_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _refreshing) return;
            if (_dgvSrcMerge.Columns[e.ColumnIndex].Name != "property_internal_name_zh") return;
            var internalName = _dgvSrcMerge.Rows[e.RowIndex].Cells["property_internal_name"].Value as string;
            e.Value = LookupZh(internalName);
            e.FormattingApplied = true;
        }

        private string LookupZh(string internalName)
        {
            if (string.IsNullOrWhiteSpace(internalName)) return string.Empty;
            if (_propByInternal.TryGetValue(internalName.Trim(), out MetaPropertyOption p))
                return string.IsNullOrWhiteSpace(p.DisplayName) ? string.Empty : p.DisplayName;
            return string.Empty;
        }
    }
}
