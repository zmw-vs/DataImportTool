using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataImportTool
{
    /// <summary>
    /// 领码设置（class_link_code_setting / part）快速维护工具。
    /// 交互：先选目标属性(target_property) → 左侧勾选纳入该属性的分类 → 右侧编辑一份编码规则模板 → 一键套用到所有勾选分类。
    /// 读写 config_item 的「最新版本」content，只写 content 字段，不动 version / owner_type / owner_object_id，不删任何记录。
    /// by_rule 规则：允许增删分类成员，但规则级 code_rule（含 code_rule_segments）与既有分类自带的 code_rule 一律保持原样，新增分类从同规则已有分类克隆 code_rule。
    /// </summary>
    public partial class CodeLinkSettingForm : Form
    {
        private readonly DbHelper _db;
        private List<ClassificationGroup> _classifications = new List<ClassificationGroup>();
        private List<MetaPropertyOption> _allProps = new List<MetaPropertyOption>();

        private JObject _root;
        private JArray _rules;
        private string _currentProp = string.Empty;
        private JObject _currentRule = null;
        // 右侧「合并属性段」网格当前正在编辑的对象标识：
        // null = 编辑规则级模板(_currentRule["code_rule"])；有值 = 编辑该 object_id 对应分类自己的 code_rule。
        // 删除/编辑只作用于此对象，防止误写入规则级模板后被「应用/保存」广播到所有勾选分类。
        private long? _editingOid = null;
        private int _ownerType = 0;
        private long _ownerObjectId = 0;
        private string _version = null;
        private bool _dirty = false;
        private bool _refreshing = false;
        // 左侧分类树：oid → 节点映射，替代原扁平 _clbClassIds
        private Dictionary<long, TreeNode> _nodeByOid = new Dictionary<long, TreeNode>();
        // 三态勾选常量（与 _stateImages 索引对应：0=未选 1=已选 2=半选）
        private const int ST_UNCHECKED = 0;
        private const int ST_CHECKED = 1;
        private const int ST_INDETERMINATE = 2;
        private ImageList _stateImages = null;
        private bool _suppressAfterSelect = false;   // 勾选点击时抑制 AfterSelect 误触发右侧重载
        private bool _suppressSelectAll = false;      // 程序化设置全选框状态时抑制其事件
        private bool _autoExample = true;             // 编码示例是否跟随合并属性段自动生成
        private bool _suppressExampleText = false;    // 程序化写入示例框文本时抑制 TextChanged 误判为用户编辑
        // 预选中（绿色）暂存集：仅记录用户经「全选 / 全选未勾选的分类」或手动勾选而选定的分类 oid，
        // 与「已落库成员」区分。绿色 = 预勾选且尚未保存；「保存到数据库」后清空。
        private HashSet<long> _preSelected = new HashSet<long>();
        // 已编辑但未保存的分类 oid 集合：用于左侧树对所有「改过但没存」的分类标绿（不只当前编辑项）。
        // 切换分类时（AfterSelect 会先 SyncEditorToEditingTarget 把界面写回内存），上一分类的改动已存入 _rules，
        // 其 oid 仍留在本集合 → 光标移开后依然显绿，便于一眼看出哪些还没存。
        private HashSet<long> _unsavedOids = new HashSet<long>();
        private static readonly Color PreSelectColor = Color.FromArgb(198, 239, 206);   // 浅绿，标识未保存（预选中新增成员 / 编辑未保存的分类）

        // 「编辑中」标记相关：左侧树中当前正在编辑的具体分类节点加粗 + 文本后缀，提示「只编辑保存此分类」
        private TreeNode _markedNode = null;        // 当前被标记节点，便于还原（重置 Text / 取消加粗）
        private Font _boldFont = null;             // 编辑中节点加粗字体（复用，避免反复 new 造成 GDI 泄漏）
        private bool _suppressCodeName = false;     // 程序化写入编码内部名时抑制 TextChanged 误判为用户编辑

        // 属性中英文映射（仅用于前端展示，后台存取一律使用英文字段名 InternalName）
        private Dictionary<string, MetaPropertyOption> _propByInternal = new Dictionary<string, MetaPropertyOption>();
        private Dictionary<string, string> _propDisplayToInternal = new Dictionary<string, string>();
        private Dictionary<string, string> _propInternalToDisplay = new Dictionary<string, string>();
        // 表单字段缓存：key=分类 object_id，避免切换分类时反复查库
        private Dictionary<long, List<FormTemplateField>> _formFieldCache = new Dictionary<long, List<FormTemplateField>>();

        // 码段“类型”列选项（前端显示中文，JSON 仍存英文）
        private List<TypeOption> _typeOptions = new List<TypeOption>();
        private Dictionary<string, string> _typeInternalToDisplay = new Dictionary<string, string>();

        /// <summary>类型下拉数据源项：Value = JSON 中保存的英文，Display = 前端显示中文。</summary>
        private class TypeOption
        {
            public string Value { get; set; }
            public string Display { get; set; }
        }

        /// <summary>属性名下拉数据源项：Value=英文属性名（入库用），Display=「内部名（中文）」。</summary>
        private class PropertyComboItem
        {
            public string Value { get; set; }
            public string Display { get; set; }
        }

        /// <summary>拖拽时缓存整行数据（含原始段 Tag），用于移动后重建该行。</summary>
        private class RowSnapshot
        {
            public object Tag;
            public object[] Cells;
        }

        private ToolTip _toolTip;

        // —— 合并属性段拖拽排序相关状态 ——
        private int _dragRowIndex = -1;          // 拖拽源行索引
        private int _dragOverRowIndex = -1;      // 当前悬停目标数据行（= -1 表示拖到末尾）
        private bool _dragInsertAfter = false;   // 插入到目标行之后(true)还是之前(false)
        private bool _dragging = false;          // 是否处于拖拽进行中
        private Point _dragStartPoint;
        private Rectangle _dragSourceRect;       // 源行显示矩形（高亮用）
        private const int DragThreshold = 4;     // 触发拖拽的鼠标移动阈值(像素)

        // —— 右键多选保护：右键按下瞬间(MouseDown，先于底层选择处理)捕获已选行，
        //    供 CellMouseDown 恢复/补全选择，防止默认右击行为把多选重置为单行 ——
        private List<int> _preRightClickSel = new List<int>();

        /// <summary>设计器专用无参构造：只做界面初始化，不访问数据库。</summary>
        public CodeLinkSettingForm()
        {
            InitializeTypeMapping();
            InitializeComponent();
        }

        public CodeLinkSettingForm(DbHelper db)
            : this()
        {
            _db = db;
            // 事件订阅放在构造函数里，避免被 VS 设计器重写的 InitializeComponent 抹掉
            WireUpEvents();
            InitToolTips();
            LoadData();
        }

        private void InitToolTips()
        {
            _toolTip = new ToolTip(this.components);
            _toolTip.SetToolTip(_btnApply, "将当前码段设置套用到所有「预选中(绿色)」的分类");
            _toolTip.SetToolTip(_btnRemoveClass, "取消当前选中分类的码段设置");
            _toolTip.SetToolTip(_btnSelectUnchecked, "把所有当前未勾选的分类置为勾选并标记为预选中(绿色)");
            _toolTip.SetToolTip(_chkSelectAll, "全选/取消全选：全部勾选并标记为预选中(绿色)");
            _toolTip.SetToolTip(_btnRegenExample, "按当前合并属性段重新生成编码示例；手动修改示例后会停止自动跟随，点此恢复");
        }

        private void WireUpEvents()
        {
            this._cmbProp.SelectedIndexChanged += _cmbProp_SelectedIndexChanged;
            this._cmbProp.TextUpdate += _cmbProp_TextUpdate;
            this._tvClass.AfterSelect += _tvClass_AfterSelect;
            this._tvClass.MouseDown += _tvClass_MouseDown;
            this._chkSelectAll.CheckStateChanged += _chkSelectAll_CheckStateChanged;
            this._btnSelectUnchecked.Click += _btnSelectUnchecked_Click;
            this.BuildStateImages();
            // 「编辑中」标记字体 & 状态条加粗（一次性创建，避免重复 new 造成 GDI 泄漏）
            this._boldFont = new Font(_tvClass.Font ?? SystemFonts.DefaultFont, FontStyle.Bold);
            this._lblStatus.Font = new Font(_lblStatus.Font ?? SystemFonts.DefaultFont, FontStyle.Bold);
            this._dgvMerge.CellFormatting += _dgvMerge_CellFormatting;
            this._dgvMerge.CellMouseDown += _dgvMerge_CellMouseDown;
            this._dgvMerge.MouseDown += _dgvMerge_PreRightClickCapture;   // 右键按下即捕获已选行快照（先于底层选择处理）
            this._dgvMerge.CellValueChanged += _dgvMerge_CellValueChanged;
            this._ctxMerge.Opening += _ctxMerge_Opening;
            this._ctxDelete.Click += _ctxDelete_Click;
            this._btnApply.Click += _btnApply_Click;
            this._btnRemoveClass.Click += _btnRemoveClass_Click;
            this._btnLoad.Click += _btnLoad_Click;
            this._btnSave.Click += _btnSave_Click;
            this._btnClose.Click += _btnClose_Click;
            this._btnCopy.Click += _btnCopy_Click;
            this._txtCodeExample.TextChanged += _txtCodeExample_TextChanged;
            this._txtCodeName.TextChanged += _txtCodeName_TextChanged;
            this._btnRegenExample.Click += _btnRegenExample_Click;
            this._dgvMerge.RowsAdded += _dgvMerge_RowsChanged;
            this._dgvMerge.RowsRemoved += _dgvMerge_RowsChanged;
            // —— 合并属性段拖拽排序：仅允许从首列「排序」手柄拖拽，避免与单元格编辑冲突 ——
            this._dgvMerge.MouseDown += _dgvMerge_DragMouseDown;
            this._dgvMerge.MouseMove += _dgvMerge_DragMouseMove;
            this._dgvMerge.MouseUp += _dgvMerge_DragMouseUp;
            this._dgvMerge.DragOver += _dgvMerge_DragOver;
            this._dgvMerge.DragDrop += _dgvMerge_DragDrop;
            this._dgvMerge.Paint += _dgvMerge_Paint;
            this._dgvMerge.QueryContinueDrag += _dgvMerge_QueryContinueDrag;
            this._dgvMerge.AllowDrop = true;
            if (!this._dgvMerge.Columns.Contains("drag_handle"))
            {
                var handleCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
                handleCol.Name = "drag_handle";
                handleCol.HeaderText = "排序";
                handleCol.ReadOnly = true;
                handleCol.Width = 44;
                handleCol.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                handleCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                this._dgvMerge.Columns.Insert(0, handleCol);
            }
            this._lblMerge.Text = "合并属性段（merge_properties，按段顺序拼接，按住首列「排序」拖拽可调整顺序）：";
            this._dgvMerge.ContextMenuStrip = this._ctxMerge;
        }

        private void InitializeTypeMapping()
        {
            _typeInternalToDisplay["fixed"] = "取固定值";
            _typeInternalToDisplay["property_number"] = "取属性编号";
            _typeInternalToDisplay["property_num"] = "取属性编号";
            _typeInternalToDisplay["property_value"] = "取属性值";
            _typeInternalToDisplay["property_val"] = "取属性值";
            _typeInternalToDisplay["property"] = "取属性值";
            _typeInternalToDisplay["serial_code"] = "取流水码";
            _typeInternalToDisplay["serial_node"] = "取流水码";
        }

        /// <summary>根据当前规则中实际出现的 type 值，构建 ComboBox 数据源（已知英文→中文，未知英文→原样）。</summary>
        private void BuildTypeDataSource(IEnumerable<string> extraTypes)
        {
            var list = new List<TypeOption>();
            AddTypeOption(list, "fixed", "取固定值");
            AddTypeOption(list, "property_number", "取属性编号");
            AddTypeOption(list, "property_value", "取属性值");
            AddTypeOption(list, "serial_code", "取流水码");
            foreach (var t in extraTypes)
            {
                if (string.IsNullOrWhiteSpace(t)) continue;
                string std = NormalizeType(t);
                string display = _typeInternalToDisplay.ContainsKey(std) ? _typeInternalToDisplay[std] : std;
                AddTypeOption(list, std, display);
            }
            _typeOptions = list;
            this.type.DataSource = _typeOptions;
            this.type.DisplayMember = "Display";
            this.type.ValueMember = "Value";
        }

        /// <summary>把历史/截图中见到的别名统一规范化为标准 type 值，方便下拉显示与保存。</summary>
        private string NormalizeType(string type)
        {
            switch (type)
            {
                case "property_num": return "property_number";
                case "property_val":
                case "property": return "property_value";
                case "serial_node": return "serial_code";
                default: return type;
            }
        }

        private void AddTypeOption(List<TypeOption> list, string value, string display)
        {
            if (!list.Exists(x => x.Value == value))
                list.Add(new TypeOption { Value = value, Display = display });
        }

        /// <summary>按当前编辑对象所属分类重建「属性名」下拉数据源：
        /// oid 有值→取该分类表单字段(GetFormTemplateFieldsByClassification)；oid 为空(属性级模板)→取系统全部属性兜底。
        /// 并总把各规则段已存的 property_internal_name 补入，兼容库中已存但不在候选池的特殊属性。</summary>
        private void BuildPropertyDataSource(long? oid)
        {
            var list = new List<PropertyComboItem>();
            if (oid.HasValue)
            {
                if (!_formFieldCache.TryGetValue(oid.Value, out List<FormTemplateField> fields))
                {
                    fields = _db.GetFormTemplateFieldsByClassification(oid.Value) ?? new List<FormTemplateField>();
                    _formFieldCache[oid.Value] = fields;
                }
                foreach (var f in fields)
                {
                    if (string.IsNullOrWhiteSpace(f.Property)) continue;
                    if (list.Exists(x => x.Value == f.Property)) continue;
                    string disp = f.Property;
                    if (_propByInternal.TryGetValue(f.Property, out MetaPropertyOption opt) && !string.IsNullOrWhiteSpace(opt.DisplayName))
                        disp = f.Property + "（" + opt.DisplayName + "）";
                    list.Add(new PropertyComboItem { Value = f.Property, Display = disp });
                }
            }
            else
            {
                foreach (var p in _allProps)
                {
                    string disp = string.IsNullOrWhiteSpace(p.DisplayName)
                        ? p.InternalName : p.InternalName + "（" + p.DisplayName + "）";
                    list.Add(new PropertyComboItem { Value = p.InternalName, Display = disp });
                }
            }
            // 兜底：把各规则段已存的属性名补入（库里已存但不在候选池的特殊属性）
            if (_rules != null)
            {
                foreach (JObject r in _rules)
                {
                    CollectPropsFromCodeRule(r["code_rule"] as JObject, list);
                    if (r["object_classification"] is JArray oc)
                        foreach (var it in oc)
                            CollectPropsFromCodeRule(it["code_rule"] as JObject, list);
                }
            }
            this.property_internal_name.DisplayMember = "Display";
            this.property_internal_name.ValueMember = "Value";
            this.property_internal_name.DataSource = list;
        }

        private void CollectPropsFromCodeRule(JObject cr, List<PropertyComboItem> list)
        {
            if (cr == null) return;
            var mp = cr["merge_properties"] as JArray;
            if (mp == null) return;
            foreach (var seg in mp)
            {
                string nm = seg["property_internal_name"] != null ? (string)seg["property_internal_name"] : null;
                if (!string.IsNullOrWhiteSpace(nm) && !list.Exists(x => x.Value == nm))
                    list.Add(new PropertyComboItem { Value = nm, Display = nm });
            }
        }

        /// <summary>按当前行 type 决定「属性名下拉」「固定值」两列可用性：
        /// type=property_number/property_value→启用属性名下拉；type=fixed→启用固定值；其余禁用并清空，避免脏数据。</summary>
        private void UpdateRowEditable(DataGridViewRow row)
        {
            if (row == null || row.IsNewRow) return;
            string t = GetCell(row, "type");
            bool isProp = (t == "property_number" || t == "property_value");
            bool isFixed = (t == "fixed");
            row.Cells["property_internal_name"].ReadOnly = !isProp;
            row.Cells["value"].ReadOnly = !isFixed;
            if (!isProp && !string.IsNullOrEmpty(GetCell(row, "property_internal_name")))
                SetCell(row, "property_internal_name", string.Empty);
            if (!isFixed && !string.IsNullOrEmpty(GetCell(row, "value")))
                SetCell(row, "value", string.Empty);
        }

        private void LoadData()
        {
            try
            {
                _classifications = _db.GetAllClassifications() ?? new List<ClassificationGroup>();
                _allProps = _db.GetMetaPropertyOptions() ?? new List<MetaPropertyOption>();

                _refreshing = true;
                _cmbProp.Items.Clear();
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
                    _cmbProp.Items.Add(disp);
                }

                BuildTree();

                var rec = _db.GetCodeLinkSettingRecord();
                if (rec == null || string.IsNullOrWhiteSpace(rec.Content))
                {
                    _root = new JObject();
                    _root["property_to_code_rules"] = new JArray();
                    _ownerType = 0; _ownerObjectId = 0; _version = null;
                    _lblStatus.Text = "未找到领码设置，将以空配置开始";
                }
                else
                {
                    _root = JObject.Parse(rec.Content);
                    _ownerType = rec.OwnerType;
                    _ownerObjectId = rec.OwnerObjectId;
                    _version = rec.Version;
                    _lblStatus.Text = string.Empty;
                }
                if (_root["property_to_code_rules"] == null)
                    _root["property_to_code_rules"] = new JArray();
                _rules = _root["property_to_code_rules"] as JArray;
                NormalizeRulesForEditing();   // 系统单块(无 target_property)拆成按属性分块，便于本工具按 property 编辑

                int selIdx = -1;
                if (_rules.Count > 0)
                {
                    string firstProp = (string)_rules[0]["target_property"];
                    // 兜底：库中已有属性若不在候选列表（如特殊属性），补足仅显示内部名的一项，保证正确选中
                    if (!_propInternalToDisplay.ContainsKey(firstProp))
                    {
                        _propInternalToDisplay[firstProp] = firstProp;
                        _propDisplayToInternal[firstProp] = firstProp;
                        _cmbProp.Items.Add(firstProp);
                    }
                    string firstDisp = _propInternalToDisplay[firstProp];
                    selIdx = _cmbProp.Items.IndexOf(firstDisp);
                }
                if (selIdx < 0 && _cmbProp.Items.Count > 0) selIdx = 0;
                if (selIdx >= 0) _cmbProp.SelectedIndex = selIdx;
                else _cmbProp.Text = string.Empty;
                _refreshing = false;

                LoadTargetProperty(CurrentPropInternalName());
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTargetProperty(string prop)
        {
            _currentProp = prop ?? string.Empty;
            _currentRule = null;
            _editingOid = null;
            if (!string.IsNullOrEmpty(_currentProp))
            {
                foreach (JObject r in _rules)
                {
                    if ((string)r["target_property"] == _currentProp) { _currentRule = r; break; }
                }
            }
            _refreshing = true;
            // by_rule 规则：右侧模板区展示首个分类的编码规则作「克隆参考」（规则级 code_rule 含 code_rule_segments，不适合在合并属性网格编辑）
            JObject tmplToShow = null;
            if (_currentRule != null)
            {
                bool isByRule = (string)_currentRule["type"] == "by_rule";
                if (isByRule)
                {
                    var oc = _currentRule["object_classification"] as JArray;
                    if (oc != null && oc.Count > 0 && oc[0]["code_rule"] != null)
                        tmplToShow = oc[0]["code_rule"] as JObject;
                }
                else if (_currentRule["code_rule"] != null)
                    tmplToShow = _currentRule["code_rule"] as JObject;
            }
            BuildPropertyDataSource(_editingOid);
            LoadTemplateToUI(tmplToShow);
            RecalcTreeStatesFromRule();
            _refreshing = false;
            // 切换目标属性后不默认选中/编辑任何分类：右侧停在属性级模板供参考，
            // 用户在左侧树中主动点选某个分类后才进入该分类的编辑（_editingOid 保持 null）。
            _refreshing = true;
            _tvClass.SelectedNode = null;
            _refreshing = false;
            UpdateStatus();
        }

        private void _cmbProp_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_refreshing) return;
            string p = CurrentPropInternalName();
            if (p == _currentProp) return;
            // 切换属性前：把当前界面写回当前编辑对象并同步分类成员，但不广播到所有分类
            SyncEditorToEditingTarget();
            ApplyClassificationChanges(false);
            LoadTargetProperty(p);
        }

        /// <summary>从下拉框解析出目标属性的英文字段名（InternalName）。
        /// 下拉显示「内部名（中文）」，需反查映射；用户手输新属性（无中文）时原样返回。后台逻辑只认英文字段名。</summary>
        private string CurrentPropInternalName()
        {
            string t = _cmbProp.Text.Trim();
            if (_propDisplayToInternal.TryGetValue(t, out string internalName))
                return internalName;
            return t;
        }

        /// <summary>用户在下拉框输入时实时过滤目标属性列表（按显示名「内部名（中文）」不区分大小写模糊匹配）。
        /// 仅影响界面展示，不改动 _allProps / 字典 / 任何后台数据。</summary>
        private void _cmbProp_TextUpdate(object sender, EventArgs e)
        {
            if (_refreshing) return;
            string t = _cmbProp.Text;
            FilterPropItems(t);
            // 保持下拉展开，方便用户从过滤结果中点选
            if (_cmbProp.Items.Count > 0 && !_cmbProp.DroppedDown)
                _cmbProp.DroppedDown = true;
        }

        private void FilterPropItems(string filter)
        {
            _refreshing = true;
            _cmbProp.Items.Clear();
            bool empty = string.IsNullOrEmpty(filter);
            string f = empty ? null : filter.ToLowerInvariant();
            // 先按候选属性顺序填充（顺序稳定，与初始加载一致）
            foreach (var p in _allProps)
            {
                string disp = _propInternalToDisplay.ContainsKey(p.InternalName)
                    ? _propInternalToDisplay[p.InternalName] : p.InternalName;
                if (empty || disp.ToLowerInvariant().Contains(f))
                    _cmbProp.Items.Add(disp);
            }
            // 再补「库中已有但不在候选列表」的特殊属性（显示名在字典中但无对应 MetaPropertyOption）
            foreach (var kv in _propInternalToDisplay)
            {
                if (_propByInternal.ContainsKey(kv.Key)) continue; // 已通过 _allProps 加入
                if (empty || kv.Value.ToLowerInvariant().Contains(f))
                    _cmbProp.Items.Add(kv.Value);
            }
            // 还原用户输入文本与光标，避免清空 Items 时被重置
            _cmbProp.Text = filter;
            _cmbProp.SelectionStart = filter == null ? 0 : filter.Length;
            _cmbProp.SelectionLength = 0;
            _refreshing = false;
        }

        // ===================== 左侧分类树（TreeView 三态勾选 + 全选） =====================

        /// <summary>选中某分类节点时：先把上一编辑对象的界面改动写回，再加载该分类码段。</summary>
        private void _tvClass_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 勾选点击已置 _suppressAfterSelect 防止误重载；_refreshing 期间（程序化选中）也不重载
            if (_refreshing || _suppressAfterSelect) { _suppressAfterSelect = false; return; }
            SyncEditorToEditingTarget();
            long oid = (e.Node.Tag is long) ? (long)e.Node.Tag : 0;
            if (oid > 0) SelectClassificationForEditing(oid);
        }

        /// <summary>左键在「状态图标」区域（标签左侧、加号/减号右侧）点击 → 切换勾选并级联子节点；
        /// 点击标签区域由 TreeView 默认选中并触发 AfterSelect。加号/减号区域不处理（留给展开/折叠）。</summary>
        private void _tvClass_MouseDown(object sender, MouseEventArgs e)
        {
            _suppressAfterSelect = false;
            if (_refreshing) return;
            var hit = _tvClass.HitTest(e.X, e.Y);
            if (hit.Node == null) return;
            int imgW = (_tvClass.StateImageList != null) ? _tvClass.StateImageList.ImageSize.Width : 16;
            int stateLeft = hit.Node.Bounds.Left - imgW - 3;   // 状态图标左边界
            if (e.X >= stateLeft && e.X < hit.Node.Bounds.Left)
            {
                int cur = NodeState(hit.Node);
                int newState = (cur == ST_CHECKED) ? ST_UNCHECKED : ST_CHECKED;
                SetNodeAndDescendants(hit.Node, newState);   // 勾选父级级联全部子级
                ApplyPreSelectedToSubtree(hit.Node, newState == ST_CHECKED);   // 同步绿色暂存集
                UpdateAncestors(hit.Node);                    // 重算父级半选
                UpdateSelectAllState();                       // 刷新全选框三态
                RefreshUncommittedColors();                   // 刷新绿色高亮
                _dirty = true;
                UpdateStatus();
                _suppressAfterSelect = true;                 // 仅切换勾选，不重载右侧
                return;
            }
        }

        /// <summary>全选 / 取消全选：三态复选框。Checked=全部勾选并全部纳入「预选中(绿色)」；Unchecked=全部取消并清空绿色集；Indeterminate=部分（仅显示，不来自点击）。</summary>
        private void _chkSelectAll_CheckStateChanged(object sender, EventArgs e)
        {
            if (_suppressSelectAll || _refreshing) return;
            bool select = (_chkSelectAll.CheckState == CheckState.Checked);
            foreach (TreeNode root in _tvClass.Nodes)
            {
                SetNodeAndDescendants(root, select ? ST_CHECKED : ST_UNCHECKED);
                ApplyPreSelectedToSubtree(root, select);
            }
            UpdateSelectAllState();
            RefreshUncommittedColors();
            _dirty = true;
            UpdateStatus();
        }

        /// <summary>全选未勾选的分类：仅把当前「未勾选(ST_UNCHECKED)」的节点置为勾选并纳入「预选中(绿色)」集；
        /// 不级联打扰已有勾选/半选父节点。处理完后自底向上重算父节点显示态（半选/已选）。</summary>
        private void _btnSelectUnchecked_Click(object sender, EventArgs e)
        {
            if (_refreshing) return;
            _refreshing = true;
            foreach (var kv in _nodeByOid)
            {
                TreeNode n = kv.Value;
                if (NodeState(n) == ST_UNCHECKED)
                {
                    SetNodeState(n, ST_CHECKED);
                    _preSelected.Add(kv.Key);
                }
            }
            // 自底向上重算父节点显示态，使半选父节点在子节点全部勾选后转为已选
            foreach (TreeNode root in _tvClass.Nodes) RecalcParentCheckStatesRecursive(root);
            UpdateSelectAllState();
            RefreshUncommittedColors();
            _refreshing = false;
            _dirty = true;
            UpdateStatus();
        }

        /// <summary>把某子树整体加入/移出「预选中」集（oid 来自节点 Tag）。</summary>
        private void ApplyPreSelectedToSubtree(TreeNode root, bool pre)
        {
            if (root.Tag is long oid)
            {
                if (pre) _preSelected.Add(oid); else _preSelected.Remove(oid);
            }
            foreach (TreeNode c in root.Nodes) ApplyPreSelectedToSubtree(c, pre);
        }

        /// <summary>刷新左侧树「未保存(绿色)」背景色。绿色代表两类尚未落库状态：
        /// ① 预选中（新增成员待保存）；② 已编辑但未保存的分类（_unsavedOids，含已切走、光标不再停留的那些）。
        /// 任一满足即标绿，使「编辑完成但光标已移开」的分类依然可见，便于一眼看出哪些还没存。</summary>
        private void RefreshUncommittedColors()
        {
            foreach (var kv in _nodeByOid)
            {
                long oid = kv.Key;
                bool uncommitted = _preSelected.Contains(oid) || _unsavedOids.Contains(oid);
                kv.Value.BackColor = uncommitted ? PreSelectColor : Color.Empty;
            }
        }

        /// <summary>标记当前编辑对象(具体分类)存在未保存改动：置全局 _dirty，并把该分类 oid 记入 _unsavedOids，
        /// 使左侧树对其标绿——即使随后切到别的分类、光标移开，它仍显绿。编辑属性级模板(_editingOid 为空)时不记具体分类。</summary>
        private void MarkDirtyForCurrent()
        {
            _dirty = true;
            if (_editingOid.HasValue) _unsavedOids.Add(_editingOid.Value);
            UpdateStatus();
        }

        /// <summary>自底向上重算每个有子节点的父节点显示态（基于当前子节点勾选态聚合）。</summary>
        private void RecalcParentCheckStatesRecursive(TreeNode n)
        {
            foreach (TreeNode c in n.Nodes) RecalcParentCheckStatesRecursive(c);
            if (n.Nodes.Count > 0)
                SetNodeState(n, AggregateChildState(n));
        }

        /// <summary>按 PARENT_ID 自引用层级构建分类树；父不在清单内（含 0）视为根。无 PARENT_ID 时退化为单层平铺。</summary>
        private void BuildTree()
        {
            _tvClass.Nodes.Clear();
            _nodeByOid.Clear();
            if (_classifications == null || _classifications.Count == 0) return;
            var ids = new HashSet<long>();
            foreach (var c in _classifications) ids.Add(c.ObjectId);
            var byParent = new Dictionary<long, List<ClassificationGroup>>();
            foreach (var c in _classifications)
            {
                long pid = (c.ParentId != 0 && ids.Contains(c.ParentId)) ? c.ParentId : 0;
                if (!byParent.ContainsKey(pid)) byParent[pid] = new List<ClassificationGroup>();
                byParent[pid].Add(c);
            }
            if (byParent.ContainsKey(0))
                foreach (var r in byParent[0]) AddTreeNodes(null, r, byParent);
            UpdateSelectAllState();
            RefreshUncommittedColors();   // 加载数据后复位绿色高亮（_preSelected 在构造时为空）
        }

        private void AddTreeNodes(TreeNode parent, ClassificationGroup c, Dictionary<long, List<ClassificationGroup>> byParent)
        {
            TreeNode node = (parent == null) ? _tvClass.Nodes.Add(c.Name) : parent.Nodes.Add(c.Name);
            node.Tag = c.ObjectId;
            node.Name = c.Name;   // 存原名，供「编辑中」标记还原 Text 之用（与显示的 Text 解耦）
            node.StateImageIndex = ST_UNCHECKED;
            _nodeByOid[c.ObjectId] = node;
            if (byParent.ContainsKey(c.ObjectId))
                foreach (var child in byParent[c.ObjectId]) AddTreeNodes(node, child, byParent);
        }

        /// <summary>运行时生成三态 StateImageList（未选/已选/半选），避免 Designer 嵌入位图的兼容问题。</summary>
        private void BuildStateImages()
        {
            _stateImages = new ImageList();
            _stateImages.ImageSize = new Size(16, 16);
            _stateImages.Images.Add(DrawStateBitmap(ST_UNCHECKED));
            _stateImages.Images.Add(DrawStateBitmap(ST_CHECKED));
            _stateImages.Images.Add(DrawStateBitmap(ST_INDETERMINATE));
            _tvClass.StateImageList = _stateImages;
        }

        private Bitmap DrawStateBitmap(int state)
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.DrawRectangle(Pens.Gray, 2, 2, 11, 11);
                if (state == ST_CHECKED)
                {
                    g.FillRectangle(Brushes.White, 3, 3, 10, 10);
                    g.DrawLine(Pens.Black, 4, 8, 6, 10);
                    g.DrawLine(Pens.Black, 6, 10, 11, 4);
                }
                else if (state == ST_INDETERMINATE)
                {
                    g.FillRectangle(Brushes.Gray, 4, 4, 8, 8);
                }
                else
                {
                    g.FillRectangle(Brushes.White, 3, 3, 10, 10);
                }
            }
            return bmp;
        }

        private int NodeState(TreeNode n)
        {
            return (n.StateImageIndex < 0) ? ST_UNCHECKED : n.StateImageIndex;
        }

        private void SetNodeState(TreeNode n, int state)
        {
            if (n.StateImageIndex != state) n.StateImageIndex = state;
        }

        private void SetNodeAndDescendants(TreeNode n, int state)
        {
            SetNodeState(n, state);
            foreach (TreeNode c in n.Nodes) SetNodeAndDescendants(c, state);
        }

        /// <summary>自底向上重算祖先节点显示态：子全选→已选；子全不选→未选；混合→半选。</summary>
        private void UpdateAncestors(TreeNode n)
        {
            TreeNode p = n.Parent;
            while (p != null)
            {
                SetNodeState(p, AggregateChildState(p));
                p = p.Parent;
            }
        }

        /// <summary>聚合某节点的子节点状态：全选→已选；全不选→未选；混合/半选→半选。</summary>
        private int AggregateChildState(TreeNode p)
        {
            bool anyChecked = false, allChecked = true, anyIndet = false;
            foreach (TreeNode c in p.Nodes)
            {
                int s = NodeState(c);
                if (s == ST_CHECKED) anyChecked = true; else allChecked = false;
                if (s == ST_INDETERMINATE) anyIndet = true;
            }
            if (anyIndet || (anyChecked && !allChecked)) return ST_INDETERMINATE;
            if (allChecked) return ST_CHECKED;
            return ST_UNCHECKED;
        }

        /// <summary>根据当前属性规则成员，刷新整棵树的勾选态：先按各分类自身是否在规则中置态，
        /// 再自底向上聚合父节点显示（父自身在规则中则保持已选，否则按子节点聚合，体现半选）。</summary>
        private void RecalcTreeStatesFromRule()
        {
            foreach (var kv in _nodeByOid)
                SetNodeState(kv.Value, IsClassInCurrentRule(kv.Key) ? ST_CHECKED : ST_UNCHECKED);
            RecalcNode(_tvClass.Nodes);
            UpdateSelectAllState();
        }

        private void RecalcNode(TreeNodeCollection nodes)
        {
            foreach (TreeNode n in nodes)
            {
                if (n.Nodes.Count > 0) RecalcNode(n.Nodes);
                // 父节点自身不在规则中时，按子节点聚合显示（支持半选）；自身在规则中则保持已选
                if (n.Nodes.Count > 0 && !IsClassInCurrentRule((long)n.Tag))
                    SetNodeState(n, AggregateChildState(n));
            }
        }

        /// <summary>刷新「全选」框三态：全部勾选→Checked；全未选→Unchecked；存在部分勾选→Indeterminate。</summary>
        private void UpdateSelectAllState()
        {
            bool anyChecked = false, allChecked = true;
            CollectStates(_tvClass.Nodes, ref anyChecked, ref allChecked);
            _suppressSelectAll = true;
            _chkSelectAll.CheckState = (allChecked && anyChecked) ? CheckState.Checked
                : (anyChecked ? CheckState.Indeterminate : CheckState.Unchecked);
            _suppressSelectAll = false;
        }

        private void CollectStates(TreeNodeCollection nodes, ref bool anyChecked, ref bool allChecked)
        {
            foreach (TreeNode n in nodes)
            {
                int s = NodeState(n);
                if (s == ST_CHECKED) anyChecked = true; else allChecked = false;
                if (n.Nodes.Count > 0) CollectStates(n.Nodes, ref anyChecked, ref allChecked);
            }
        }

        /// <summary>根据左侧选中的分类 oid，在右侧加载该分类的码段模板。
        /// 判断依据为「该分类是否属于当前属性规则成员（object_classification 包含其 object_id）」而非勾选态；
        /// 仅关联分类才编辑其自带 code_rule，其余一律编辑规则级模板。分类自带 code_rule 为空时回退显示规则级模板作参考。
        /// 特别地：勾选了但尚未纳入规则的新分类，会被立即建成一条独立的 code_rule 并锁定为当前编辑对象，
        /// 使其规则与「属性级模板」完全分离，避免误改属性本身的规则。</summary>
        private void SelectClassificationForEditing(long oid)
        {
            if (oid <= 0)
            {
                // 未选中有效分类：编辑「属性级模板」（通用模板），不影响任何具体分类
                _editingOid = null;
                JObject tmpl = (_currentRule != null && _currentRule["code_rule"] != null)
                    ? _currentRule["code_rule"] as JObject : null;
                BuildPropertyDataSource(_editingOid);
                LoadTemplateToUI(tmpl);
                UpdateStatus();   // 刷新「编辑中」标记（此处 _editingOid 为 null → 清除具体分类标记）
                return;
            }
            bool inRule = false;
            JObject classRule = null;
            if (_currentRule != null && _currentRule["object_classification"] != null)
            {
                foreach (var it in (JArray)_currentRule["object_classification"])
                {
                    if ((long)it["object_id"] == oid) { inRule = true; classRule = it as JObject; break; }
                }
            }
            if (!inRule)
            {
                if (IsChecked(oid))
                {
                    // 已被勾选但尚未纳入规则：说明用户想为「这个分类」单独设规则。
                    // 立即在 object_classification 中为其新建一条独立的 code_rule（初始克隆自属性模板），
                    // 并把右侧编辑对象锁定到该分类本身，避免误写进属性级模板污染其它分类。
                    var ocArr = _currentRule["object_classification"] as JArray;
                    if (ocArr == null) { ocArr = new JArray(); _currentRule["object_classification"] = ocArr; }
                    var entry = new JObject();
                    entry["object_id"] = oid;
                    string name = string.Empty;
                    foreach (var c in _classifications) if (c.ObjectId == oid) { name = c.Name; break; }
                    entry["name"] = name;
                    entry["type"] = "merge_property";
                    JObject seed = _currentRule["code_rule"] as JObject;
                    entry["code_rule"] = seed != null ? (JObject)seed.DeepClone() : new JObject();
                    ocArr.Add(entry);
                    _unsavedOids.Add(oid);
                    _dirty = true;
                    _editingOid = oid;
                    BuildPropertyDataSource(_editingOid);
                    LoadTemplateToUI(entry["code_rule"] as JObject);
                    UpdateStatus();
                    return;
                }
                // 既未勾选、也不在规则中：编辑属性级模板（通用模板），用于后续「应用」到勾选分类
                _editingOid = null;
                JObject tmpl = (_currentRule != null && _currentRule["code_rule"] != null)
                    ? _currentRule["code_rule"] as JObject : null;
                BuildPropertyDataSource(_editingOid);
                LoadTemplateToUI(tmpl);
                UpdateStatus();   // 刷新「编辑中」标记（_editingOid 为 null → 清除具体分类标记）
                return;
            }
            _editingOid = oid;   // 锁定当前编辑对象为该分类，删除/编辑只影响它
            JObject cr = (classRule != null && classRule["code_rule"] != null) ? classRule["code_rule"] as JObject : null;
            if (cr == null && _currentRule != null && _currentRule["code_rule"] != null)
                cr = _currentRule["code_rule"] as JObject;   // 分类无自带码段时回退规则级模板
            BuildPropertyDataSource(_editingOid);
            LoadTemplateToUI(cr);
            UpdateStatus();   // 刷新「编辑中」标记（此时 _editingOid 锁定为该分类 → 加粗并标注）
        }

        private void _btnApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentProp))
            {
                MessageBox.Show("请先选择或输入目标属性。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_preSelected.Count == 0)
            {
                MessageBox.Show("请先通过「全选」或「全选未勾选的分类」预选分类（绿色），再应用码段。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 「应用」只把当前界面作为模板套用到「预选中(绿色)」分类，绝不写回属性级规则(_currentRule["code_rule"])，
            // 否则会污染「此分类所属属性的规则」（bug：编辑属性级模板时点应用会把界面内容写回属性规则）。
            // 模板直接来自界面 BuildTemplateFromUI()，无需先把界面固化到当前编辑对象（切换属性/保存时另有 Sync 兜底）。
            var template = BuildTemplateFromUI();
            ApplyCodeRuleToPreSelected(template);
            _dirty = true;
            foreach (long oid in _preSelected) _unsavedOids.Add(oid);   // 套用后这些预选分类内容已改，标绿
            UpdateStatus();
            MessageBox.Show("已将当前码段设置套用到 " + _preSelected.Count + " 个预选分类（绿色）。记得点「保存到数据库」。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>把给定码段模板套用到所有「预选中(绿色)」分类：确保它们成为当前属性规则成员并写入 code_rule。
        /// 仅写入内存中的 _currentRule["object_classification"]，不触库；由「保存到数据库」统一持久化。绿色(预选中)状态保持，直到保存。</summary>
        private void ApplyCodeRuleToPreSelected(JObject template)
        {
            if (string.IsNullOrWhiteSpace(_currentProp)) return;
            if (_currentRule == null)
            {
                _currentRule = new JObject();
                _currentRule["target_property"] = _currentProp;
                _currentRule["type"] = "merge_property";
                _currentRule["object_classification"] = new JArray();
                _currentRule["code_rule"] = new JObject();
                _rules.Add(_currentRule);
            }
            var ocArr = _currentRule["object_classification"] as JArray;
            if (ocArr == null) { ocArr = new JArray(); _currentRule["object_classification"] = ocArr; }
            foreach (long oid in _preSelected)
            {
                // 兜底：确保勾选态（绿色节点必然是勾选的）
                if (_nodeByOid.TryGetValue(oid, out TreeNode n)) SetNodeAndDescendants(n, ST_CHECKED);
                JObject existing = null;
                foreach (var it in ocArr)
                    if ((long)it["object_id"] == oid) { existing = it as JObject; break; }
                if (existing == null)
                {
                    existing = new JObject();
                    existing["object_id"] = oid;
                    string name = string.Empty;
                    foreach (var c in _classifications) if (c.ObjectId == oid) { name = c.Name; break; }
                    existing["name"] = name;
                    existing["type"] = "merge_property";
                    existing["code_rule"] = template.DeepClone();
                    ocArr.Add(existing);
                }
                else
                {
                    existing["code_rule"] = template.DeepClone();
                }
            }
            UpdateSelectAllState();
            RefreshUncommittedColors();
        }

        /// <summary>把当前界面写入 _currentRule。applyTemplateToExisting=true 时（「应用」按钮）会把规则级模板广播到所有勾选分类；
        /// =false 时（「保存」/切换属性）仅同步分类成员与新建分类，已存在分类的 code_rule 保持原样（不广播，避免局部编辑误伤其它分类）。</summary>
        private void ApplyCurrent(bool applyTemplateToExisting = true)
        {
            if (string.IsNullOrWhiteSpace(_currentProp)) return;
            bool isByRule = _currentRule != null && (string)_currentRule["type"] == "by_rule";
            if (_currentRule == null)
            {
                _currentRule = new JObject();
                _currentRule["target_property"] = _currentProp;
                _currentRule["type"] = "merge_property";
                _currentRule["object_classification"] = new JArray();
                _currentRule["code_rule"] = new JObject();
                _rules.Add(_currentRule);
            }
            if (!isByRule && _editingOid == null)
            {
                // 仅当「当前正在编辑属性级模板」(_editingOid 为空) 时，才把界面内容覆盖到属性模板；
                // 若当前编辑的是某个具体分类(_editingOid 有值)，界面内容只属于该分类，绝不能写回属性模板，
                // 否则会出现「给某分类设规则、属性本身的规则也被改掉」的问题。
                var template = BuildTemplateFromUI();
                _currentRule["code_rule"] = template.DeepClone();
            }
            // 无论 by_rule 还是 merge_property，都按勾选状态更新分类成员（by_rule 仅改成员、不动各分类自带 code_rule）
            ApplyClassificationChanges(applyTemplateToExisting);
        }

        /// <summary>按勾选状态更新 object_classification：
        /// ① 本工具已知分类：仅当勾选时保留；② 不在清单内的分类（如后续新增）一律保留，防误删；
        /// ③ 勾选且缺失的分类新增并套用模板。
        /// applyTemplateToExisting=true（「应用」）时把界面模板广播到所有已存在勾选分类；
        /// =false（「保存」/切换属性）时仅同步成员与新建分类，已存在分类的 code_rule 保持原样（不广播，避免误伤其它分类）。</summary>
        private void ApplyClassificationChanges(bool applyTemplateToExisting = true)
        {
            bool isByRule = _currentRule != null && (string)_currentRule["type"] == "by_rule";
            var ocArr = _currentRule["object_classification"] as JArray;
            if (ocArr == null) { ocArr = new JArray(); _currentRule["object_classification"] = ocArr; }
            var template = BuildTemplateFromUI();
            var keep = new JArray();
            foreach (var it in ocArr)
            {
                long oid = (long)it["object_id"];
                if (_nodeByOid.ContainsKey(oid))
                {
                    if (IsChecked(oid)) keep.Add(it);   // 已知分类：勾选才保留
                }
                else
                {
                    keep.Add(it);   // 未知分类：保留，避免误删既有配置
                }
            }
            foreach (var c in _classifications)
            {
                if (IsChecked(c.ObjectId))
                {
                    JObject existing = null;
                    foreach (var it in keep)
                        if ((long)it["object_id"] == c.ObjectId) { existing = it as JObject; break; }
                    if (existing == null)
                    {
                        existing = new JObject();
                        existing["object_id"] = c.ObjectId;
                        existing["name"] = c.Name;
                        existing["type"] = "merge_property";
                        // by_rule 规则下，新增分类的 code_rule 从同规则已有分类克隆（结构一致、系统可识别）；
                        // merge_property 则用界面模板
                        existing["code_rule"] = isByRule ? CloneSiblingCodeRule(ocArr) : template.DeepClone();
                        keep.Add(existing);
                    }
                    else if (!isByRule && applyTemplateToExisting)
                    {
                        // 仅「应用」时把界面模板广播到已存在分类；「保存」时保持各分类原有 code_rule
                        existing["code_rule"] = template.DeepClone();
                    }
                }
            }
            _currentRule["object_classification"] = keep;
        }

        /// <summary>从同规则已有分类克隆一份 code_rule 作为新增分类的缺省（by_rule 场景下界面模板不适用）。无可克隆样本时返回最小有效结构。</summary>
        private JObject CloneSiblingCodeRule(JArray ocArr)
        {
            if (ocArr != null)
            {
                foreach (var it in ocArr)
                {
                    var cr = it["code_rule"] as JObject;
                    if (cr != null) return (JObject)cr.DeepClone();
                }
            }
            var def = new JObject();
            def["code_example"] = string.Empty;
            def["code_internal_name"] = "item_code";
            var mp = new JArray();
            var seg = new JObject();
            seg["type"] = "fixed";
            seg["value"] = string.Empty;
            mp.Add(seg);
            def["merge_properties"] = mp;
            return def;
        }

        private void _btnRemoveClass_Click(object sender, EventArgs e)
        {
            TreeNode node = _tvClass.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("请先在左侧选中一个分类。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            long oid = (long)node.Tag;
            SetNodeState(node, ST_UNCHECKED);
            _preSelected.Remove(oid);
            RefreshUncommittedColors();
            UpdateAncestors(node);
            UpdateSelectAllState();
            if (_currentRule != null && _currentRule["object_classification"] != null)
            {
                var arr = _currentRule["object_classification"] as JArray;
                var keep = new JArray();
                foreach (var it in arr)
                    if ((long)it["object_id"] != oid) keep.Add(it);
                _currentRule["object_classification"] = keep;
                _unsavedOids.Remove(oid);   // 已从规则移除，不再视为「编辑未保存」分类
                _dirty = true;
                UpdateStatus();
                MessageBox.Show("已将该分类从当前属性的领码规则中移除。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void _btnLoad_Click(object sender, EventArgs e)
        {
            if (_dirty)
            {
                var dr = MessageBox.Show("有未保存的改动，重新加载将丢弃，继续吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr != DialogResult.Yes) return;
            }
            LoadData();
            _dirty = false;
            _unsavedOids.Clear();   // 重新加载后所有内容以数据库为准，清空未保存标记
        }

        private void _btnSave_Click(object sender, EventArgs e) { Save(); }

        private void _btnClose_Click(object sender, EventArgs e) { Close(); }

        /// <summary>右键菜单弹出前过滤：仅在有效数据行上显示。多选状态由 _dgvMerge_CellMouseDown 维护，
        /// 这里只负责按当前选中数量动态调整菜单文字。</summary>
        private void _ctxMerge_Opening(object sender, CancelEventArgs e)
        {
            var pt = _dgvMerge.PointToClient(Cursor.Position);
            var hit = _dgvMerge.HitTest(pt.X, pt.Y);
            if (hit.RowIndex < 0 || hit.RowIndex >= _dgvMerge.Rows.Count || _dgvMerge.Rows[hit.RowIndex].IsNewRow)
            {
                e.Cancel = true;
                return;
            }
            int selCount = _dgvMerge.SelectedRows.Cast<DataGridViewRow>().Count(r => !r.IsNewRow);
            _ctxDelete.Text = selCount > 1 ? ("删除选中的 " + selCount + " 个码段") : "删除该码段";
        }

        /// <summary>右键按下瞬间捕获当前已选行（在 DataGridView 底层选择处理之前执行，
        /// 因此拿到的是「右击前」的真实多选快照；供 _dgvMerge_CellMouseDown 恢复/补全选择用）。</summary>
        private void _dgvMerge_PreRightClickCapture(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            _preRightClickSel = _dgvMerge.Rows.Cast<DataGridViewRow>()
                .Where(r => r.Selected && !r.IsNewRow)
                .Select(r => r.Index)
                .ToList();
        }

        /// <summary>右击接管：以「右键按下瞬间」的已选快照为准恢复选择。
        /// DataGridView 默认右击会把多选重置成单行，不接管就没法批量删除。
        /// 规则：右击行已选中 → 保留全部已选；未选中 → 在原有已选基础上追加该行（不清空）。</summary>
        private void _dgvMerge_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || _dgvMerge.Rows[e.RowIndex].IsNewRow) return;
            var row = _dgvMerge.Rows[e.RowIndex];
            if (_preRightClickSel.Contains(e.RowIndex))
            {
                // 右击行本就选中：把快照里的所有已选行重新选中（即便底层默认清掉了其余行也能恢复）
                foreach (int idx in _preRightClickSel)
                    if (idx >= 0 && idx < _dgvMerge.Rows.Count && !_dgvMerge.Rows[idx].IsNewRow)
                        _dgvMerge.Rows[idx].Selected = true;
            }
            else
            {
                // 右击行未选中：保留原有已选，并追加右击行（而非清空重置）
                foreach (int idx in _preRightClickSel)
                    if (idx >= 0 && idx < _dgvMerge.Rows.Count && !_dgvMerge.Rows[idx].IsNewRow)
                        _dgvMerge.Rows[idx].Selected = true;
                row.Selected = true;
            }
            // 仅当右击行尚未为当前单元格时设置，避免无谓触发 SelectionChanged
            if (_dgvMerge.CurrentCell == null || _dgvMerge.CurrentCell.RowIndex != e.RowIndex)
                _dgvMerge.CurrentCell = row.Cells[e.ColumnIndex >= 0 ? e.ColumnIndex : 0];
        }

        /// <summary>某行 type 改变后，刷新「属性名下拉」「固定值」两列的可用性（属性类→启用属性下拉；固定值→启用 value；其余禁用并清空）。</summary>
        private void _dgvMerge_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_refreshing || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (_dgvMerge.Columns[e.ColumnIndex].Name == "type")
                UpdateRowEditable(_dgvMerge.Rows[e.RowIndex]);
            RefreshCodeExample();
            MarkDirtyForCurrent();   // 合并属性段被编辑→记入未保存分类(左侧标绿)；刷新「编辑中·未保存」标记
        }

        // ===================== 编码示例实时预览（跟随合并属性段） =====================

        /// <summary>根据当前合并属性段实时生成「编码示例」预览（按段顺序拼接）：
        /// 取固定值→原值；取属性(编号/值)→属性内部名(internal_name)；取流水码→**占位；段间连接串原样追加。
        /// 即按用户配置实时拼出「实际值组合」，例如 属性值scbm + 固定值- + 流水码** + 属性编号mplx → scbm-**mplx。
        /// 仅作界面预览，实时随段配置变化；保存时一并写入 json（code_example）。</summary>
        private string BuildCodeExample()
        {
            var sb = new System.Text.StringBuilder();
            foreach (DataGridViewRow row in _dgvMerge.Rows)
            {
                if (row.IsNewRow) continue;
                string type = GetCell(row, "type");
                if (string.IsNullOrWhiteSpace(type)) continue;
                string part = null;
                switch (type)
                {
                    case "fixed":
                        part = GetCell(row, "value");
                        break;
                    case "property_number":
                    case "property_value":
                        string pin = GetCell(row, "property_internal_name");
                        part = string.IsNullOrWhiteSpace(pin) ? "{属性}" : pin;   // 直接用属性内部名展示实际值组合，不包裹、不取中文名
                        break;
                    case "serial_code":
                        part = "**";   // 流水码以 ** 占位，让人一眼看出此处为流水号位
                        break;
                    default:
                        part = "{" + type + "}";
                        break;
                }
                if (!string.IsNullOrEmpty(part)) sb.Append(part);
                // 段间连接串（json_array_connect_string，若有）追加在该段之后
                string conn = GetCell(row, "json_array_connect_string");
                if (!string.IsNullOrEmpty(conn)) sb.Append(conn);
            }
            return sb.ToString();
        }

        /// <summary>用当前段刷新「编码示例」框：仅在自动模式下、且存在有效段时覆盖文本，避免清空历史示例。</summary>
        private void RefreshCodeExample()
        {
            if (!_autoExample) return;
            bool hasSeg = false;
            foreach (DataGridViewRow r in _dgvMerge.Rows)
                if (!r.IsNewRow && !string.IsNullOrWhiteSpace(GetCell(r, "type"))) { hasSeg = true; break; }
            if (!hasSeg) return;
            _suppressExampleText = true;
            _txtCodeExample.Text = BuildCodeExample();
            _suppressExampleText = false;
        }

        /// <summary>网格行增删（新增/删除码段）后刷新示例预览。</summary>
        private void _dgvMerge_RowsChanged(object sender, EventArgs e)
        {
            if (_refreshing) return;
            RefreshCodeExample();
        }

        /// <summary>用户在示例框手动编辑后，退出自动跟随，避免被段配置变更覆盖。</summary>
        private void _txtCodeExample_TextChanged(object sender, EventArgs e)
        {
            if (_suppressExampleText) return;
            _autoExample = false;
        }

        /// <summary>用户在「编码内部名」框手动编辑后标记脏（程序化加载已用 _suppressCodeName 抑制）。</summary>
        private void _txtCodeName_TextChanged(object sender, EventArgs e)
        {
            if (_suppressCodeName || _refreshing) return;
            MarkDirtyForCurrent();
        }

        /// <summary>「重新生成示例」：恢复自动跟随并立即按当前段重算。</summary>
        private void _btnRegenExample_Click(object sender, EventArgs e)
        {
            _autoExample = true;
            RefreshCodeExample();
        }

        // ===================== 合并属性段拖拽排序 =====================

        /// <summary>左键按下「排序」列时记录拖拽源行，准备拖拽。</summary>
        private void _dgvMerge_DragMouseDown(object sender, MouseEventArgs e)
        {
            if (_refreshing) return;
            if (e.Button != MouseButtons.Left) return;
            if (!_dgvMerge.Columns.Contains("drag_handle")) return;
            var hit = _dgvMerge.HitTest(e.X, e.Y);
            if (hit.RowIndex < 0 || hit.RowIndex >= _dgvMerge.Rows.Count) return;
            if (_dgvMerge.Rows[hit.RowIndex].IsNewRow) return;
            if (hit.ColumnIndex != _dgvMerge.Columns["drag_handle"].Index) return;
            _dragRowIndex = hit.RowIndex;
            _dragStartPoint = e.Location;
            _dragSourceRect = _dgvMerge.GetRowDisplayRectangle(hit.RowIndex, false);
            _dragging = false;
        }

        /// <summary>左键移动超过阈值后开始拖拽（DoDragDrop 为模态，结束后清理状态）。</summary>
        private void _dgvMerge_DragMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragRowIndex < 0) return;
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left) { ResetDragState(); return; }
            if (!_dragging && (Math.Abs(e.X - _dragStartPoint.X) > DragThreshold
                || Math.Abs(e.Y - _dragStartPoint.Y) > DragThreshold))
            {
                _dragging = true;
                _dgvMerge.Invalidate();
                _dgvMerge.DoDragDrop(_dragRowIndex, DragDropEffects.Move);
                ResetDragState();
            }
        }

        /// <summary>未真正进入拖拽（仅点击）时复位状态。</summary>
        private void _dgvMerge_DragMouseUp(object sender, MouseEventArgs e)
        {
            if (!_dragging) ResetDragState();
        }

        /// <summary>拖拽悬停时实时计算插入位置（上/下半区）并触发重绘以显示插入线。</summary>
        private void _dgvMerge_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(int))) { e.Effect = DragDropEffects.None; return; }
            e.Effect = DragDropEffects.Move;
            Point pt = _dgvMerge.PointToClient(new Point(e.X, e.Y));
            var hit = _dgvMerge.HitTest(pt.X, pt.Y);
            int newOver;
            bool newAfter;
            if (hit.RowIndex >= 0 && hit.RowIndex < _dgvMerge.Rows.Count && !_dgvMerge.Rows[hit.RowIndex].IsNewRow)
            {
                var rect = _dgvMerge.GetRowDisplayRectangle(hit.RowIndex, false);
                newAfter = pt.Y > rect.Top + rect.Height / 2;
                newOver = hit.RowIndex;
            }
            else
            {
                newOver = -1;     // 拖到网格外或新行之上：视为追加到末尾
                newAfter = true;
            }
            if (newOver != _dragOverRowIndex || newAfter != _dragInsertAfter)
            {
                _dragOverRowIndex = newOver;
                _dragInsertAfter = newAfter;
                _dgvMerge.Invalidate();
            }
        }

        /// <summary>执行落位：把源行移动到计算的插入位置，写回当前编辑对象并标记脏。</summary>
        private void _dgvMerge_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(int))) { ResetDragState(); return; }
            int from = (int)e.Data.GetData(typeof(int));
            Point pt = _dgvMerge.PointToClient(new Point(e.X, e.Y));
            int insertIndex = ComputeInsertIndex(pt);

            _dgvMerge.EndEdit();   // 先提交可能处于编辑态的单元格，避免丢值
            var snap = CaptureRow(from);
            _dgvMerge.Rows.RemoveAt(from);
            if (from < insertIndex) insertIndex--;
            if (insertIndex < 0) insertIndex = 0;
            int maxInsert = _dgvMerge.Rows.Count - 1;   // 不允许插到末尾「新行」之后
            if (insertIndex > maxInsert) insertIndex = maxInsert;

            if (insertIndex == from)
            {
                // 净位置未变，原样放回即可
                int restore = from > maxInsert ? maxInsert : from;
                InsertRowAt(restore, snap);
                RefreshDragHandles();
                ResetDragState();
                return;
            }

            InsertRowAt(insertIndex, snap);
            RefreshDragHandles();
            _dgvMerge.ClearSelection();
            if (insertIndex >= 0 && insertIndex < _dgvMerge.Rows.Count)
                _dgvMerge.Rows[insertIndex].Selected = true;
            // 写回当前编辑对象（具体分类或规则级模板），使新顺序在保存后生效
            SyncEditorToEditingTarget();
            RefreshCodeExample();   // 拖拽改变段顺序后，编码示例同步重排
            MarkDirtyForCurrent();
            ResetDragState();
        }

        /// <summary>根据光标位置计算插入行索引：拖到某行上半区→插入其前；下半区→插入其后；网格外→末尾。</summary>
        private int ComputeInsertIndex(Point clientPt)
        {
            var hit = _dgvMerge.HitTest(clientPt.X, clientPt.Y);
            if (hit.RowIndex >= 0 && hit.RowIndex < _dgvMerge.Rows.Count && !_dgvMerge.Rows[hit.RowIndex].IsNewRow)
            {
                var rect = _dgvMerge.GetRowDisplayRectangle(hit.RowIndex, false);
                bool after = clientPt.Y > rect.Top + rect.Height / 2;
                return after ? hit.RowIndex + 1 : hit.RowIndex;
            }
            return _dgvMerge.Rows.Count - 1;   // 追加到最后一个数据行之后（新行之前）
        }

        /// <summary>绘制实时拖拽反馈：高亮源行 + 蓝色插入线 + 「移动到第 N 行」角标。</summary>
        private void _dgvMerge_Paint(object sender, PaintEventArgs e)
        {
            if (!_dragging || _dragRowIndex < 0) return;
            // 高亮源行
            if (_dragRowIndex >= 0 && _dragRowIndex < _dgvMerge.Rows.Count && !_dgvMerge.Rows[_dragRowIndex].IsNewRow)
            {
                Rectangle sr = _dgvMerge.GetRowDisplayRectangle(_dragRowIndex, false);
                if (sr.Height > 0)
                    using (var b = new SolidBrush(Color.FromArgb(60, 30, 144, 255)))
                        e.Graphics.FillRectangle(b, sr);
            }
            // 计算插入线参考矩形
            Rectangle refRect;
            int overIdx = _dragOverRowIndex;
            bool after = _dragInsertAfter;
            if (overIdx >= 0 && overIdx < _dgvMerge.Rows.Count && !_dgvMerge.Rows[overIdx].IsNewRow)
                refRect = _dgvMerge.GetRowDisplayRectangle(overIdx, false);
            else
            {
                int last = -1;
                for (int i = _dgvMerge.Rows.Count - 1; i >= 0; i--)
                { if (!_dgvMerge.Rows[i].IsNewRow) { last = i; break; } }
                if (last < 0) return;
                refRect = _dgvMerge.GetRowDisplayRectangle(last, false);
                after = true;
            }
            if (refRect.Height <= 0) return;
            int y = after ? refRect.Bottom : refRect.Top;
            int w = _dgvMerge.Width;
            using (var pen = new Pen(Color.DodgerBlue, 2))
                e.Graphics.DrawLine(pen, 0, y, w, y);
            // 提示角标
            int insertPos = after
                ? (overIdx >= 0 ? overIdx + 2 : _dgvMerge.Rows.Count)
                : (overIdx >= 0 ? overIdx + 1 : _dgvMerge.Rows.Count);
            string tip = string.Format("移动到第 {0} 行", insertPos);
            using (var font = new Font(_dgvMerge.Font, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.DodgerBlue))
            {
                var size = e.Graphics.MeasureString(tip, font);
                float tx = w - size.Width - 6;
                float ty = y - size.Height - 2;
                if (ty < 0) ty = y + 2;
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 230, 243, 255)), tx - 2, ty, size.Width + 4, size.Height + 2);
                e.Graphics.DrawString(tip, font, brush, tx, ty);
            }
        }

        /// <summary>按 Esc 取消拖拽。</summary>
        private void _dgvMerge_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (e.EscapePressed) { e.Action = DragAction.Cancel; ResetDragState(); }
        }

        /// <summary>缓存整行数据（含原始段 Tag），用于移动后重建。</summary>
        private RowSnapshot CaptureRow(int index)
        {
            var row = _dgvMerge.Rows[index];
            var snap = new RowSnapshot();
            snap.Tag = row.Tag;
            snap.Cells = new object[_dgvMerge.Columns.Count];
            for (int c = 0; c < _dgvMerge.Columns.Count; c++)
                snap.Cells[c] = row.Cells[c].Value;
            return snap;
        }

        /// <summary>在指定索引处重建一行并恢复数据与可用性。</summary>
        private void InsertRowAt(int index, RowSnapshot snap)
        {
            _dgvMerge.Rows.Insert(index);
            var row = _dgvMerge.Rows[index];
            row.Tag = snap.Tag;
            for (int c = 0; c < snap.Cells.Length && c < _dgvMerge.Columns.Count; c++)
                row.Cells[c].Value = snap.Cells[c];
            UpdateRowEditable(row);
        }

        /// <summary>重排后刷新首列「排序」的序号展示。</summary>
        private void RefreshDragHandles()
        {
            if (!_dgvMerge.Columns.Contains("drag_handle")) return;
            int n = 1;
            foreach (DataGridViewRow row in _dgvMerge.Rows)
            {
                if (row.IsNewRow) continue;
                row.Cells["drag_handle"].Value = n.ToString();
                n++;
            }
        }

        /// <summary>复位全部拖拽状态并触发一次重绘以清除反馈。</summary>
        private void ResetDragState()
        {
            _dragRowIndex = -1;
            _dragOverRowIndex = -1;
            _dragInsertAfter = false;
            _dragging = false;
            _dgvMerge.Invalidate();
        }

        /// <summary>删除所有选中的码段（支持多选批量删除）：从当前属性的规则级 code_rule.merge_properties 中按降序移除对应元素。
        /// by_rule 规则下右侧为参考模板，不支持在此直接删除。</summary>
        private void _ctxDelete_Click(object sender, EventArgs e)
        {
            if (_currentRule == null) return;
            if ((string)_currentRule["type"] == "by_rule")
            {
                MessageBox.Show("by_rule 规则请在左侧分类列表选中具体分类后再编辑删除。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 当前编辑对象：具体分类(_editingOid) 或 规则级模板；删除只作用于它，不影响其他分类
            JObject targetCr = ResolveEditingCodeRule();
            if (targetCr == null)
            {
                MessageBox.Show("未找到当前编辑对象（请先选中一个已勾选分类，或处于规则级模板视图）。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 收集所有选中且非新行的行索引，按降序排列，保证逐个删除时索引不串位
            var sel = _dgvMerge.SelectedRows.Cast<DataGridViewRow>()
                .Where(x => !x.IsNewRow)
                .Select(x => x.Index)
                .Distinct()
                .OrderByDescending(i => i)
                .ToList();
            if (sel.Count == 0) return;
            string targetName = GetCurrentEditingTargetName();
            string msg = sel.Count == 1
                ? string.Format("确定删除第 {0} 个码段吗？\n\n所属对象：{1}\n此操作不可撤销。", sel[0] + 1, targetName)
                : string.Format("确定删除选中的 {0} 个码段吗？\n\n所属对象：{1}\n此操作不可撤销。", sel.Count, targetName);
            var dr = MessageBox.Show(msg, "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes) return;
            // 仅把界面同步到「当前编辑对象」本身（引用写入），再删其 merge_properties 中对应行；不影响其他分类/规则级模板
            SyncEditorToEditingTarget(targetCr);
            var mp = targetCr["merge_properties"] as JArray;
            if (mp != null)
            {
                foreach (int idx in sel)
                {
                    if (idx >= 0 && idx < mp.Count) mp.RemoveAt(idx);
                }
                _dirty = true;
                if (_editingOid.HasValue) _unsavedOids.Add(_editingOid.Value);
                LoadTemplateToUI(targetCr);
                UpdateStatus();
            }
        }

        /// <summary>打开「跨属性分类规则复制」窗体。共享当前内存中的 _root / _rules / _allProps，
        /// 复制结果直接写入 _rules；返回 OK 后标记脏并刷新当前属性视图，由「保存到数据库」统一落库。</summary>
        private void _btnCopy_Click(object sender, EventArgs e)
        {
            if (_dirty)
            {
                var dr = MessageBox.Show("当前有未保存的改动，跨属性复制前建议先保存。仍要继续吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;
            }
            using (var f = new CodeLinkCopyForm(_root, _rules, _allProps))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    _dirty = true;
                    LoadTargetProperty(_currentProp);
                    // 复制结果已写入 _rules（覆盖当前属性各分类规则），标记它们为未保存
                    if (_currentRule != null && _currentRule["object_classification"] != null)
                        foreach (JObject it in (JArray)_currentRule["object_classification"])
                            if (it["object_id"] != null) _unsavedOids.Add((long)it["object_id"]);
                    UpdateStatus();
                }
            }
        }

        /// <summary>加载时：把系统的「单块(无 target_property，所有分类堆在 object_classification 里)」结构，
        /// 拆成按各分类自身 code_rule.code_internal_name 分组的「每属性一块」结构，便于本工具按 target_property 编辑与勾选联动。
        /// 同时记下原块的锚 code_rule，供保存时还原。</summary>
        private void NormalizeRulesForEditing()
        {
            // 系统结构 = property_to_code_rules 每元素一个属性块，属性名在【块级 target_property】字段
            // （块级 code_rule.code_internal_name 是编码类型，两者可能不同，不能互替）。
            // 工具直接读写该多块结构，【无需任何转换】：加载原样保留，不拆、不重组、不补、不覆盖任何字段。
            // 此前「按分类 code_internal_name 拆块」和「target_property = code_internal_name 补标记」均属错误，
            // 都会破坏块级属性名 target_property —— 这就是「属性名消失」的根因。
        }

        /// <summary>保存时：把所有按属性分块的规则合并回「系统单块」结构(去掉 target_property)，
        /// 所有分类汇总进一个 object_classification，锚 code_rule 还原为加载时记下的系统锚(缺失则用 drawing_code 默认)。
        /// 这样跟系统自己维护的结构一致，生成引擎才认得每个分类自己的 code_internal_name。</summary>
        private void MergeRulesForSave()
        {
            // 保存：直接把内存中的 _rules 原样挂回 property_to_code_rules，【不做任何合并/去重/删除/覆盖】。
            // 关键：必须保留每个块的 target_property（= 属性名，系统靠它识别属性）——一旦 Remove 掉，属性名就没了。
            // 最终修复：加载不拆、保存不合并，多块结构原样往返。
            _root["property_to_code_rules"] = _rules;
        }

        private void Save()
        {
            try
            {
                // 同步属性名（用户可能直接在 ComboBox 输入新属性名，不触发 SelectedIndexChanged）
                _currentProp = CurrentPropInternalName();
                if (_currentRule == null || (string)_currentRule["target_property"] != _currentProp)
                {
                    _currentRule = null;
                    foreach (JObject r in _rules)
                        if ((string)r["target_property"] == _currentProp) { _currentRule = r; break; }
                    _editingOid = null;
                }
                if (_currentRule == null && !string.IsNullOrWhiteSpace(_currentProp))
                {
                    // 新建属性规则（与 ApplyCurrent 新建逻辑一致），使后续写回/保存有承载对象
                    _currentRule = new JObject();
                    _currentRule["target_property"] = _currentProp;
                    _currentRule["type"] = "merge_property";
                    _currentRule["object_classification"] = new JArray();
                    _currentRule["code_rule"] = new JObject();
                    _rules.Add(_currentRule);
                }
                // 仅把右侧界面写回当前编辑对象（具体分类或规则级模板），并同步分类成员；
                // 不广播到所有勾选分类——避免删除/局部编辑误伤其它分类。需要整体套用请点「应用」。
                SyncEditorToEditingTarget();
                ApplyClassificationChanges(false);
                MergeRulesForSave();   // 合并回系统单块结构(去 target_property)，保证与系统配置一致、能被生成引擎正确识别
                string json = _root.ToString(Formatting.None);
                _db.SaveCodeLinkSettingContent(json, _ownerType, _ownerObjectId, _version);
                _dirty = false;
                // 保存后已落库：清空「预选中(绿色)」暂存集，绿色高亮随之消失（节点仍保持勾选=已保存成员）
                _preSelected.Clear();
                _unsavedOids.Clear();   // 已落库：清空未保存分类集合，绿色随之消失
                RefreshUncommittedColors();
                MessageBox.Show("已保存到数据库。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError("领码设置", "保存失败：" + ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsChecked(long oid)
        {
            return _nodeByOid.TryGetValue(oid, out TreeNode n) && NodeState(n) == ST_CHECKED;
        }

        private bool IsClassInCurrentRule(long oid)
        {
            if (_currentRule == null) return false;
            var arr = _currentRule["object_classification"] as JArray;
            if (arr == null) return false;
            foreach (var it in arr)
                if ((long)it["object_id"] == oid) return true;
            return false;
        }

        private JObject BuildTemplateFromUI()
        {
            var cr = new JObject();
            cr["code_internal_name"] = _txtCodeName.Text.Trim();
            cr["code_example"] = _txtCodeExample.Text.Trim();
            var mp = new JArray();
            foreach (DataGridViewRow row in _dgvMerge.Rows)
            {
                if (row.IsNewRow) continue;
                string type = GetCell(row, "type");
                if (string.IsNullOrWhiteSpace(type)) continue;
                var seg = new JObject();
                seg["type"] = type;
                // 可选字段采用「仅在有意义的（非空/非默认）值时写出」策略，避免「打开→保存」把库中缺省的字段
                // 固化成 false / 空串（往返污染）。JSON 中缺省即视为 false，故布尔 flag 未勾选时直接省略，语义等价。
                string conn = GetCell(row, "json_array_connect_string");
                if (!string.IsNullOrEmpty(conn)) seg["json_array_connect_string"] = conn;
                if (GetBoolCell(row, "property_merged_fill_back_flag")) seg["property_merged_fill_back_flag"] = true;
                string fillVal = GetCell(row, "property_merged_fill_value");
                if (!string.IsNullOrEmpty(fillVal)) seg["property_merged_fill_value"] = fillVal;
                string fillLen = GetCell(row, "property_merged_length");
                if (!string.IsNullOrEmpty(fillLen)) seg["property_merged_length"] = fillLen;
                if (GetBoolCell(row, "next_empty_clear_flag")) seg["next_empty_clear_flag"] = true;
                if (GetBoolCell(row, "prev_empty_clear_flag")) seg["prev_empty_clear_flag"] = true;
                if (type == "fixed") seg["value"] = GetCell(row, "value");
                if (type == "property_number" || type == "property_value") seg["property_internal_name"] = GetCell(row, "property_internal_name");
                if (type == "serial_code")
                {
                    seg["internal_name"] = "code_rule_segment";
                    long oid = GetLongCell(row, "object_id");
                    if (oid == 0)
                    {
                        // 解析失败/为空：回退到该行原始 object_id，避免写入非法 0 生成无效码段
                        var orig = row.Tag as JObject;
                        if (orig != null && orig["object_id"] != null && orig["object_id"].Type == JTokenType.Integer)
                            oid = (long)orig["object_id"];
                    }
                    if (oid != 0) seg["object_id"] = oid;
                }
                mp.Add(seg);
            }
            cr["merge_properties"] = mp;
            return cr;
        }

        private void LoadTemplateToUI(JObject cr)
        {
            _refreshing = true;
            // 先按当前规则实际出现的 type 值重建下拉数据源，避免库中旧英文值不在下拉项而抛异常
            var extraTypes = new List<string>();
            if (cr != null)
            {
                var mpTypes = cr["merge_properties"] as JArray;
                if (mpTypes != null)
                    foreach (var seg in mpTypes)
                    {
                        string t = seg["type"] != null ? (string)seg["type"] : string.Empty;
                        if (!string.IsNullOrWhiteSpace(t)) extraTypes.Add(t);
                    }
            }
            BuildTypeDataSource(extraTypes);
            _dgvMerge.Rows.Clear();
            if (cr == null)
            {
                _txtCodeName.Text = string.Empty;
                _txtCodeExample.Text = string.Empty;
                _refreshing = false;
                return;
            }
            _suppressCodeName = true;
            _txtCodeName.Text = cr["code_internal_name"] != null ? (string)cr["code_internal_name"] : string.Empty;
            _suppressCodeName = false;
            _suppressExampleText = true;
            _txtCodeExample.Text = cr["code_example"] != null ? (string)cr["code_example"] : string.Empty;
            _suppressExampleText = false;
            var mp = cr["merge_properties"] as JArray;
            if (mp != null)
            {
                foreach (var seg in mp)
                {
                    int i = _dgvMerge.Rows.Add();
                    DataGridViewRow row = _dgvMerge.Rows[i];
                    row.Tag = seg;   // 存原始段，供保存时 object_id 解析失败回退到原值
                    // 直接以英文 Value 赋值，DataSource 绑定会自动显示中文 Display
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
                    UpdateRowEditable(row);
                }
            }
            // 点击分类刷新后，默认不选中任何行，避免误删；用户需手动选择要操作的行
            _dgvMerge.ClearSelection();
            RefreshDragHandles();
            _autoExample = true;          // 加载后恢复示例跟随配置
            RefreshCodeExample();         // 按当前段实时生成编码示例预览
            _refreshing = false;
        }

        /// <summary>解析右侧网格当前编辑对象的 code_rule（引用）：
        /// _editingOid 有值→该分类自身的 code_rule；否则→规则级模板 _currentRule["code_rule"]（缺失则新建）。</summary>
        private JObject ResolveEditingCodeRule()
        {
            if (_currentRule == null) return null;
            if (_editingOid.HasValue)
            {
                var arr = _currentRule["object_classification"] as JArray;
                if (arr != null)
                    foreach (var it in arr)
                        if ((long)it["object_id"] == _editingOid.Value && it["code_rule"] != null)
                            return it["code_rule"] as JObject;
                return null;
            }
            var cr = _currentRule["code_rule"] as JObject;
            if (cr == null) { cr = new JObject(); _currentRule["code_rule"] = cr; }
            return cr;
        }

        /// <summary>获取当前右侧网格正在编辑的目标名称（具体分类显示名或规则级模板）。用于删除确认提示。</summary>
        private string GetCurrentEditingTargetName()
        {
            if (_editingOid.HasValue && _tvClass.SelectedNode != null)
                return _tvClass.SelectedNode.Name;   // 用原名，避免带「◀ 编辑中」标记后缀
            return "规则级模板（当前属性通用模板）";
        }

        /// <summary>把右侧网格当前内容写入指定 code_rule 对象（引用写入，不新建、不换对象身份），保留该分类独立性。</summary>
        private void SyncEditorToEditingTarget(JObject targetCr)
        {
            if (targetCr == null) return;
            var ui = BuildTemplateFromUI();
            targetCr["code_internal_name"] = ui["code_internal_name"];
            targetCr["code_example"] = ui["code_example"];
            targetCr["merge_properties"] = ui["merge_properties"];
        }

        /// <summary>无参重载：写入「当前编辑对象」（由 _editingOid 决定具体分类或规则级模板）。</summary>
        private void SyncEditorToEditingTarget()
        {
            SyncEditorToEditingTarget(ResolveEditingCodeRule());
        }

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

        /// <summary>左侧分类树「编辑中」标记：清除上一个被标记节点（还原原名 + 取消加粗），
        /// 再对当前正在编辑的具体分类（_editingOid 有值且恰为选中节点）加粗并在文本前方加入「◀ 编辑中 / ◀ 编辑中·未保存」。
        /// 目的是在「所有分类都勾选」的状体下，明确告诉用户：右侧合并属性段当前只编辑、只保存「这一个」分类。
        /// 编辑属性级通用模板(_editingOid 为空)时不标记任何具体分类。</summary>
        private void UpdateEditingMarker()
        {
            // 复位上一个被标记节点
            if (_markedNode != null && _markedNode.TreeView != null)
            {
                _markedNode.Text = _markedNode.Name;   // 用存好的原名还原
                _markedNode.NodeFont = null;
                _markedNode = null;
            }
            if (_refreshing) return;
            // 仅当「正在编辑某个具体分类」且「该分类正是当前选中节点」时才标记
            if (!_editingOid.HasValue) return;
            if (_tvClass.SelectedNode != null && _tvClass.SelectedNode.Tag is long oid && oid == _editingOid.Value)
            {
                var node = _tvClass.SelectedNode;
                if (_boldFont != null) node.NodeFont = _boldFont;
                node.Text = (_dirty ? "◀ 编辑中·未保存  " : "◀ 编辑中  ") + node.Name;
                _markedNode = node;
            }
        }

        private void UpdateStatus()
        {
            int n = 0;
            if (_currentRule != null && _currentRule["object_classification"] != null)
                n = ((JArray)_currentRule["object_classification"]).Count;
            // 统计「尚未落库(绿色)」节点：预选中 + 已编辑未保存（含光标已移开的分类）
            int green = 0;
            foreach (var kv in _nodeByOid)
            {
                long oid = kv.Key;
                if (_preSelected.Contains(oid) || _unsavedOids.Contains(oid)) green++;
            }
            string propZh = LookupZh(_currentProp);
            string propLabel = string.IsNullOrEmpty(_currentProp)
                ? "(未选)"
                : _currentProp + (string.IsNullOrEmpty(propZh) ? string.Empty : "（" + propZh + "）");
            // 顶部状态条：明确当前编辑对象，突出「仅编辑此分类」提示（所有分类都勾选时尤需此提示）
            if (_editingOid.HasValue && _tvClass.SelectedNode != null)
            {
                _lblStatus.ForeColor = _dirty ? Color.FromArgb(200, 60, 0) : Color.FromArgb(0, 110, 160);
                _lblStatus.Text = "当前属性：" + propLabel + "（" + n + " 个分类）   ◆ 仅编辑此分类："
                    + _tvClass.SelectedNode.Name + (_dirty ? "（有未保存修改）" : "（只保存这一个分类）");
            }
            else
            {
                _lblStatus.ForeColor = Color.FromArgb(120, 120, 120);
                _lblStatus.Text = "当前属性：" + propLabel + "（" + n + " 个分类）   ◇ 当前编辑：属性级通用模板（「应用」后将影响所有勾选分类）";
            }
            // 左侧分类标签：反映成员数 + 当前未保存(绿色)节点数，提供实时反馈
            _lblClass.Text = "分类（共 " + n + " 个成员，" + green + " 个未保存(绿色)）：";
            RefreshUncommittedColors();   // 刷新绿色高亮（含编辑未保存的分类）
            UpdateEditingMarker();
        }

        /// <summary>DataGridView 中文列实时显示：根据同一行 property_internal_name 反查中文名（纯展示，不入库、不影响后台逻辑）。</summary>
        private void _dgvMerge_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _refreshing) return;
            if (_dgvMerge.Columns[e.ColumnIndex].Name != "property_internal_name_zh") return;
            var internalName = _dgvMerge.Rows[e.RowIndex].Cells["property_internal_name"].Value as string;
            e.Value = LookupZh(internalName);
            e.FormattingApplied = true;
        }

        /// <summary>英文字段名 → 中文名；找不到或为空返回空串。</summary>
        private string LookupZh(string internalName)
        {
            if (string.IsNullOrWhiteSpace(internalName)) return string.Empty;
            if (_propByInternal.TryGetValue(internalName.Trim(), out MetaPropertyOption p))
                return string.IsNullOrWhiteSpace(p.DisplayName) ? string.Empty : p.DisplayName;
            return string.Empty;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_dirty)
            {
                var dr = MessageBox.Show("有未保存的改动，确定关闭吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr != DialogResult.Yes) e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}
