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
    /// 零件匹配规则（match_rule）快速配置窗体。
    /// 仅编辑 manufacture（制造件）下的 object_classifications 数组，
    /// 其余 JSON 结构（cooperation/purchased/standard、开关、range_match 等）原样保留。
    /// </summary>
    public partial class MatchRuleConfigForm : Form
    {
        private readonly DbHelper _db;

        private List<MatchRuleClassification> _rules = new List<MatchRuleClassification>();
        private List<ClassificationGroup> _classifications = new List<ClassificationGroup>();
        private List<MetaPropertyOption> _allProps = new List<MetaPropertyOption>();
        private List<SourceTypeOption> _sourceTypes = new List<SourceTypeOption>();
        private string _currentKey = "manufacture";

        private JObject _originalRoot;
        private int _selectedIndex = -1;
        private List<string> _selectedFields = new List<string>();
        /// <summary>右侧正在配置、尚未经「预保存」暂存的分类规则草稿；null 表示当前编辑的是已暂存项或空。</summary>
        private MatchRuleClassification _draft = null;
        private List<string> _clbFieldNames = new List<string>();
        /// <summary>当前绑定分类的表单模板字段集合（大小写不敏感），用于右侧匹配字段排序和候选集补充：表单里的字段不在 meta_element 中也能被勾到。</summary>
        private HashSet<string> _currentFormFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _dirty = false;
        private bool _refreshing = false;
        private int _ruleOwnerType = 0;
        private long _ruleOwnerObjectId = 0;
        private string _ruleVersion = null;

        private const string DefaultMatchRuleJson =
            "{\"empty_key_match_flag\":true,\"global_flag\":true,\"global_match\":{\"condition_match\":{\"condition\":" +
            "{\"cooperation\":{\"fields\":[\"id_code\"],\"object_classifications\":[]}," +
            "\"manufacture\":{\"fields\":[\"id_code\"],\"object_classifications\":[]}," +
            "\"purchased\":{\"fields\":[\"id_code\"],\"object_classifications\":[]}," +
            "\"standard\":{\"fields\":[\"id_code\",\"name\"],\"object_classifications\":[]}}," +
            "\"meta_name\":\"part_source_type\"},\"condition_match_flag\":true,\"fields\":[\"id_code\"],\"object_classifications\":[]}," +
            "\"range_match\":null}";

        private Label _lblSourceType;
        private ComboBox _cmbSourceType;
        private Label _lblStatus;
        private ListBox _lstRules;
        private Label _lblClass;
        private ComboBox _cmbClass;
        /// <summary>已被其他规则占用的分类 ObjectId 集合（当前正在编辑的规则除外），用于 _cmbClass 单项置灰禁用。</summary>
        private HashSet<long> _disabledClassObjectIds = new HashSet<long>();
        /// <summary>_cmbClass 上次合法选中项的索引，用户误选禁用项时回退到此。</summary>
        private int _lastValidClassIndex = -1;
        private CheckBox _chkStrict;
        private Label _lblSearch;
        private TextBox _txtSearch;
        private CheckedListBox _clbFields;
        private Button _btnSelectAll;
        private Button _btnClearFields;
        private Button _btnDelete;
        private Button _btnPreSave;
        private Button _btnLoad;
        private Button _btnSave;
        private Button _btnClose;
        private Button _btnExcludeFields;
        private Label lblRules;
        private Label lblFields;
        private ToolTip _toolTip;

        /// <summary>设计器专用无参构造：仅初始化 UI 与事件订阅，不访问数据库（设计期 _db 为 null）。</summary>
        public MatchRuleConfigForm()
        {
            InitializeComponent();
            WireUpEvents();
        }

        public MatchRuleConfigForm(DbHelper db)
            : this()
        {
            _db = db;
            LoadData();
        }

        /// <summary>当前右侧面板绑定的规则对象：草稿优先，否则取已暂存的 _selectedIndex 项。</summary>
        private MatchRuleClassification CurrentRule
        {
            get
            {
                if (_draft != null) return _draft;
                if (_selectedIndex >= 0 && _selectedIndex < _rules.Count) return _rules[_selectedIndex];
                return null;
            }
        }

        /// <summary>集中订阅所有事件。必须放在构造函数（InitializeComponent 之后）而非 InitializeComponent 内，
        /// 避免手写 InitializeComponent 时遗漏订阅导致交互全部失灵。</summary>
        private void WireUpEvents()
        {
            _cmbSourceType.SelectedIndexChanged += _cmbSourceType_SelectedIndexChanged;
            _lstRules.SelectedIndexChanged += _lstRules_SelectedIndexChanged;
            _lstRules.MouseMove += _lstRules_MouseMove;
            _lstRules.DrawItem += _lstRules_DrawItem;
            _cmbClass.SelectedIndexChanged += _cmbClass_SelectedIndexChanged;
            _cmbClass.DrawItem += _cmbClass_DrawItem;
            _txtSearch.TextChanged += _txtSearch_TextChanged;
            _clbFields.ItemCheck += _clbFields_ItemCheck;
            _chkStrict.CheckedChanged += _chkStrict_CheckedChanged;
            _btnSelectAll.Click += _btnSelectAll_Click;
            _btnClearFields.Click += _btnClearFields_Click;
            _btnDelete.Click += _btnDelete_Click;
            _btnPreSave.Click += _btnPreSave_Click;
            _btnLoad.Click += _btnLoad_Click;
            _btnSave.Click += _btnSave_Click;
            _btnClose.Click += _btnClose_Click;
            _btnExcludeFields.Click += _btnExcludeFields_Click;
        }

        private void LoadData()
        {
            try
            {
                _classifications = _db.GetAllClassifications() ?? new List<ClassificationGroup>();
                _allProps = _db.GetMetaPropertyOptions() ?? new List<MetaPropertyOption>();
                _sourceTypes = _db.GetSourceTypes() ?? new List<SourceTypeOption>();

                _cmbClass.Items.Clear();
                foreach (var c in _classifications)
                    _cmbClass.Items.Add(string.Format("{0}  ({1})", c.Name, c.ObjectId));

                // 填充生产类型下拉，默认选中「自制件 manufacture」（兼容既有数据）
                _refreshing = true;
                _cmbSourceType.Items.Clear();
                foreach (var s in _sourceTypes)
                    _cmbSourceType.Items.Add(s.DisplayName);
                int defaultIdx = 0;
                for (int i = 0; i < _sourceTypes.Count; i++)
                {
                    if (_sourceTypes[i].InternalName == "manufacture")
                    {
                        defaultIdx = i;
                        break;
                    }
                }
                if (_sourceTypes.Count > 0)
                {
                    _cmbSourceType.SelectedIndex = defaultIdx;
                    _currentKey = _sourceTypes[defaultIdx].InternalName;
                }
                _refreshing = false;

                var rec = _db.GetMatchRuleRecord("part");
                if (rec == null || string.IsNullOrWhiteSpace(rec.Content))
                {
                    _originalRoot = null;
                    _rules = new List<MatchRuleClassification>();
                    _ruleOwnerType = 0;
                    _ruleOwnerObjectId = 0;
                    _ruleVersion = null;
                    _lblStatus.Text = "未找到匹配规则，将以空规则开始";
                }
                else
                {
                    _originalRoot = JObject.Parse(rec.Content);
                    _rules = ParseRulesForSource(_originalRoot, _currentKey);
                    _ruleOwnerType = rec.OwnerType;
                    _ruleOwnerObjectId = rec.OwnerObjectId;
                    _ruleVersion = rec.Version;
                    UpdateStatusForCurrentKey();
                }

                _draft = null;
                MergeStagedIntoRules();
                RefreshRuleList();
                _selectedIndex = _lstRules.SelectedIndex;
                LoadRuleToPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _lblStatus.Text = "加载失败";
            }
        }

        private static List<MatchRuleClassification> ParseRulesForSource(JObject root, string sourceKey)
        {
            var rules = new List<MatchRuleClassification>();
            if (root == null || string.IsNullOrEmpty(sourceKey)) return rules;
            try
            {
                var arr = root.SelectToken("global_match.condition_match.condition." + sourceKey + ".object_classifications") as JArray;
                if (arr == null) return rules;

                foreach (var item in arr)
                {
                    var r = new MatchRuleClassification();
                    r.ObjectId = item["object_id"] != null ? (long)item["object_id"] : 0L;
                    r.StrictMatchFlag = item["strict_match_flag"] != null && (bool)item["strict_match_flag"];
                    var fs = item["fields"] as JArray;
                    if (fs != null)
                        foreach (var f in fs)
                            r.Fields.Add(f.ToString());
                    rules.Add(r);
                }
            }
            catch { }
            return rules;
        }

        private void _cmbSourceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_refreshing) return;
            int idx = _cmbSourceType.SelectedIndex;
            if (idx < 0 || idx >= _sourceTypes.Count) return;
            string newKey = _sourceTypes[idx].InternalName;
            if (newKey == _currentKey) return;

            EnsureDraftCommitted();   // 切换生产类型前，先把未预保存草稿提交到当前类型
            WriteCurrentRulesToRoot();
            _currentKey = newKey;
            LoadRulesForCurrentKey();
            _dirty = true;
        }

        private void LoadRulesForCurrentKey()
        {
            _draft = null;
            if (_originalRoot != null)
                _rules = ParseRulesForSource(_originalRoot, _currentKey);
            else
                _rules = new List<MatchRuleClassification>();

            MergeStagedIntoRules();

            _selectedFields = new List<string>();
            RefreshRuleList();
            _selectedIndex = _lstRules.SelectedIndex;
            LoadRuleToPanel();
            UpdateStatusForCurrentKey();
        }

        private void WriteCurrentRulesToRoot()
        {
            if (_originalRoot == null)
                _originalRoot = JObject.Parse(DefaultMatchRuleJson);
            ApplyCurrentPanelToRule();
            WriteRulesToSource(_originalRoot, _currentKey, _rules);
        }

        private void WriteRulesToSource(JObject root, string key, List<MatchRuleClassification> rules)
        {
            var condition = root.SelectToken("global_match.condition_match.condition") as JObject;
            if (condition == null) return;
            var node = condition[key] as JObject;
            if (node == null)
            {
                node = new JObject();
                node["fields"] = new JArray(new JValue("id_code"));
                condition[key] = node;
            }
            node["object_classifications"] = BuildClassificationsArray(rules);
        }

        private void UpdateStatusForCurrentKey()
        {
            string name = _currentKey;
            foreach (var s in _sourceTypes)
                if (s.InternalName == _currentKey)
                {
                    name = s.DisplayName;
                    break;
                }
            _lblStatus.Text = string.Format("当前类型：{0}（{1} 个分类规则）", name, _rules.Count);
        }

        private string FormatRule(MatchRuleClassification rule)
        {
            string name = GetClassName(rule.ObjectId);
            string head = string.IsNullOrEmpty(name)
                ? string.Format("object_id {0}", rule.ObjectId)
                : string.Format("{0}（{1}）", name, rule.ObjectId);
            if (rule.Fields == null || rule.Fields.Count == 0)
                return head + " — [无字段]";
            return head + " — " + string.Join(", ", rule.Fields);
        }

        private string GetClassName(long objectId)
        {
            foreach (var c in _classifications)
                if (c.ObjectId == objectId)
                    return c.Name;
            return string.Empty;
        }

        private int FindClassificationIndex(long objectId)
        {
            for (int i = 0; i < _classifications.Count; i++)
                if (_classifications[i].ObjectId == objectId)
                    return i;
            return -1;
        }

        private void RefreshRuleList()
        {
            _refreshing = true;
            int sel = _lstRules.SelectedIndex;
            int top = _lstRules.TopIndex;   // 重建前记录滚动位置，避免切换分类后列表跳回顶部
            _lstRules.BeginUpdate();
            _lstRules.Items.Clear();
            foreach (var r in _rules)
                _lstRules.Items.Add(FormatRule(r));
            _lstRules.EndUpdate();
            if (sel >= 0 && sel < _rules.Count)
                _lstRules.SelectedIndex = sel;
            else if (_rules.Count > 0)
                _lstRules.SelectedIndex = 0;
            // 保持原滚动位置（仅当选中的分类原本就在可视区，避免强制拉回顶部导致选中项不可见）
            if (sel >= top && top < _lstRules.Items.Count)
                _lstRules.TopIndex = top;
            _refreshing = false;
        }

        /// <summary>重算「被其他规则占用的分类」集合，并触发 _cmbClass 重绘（OwnerDraw 下灰化禁用项）。
        /// 排除当前正在编辑的规则（草稿或已暂存）自身，使其分类始终可选。</summary>
        private void RefreshClassDisabledState()
        {
            _disabledClassObjectIds.Clear();
            long currentOid = CurrentRule != null ? CurrentRule.ObjectId : 0L;
            for (int i = 0; i < _rules.Count; i++)
            {
                long oid = _rules[i].ObjectId;
                if (oid > 0 && oid != currentOid)
                    _disabledClassObjectIds.Add(oid);
            }
            if (_cmbClass.IsHandleCreated)
                _cmbClass.Invalidate();
        }

        /// <summary>将临时目录中「预保存未落库」的暂存规则合并进当前 _rules：按 ObjectId 去重，暂存项(IsNew=true)覆盖同名项，
        /// 实现程序退出后重新打开时预保存数据自动恢复并仍显示绿色高亮。</summary>
        private void MergeStagedIntoRules()
        {
            var staged = MatchRuleStaging.LoadStaged(_currentKey);
            if (staged == null || staged.Count == 0) return;
            var existing = new HashSet<long>();
            foreach (var r in _rules)
                existing.Add(r.ObjectId);
            foreach (var s in staged)
            {
                if (existing.Contains(s.ObjectId))
                {
                    int idx = _rules.FindIndex(r => r.ObjectId == s.ObjectId);
                    if (idx >= 0) _rules[idx] = s;   // 暂存项优先（含最新编辑的字段）
                }
                else
                {
                    _rules.Add(s);
                }
            }
        }

        /// <summary>自绘 _cmbClass：被其他规则占用的分类画成灰色禁用（悬停也保持浅底灰字，避免高亮底色使其“变黑”）；当前选中项（即便 ObjectId 在禁用集）仍正常显示；启用项走系统高亮（悬停=蓝底白字）。</summary>
        private void _cmbClass_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool disabled = _disabledClassObjectIds.Contains(_classifications[e.Index].ObjectId)
                            && e.Index != _cmbClass.SelectedIndex;

            if (disabled)
            {
                // 禁用项：始终是“浅色背景 + 灰色文字”，绝不画系统高亮背景（否则悬停时深蓝高亮叠灰字会发黑）
                using (var backBrush = new SolidBrush(SystemColors.Window))
                    e.Graphics.FillRectangle(backBrush, e.Bounds);
                using (var brush = new SolidBrush(SystemColors.GrayText))
                    e.Graphics.DrawString(_cmbClass.Items[e.Index].ToString(), e.Font, brush, e.Bounds);
                return;
            }

            // 启用项：交给系统绘制（悬停/选中=高亮蓝底），文字用高亮文字色/窗口文字色
            e.DrawBackground();
            bool isSelected = (e.State & DrawItemState.Selected) != 0;
            Color textColor = isSelected ? SystemColors.HighlightText : SystemColors.WindowText;
            using (var brush = new SolidBrush(textColor))
                e.Graphics.DrawString(_cmbClass.Items[e.Index].ToString(), e.Font, brush, e.Bounds);
            if (isSelected) e.DrawFocusRectangle();
        }

        /// <summary>自绘左侧「制造件分类规则」列表：预保存项(IsNew)以浅绿底+深绿字高亮；选中项走系统高亮；其余正常。</summary>
        private void _lstRules_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _rules.Count) return;
            var rule = _rules[e.Index];
            bool selected = (e.State & DrawItemState.Selected) != 0;

            Color backColor, textColor;
            if (selected)
            {
                backColor = SystemColors.Highlight;
                textColor = SystemColors.HighlightText;
            }
            else if (rule.IsNew)
            {
                backColor = Color.LightGreen;   // 已预保存（待保存）高亮
                textColor = Color.DarkGreen;
            }
            else
            {
                backColor = SystemColors.Window;
                textColor = SystemColors.WindowText;
            }

            using (var bb = new SolidBrush(backColor))
                e.Graphics.FillRectangle(bb, e.Bounds);
            using (var tb = new SolidBrush(textColor))
                e.Graphics.DrawString(_lstRules.Items[e.Index].ToString(), e.Font, tb, e.Bounds);
            if (selected) e.DrawFocusRectangle();
        }

        private void _lstRules_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_refreshing) return;
            EnsureDraftCommitted();   // 切走前自动预保存未提交的草稿，避免属性白选丢失
            RefreshRuleList();
            _selectedIndex = _lstRules.SelectedIndex;
            _draft = null;
            LoadRuleToPanel();
        }

        /// <summary>若当前右侧是未预保存的草稿，则自动提交到暂存列表（_rules）并标记为已预保存高亮。已暂存项不处理。</summary>
        private void EnsureDraftCommitted()
        {
            if (_draft == null) return;
            _draft.IsNew = true;
            _rules.Add(_draft);
            _selectedIndex = _rules.Count - 1;
            _draft = null;
            MatchRuleStaging.SaveStaged(_currentKey, _rules);   // 草稿自动提交后也持久化，确保退出可恢复
        }

        private void _lstRules_MouseMove(object sender, MouseEventArgs e)
        {
            int idx = _lstRules.IndexFromPoint(e.Location);
            if (idx >= 0 && idx < _rules.Count)
            {
                string tip = string.Join(", ", _rules[idx].Fields);
                if (string.IsNullOrEmpty(tip)) tip = "[无字段]";
                _toolTip.SetToolTip(_lstRules, tip);
            }
            else
            {
                _toolTip.SetToolTip(_lstRules, string.Empty);
            }
        }

        private void LoadRuleToPanel()
        {
            _refreshing = true;
            int ci;
            var rule = CurrentRule;
            if (rule != null)
            {
                ci = FindClassificationIndex(rule.ObjectId);
                _cmbClass.SelectedIndex = ci;
                _chkStrict.Checked = rule.StrictMatchFlag;
                _selectedFields = new List<string>(rule.Fields);
            }
            else
            {
                ci = -1;
                _cmbClass.SelectedIndex = -1;
                _chkStrict.Checked = true;
                _selectedFields = new List<string>();
            }
            _lastValidClassIndex = ci;
            // 刷新当前分类的表单字段集合，供右侧列表“表单属性优先 + 候选集补充”使用
            if (rule != null && rule.ObjectId > 0)
                RefreshFormFields(rule.ObjectId);
            else
                _currentFormFields.Clear();
            RebuildFieldsList();
            _refreshing = false;
            RefreshClassDisabledState();
        }

        private void RebuildFieldsList()
        {
            _clbFields.BeginUpdate();
            _clbFields.Items.Clear();
            _clbFieldNames.Clear();

            string kw = (_txtSearch.Text ?? string.Empty).Trim();
            string kwLower = kw.ToLowerInvariant();

            // 合并候选集：元模型属性 + 本分类表单中出现但不在元模型的字段 + 已勾选但两者都查不到的游离字段。
            // 保证“表单内的字段”一定出现在列表里、可被勾选（之前因不在 meta_element 中而找不到）。
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var merged = new List<MetaPropertyOption>();
            foreach (var p in _allProps)
                if (seen.Add(p.InternalName)) merged.Add(p);
            foreach (var f in _currentFormFields)
                if (seen.Add(f)) merged.Add(new MetaPropertyOption { InternalName = f, DisplayName = null, IsSystemDefault = false });
            foreach (var s in _selectedFields)
                if (seen.Add(s)) merged.Add(new MetaPropertyOption { InternalName = s, DisplayName = null, IsSystemDefault = false });

            // 排序优先级（用户要求）：①已勾选 > ②本分类表单中的属性 > ③其余（保持原默认序：用户自定义优先、再按名称升序）
            var ordered = merged
                .OrderBy(p => _selectedFields.Contains(p.InternalName) ? 0
                             : (_currentFormFields.Contains(p.InternalName) ? 1 : 2))
                .ThenBy(p => p.IsSystemDefault ? 1 : 0)
                .ThenBy(p => p.InternalName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var p in ordered)
            {
                if (kw.Length > 0)
                {
                    bool hit = p.InternalName.ToLowerInvariant().Contains(kwLower)
                        || (p.DisplayName ?? string.Empty).IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!hit) continue;
                }

                string label = string.IsNullOrEmpty(p.DisplayName) || p.DisplayName == p.InternalName
                    ? p.InternalName
                    : string.Format("{0}  ({1})", p.InternalName, p.DisplayName);

                int idx = _clbFields.Items.Add(label);
                _clbFieldNames.Add(p.InternalName);
                _clbFields.SetItemChecked(idx, _selectedFields.Contains(p.InternalName));
            }
            _clbFields.EndUpdate();
        }

        private void ApplySelectedFieldsToRule()
        {
            var rule = CurrentRule;
            if (rule == null) return;
            rule.Fields = new List<string>(_selectedFields);
        }

        private void ApplyCurrentPanelToRule()
        {
            var rule = CurrentRule;
            if (rule == null) return;
            int ci = _cmbClass.SelectedIndex;
            if (ci >= 0 && ci < _classifications.Count)
                rule.ObjectId = _classifications[ci].ObjectId;
            rule.StrictMatchFlag = _chkStrict.Checked;
            rule.Fields = new List<string>(_selectedFields);
        }

        private void _txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_refreshing) return;
            RebuildFieldsList();
        }

        private void _clbFields_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_refreshing || CurrentRule == null) return;
            if (e.Index >= _clbFieldNames.Count) return;
            string name = _clbFieldNames[e.Index];
            if (e.NewValue == CheckState.Checked)
            {
                if (!_selectedFields.Contains(name))
                    _selectedFields.Add(name);
            }
            else
            {
                _selectedFields.Remove(name);
            }
            ApplySelectedFieldsToRule();
            _dirty = true;
        }

        private void _cmbClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_refreshing || _classifications.Count == 0) return;
            int ci = _cmbClass.SelectedIndex;
            if (ci < 0 || ci >= _classifications.Count) return;
            long clsId = _classifications[ci].ObjectId;

            // 阻止选到被其他已暂存规则占用的分类（已置灰项）：回退到上次合法选择，避免重复数据
            if (_disabledClassObjectIds.Contains(clsId))
            {
                _cmbClass.SelectedIndex = _lastValidClassIndex;
                return;
            }
            // 选中的就是当前正在编辑的分类（草稿或已暂存），无需重建
            if (CurrentRule != null && CurrentRule.ObjectId == clsId)
            {
                _lastValidClassIndex = ci;
                return;
            }

            // 切到新分类前，先把未预保存的草稿自动提交（避免属性白选丢失）
            EnsureDraftCommitted();
            RefreshRuleList();

            // 选了一个尚未设置匹配规则的分类：自动新建草稿并按表单模板预选属性（保持现有属性选择逻辑）
            _refreshing = true;
            _draft = new MatchRuleClassification { ObjectId = clsId, StrictMatchFlag = true, Fields = new List<string>() };
            _selectedIndex = -1;
            _lstRules.SelectedIndex = -1;
            _refreshing = false;
            LoadRuleToPanel();
            ApplyPreSelect(clsId);

            _lastValidClassIndex = ci;
            _dirty = true;
        }

        /// <summary>刷新「本分类表单字段」并返回 property 列表，合并进右侧匹配字段候选集：
        /// 表单里的字段不在 meta_element 元模型属性中也照样能被勾到。</summary>
        private List<string> RefreshFormFields(long clsId)
        {
            if (clsId <= 0) { _currentFormFields.Clear(); return new List<string>(); }
            try
            {
                var ff = _db.GetFormTemplateFieldsByClassification(clsId);
                _currentFormFields = new HashSet<string>(ff.Select(x => x.Property), StringComparer.OrdinalIgnoreCase);
                return ff.Select(x => x.Property).ToList();
            }
            catch
            {
                _currentFormFields.Clear();
                return new List<string>();
            }
        }

        /// <summary>选择分类后，按该分类表单模板字段预选（默认预选全部，用户「默认排除字段」配置中的字段除外）。</summary>
        private void ApplyPreSelect(long clsId)
        {
            try
            {
                var fields = RefreshFormFields(clsId);
                var excluded = MatchRuleExclusions.GetExcluded();
                _selectedFields = new List<string>();
                int excludedCount = 0;
                foreach (var f in fields)
                {
                    if (excluded.Contains(f))
                    {
                        excludedCount++;   // 按默认排除设置剔除
                        continue;
                    }
                    _selectedFields.Add(f);
                }
                ApplySelectedFieldsToRule();
                RebuildFieldsList();
                _lblStatus.Text = string.Format(
                    "已预选 {0} 个字段（按默认排除设置剔除 {1} 个）", _selectedFields.Count, excludedCount);
            }
            catch (Exception ex)
            {
                // 预选失败不阻断手动配置，但在状态栏提示具体错误，便于排查
                _lblStatus.Text = "预选失败：" + ex.Message;
            }
        }

        /// <summary>打开「默认排除字段」配置窗体。保存不关闭窗体，故订阅 ExclusionsSaved 事件实时刷新预选。</summary>
        private void _btnExcludeFields_Click(object sender, EventArgs e)
        {
            using (var f = new ExcludedFieldsForm(_db))
            {
                f.ExclusionsSaved += () =>
                {
                    if (_draft != null && _draft.ObjectId > 0)
                        ApplyPreSelect(_draft.ObjectId);   // 草稿分类即时按新排除规则刷新
                    else
                        _lblStatus.Text = "已更新默认排除字段，新建分类时自动生效";
                };
                f.ShowDialog(this);
            }
        }

        private void _chkStrict_CheckedChanged(object sender, EventArgs e)
        {
            if (_refreshing || CurrentRule == null) return;
            CurrentRule.StrictMatchFlag = _chkStrict.Checked;
            _dirty = true;
        }

        private void _btnSelectAll_Click(object sender, EventArgs e)
        {
            if (CurrentRule == null) return;
            for (int i = 0; i < _clbFieldNames.Count; i++)
            {
                _clbFields.SetItemChecked(i, true);
                if (!_selectedFields.Contains(_clbFieldNames[i]))
                    _selectedFields.Add(_clbFieldNames[i]);
            }
            ApplySelectedFieldsToRule();
            _dirty = true;
        }

        private void _btnClearFields_Click(object sender, EventArgs e)
        {
            if (CurrentRule == null) return;
            var remove = new List<string>();
            for (int i = 0; i < _clbFieldNames.Count; i++)
            {
                _clbFields.SetItemChecked(i, false);
                remove.Add(_clbFieldNames[i]);
            }
            foreach (var n in remove)
                _selectedFields.Remove(n);
            ApplySelectedFieldsToRule();
            _dirty = true;
        }

        private void _btnDelete_Click(object sender, EventArgs e)
        {
            // 取消「已预保存项」：仅对左侧暂存列表中的项生效；草稿不在列表内，无需删除
            if (_selectedIndex < 0 || _selectedIndex >= _rules.Count) return;
            _rules.RemoveAt(_selectedIndex);
            if (_rules.Count > 0)
            {
                if (_selectedIndex >= _rules.Count) _selectedIndex = _rules.Count - 1;
            }
            else
            {
                _selectedIndex = -1;
                _draft = null;
            }
            _refreshing = true;
            RefreshRuleList();
            _lstRules.SelectedIndex = (_rules.Count > 0) ? _selectedIndex : -1;
            _refreshing = false;
            RefreshClassDisabledState();
            LoadRuleToPanel();
            MatchRuleStaging.SaveStaged(_currentKey, _rules);   // 删除暂存项后同步临时目录
            _dirty = true;
        }

        /// <summary>预保存：将右侧当前规则提交到左侧暂存列表并高亮为已预保存。
        /// 草稿 → 加入 _rules（绿底高亮）；已暂存项 → 标记 IsNew=true（绿底高亮）。</summary>
        private void _btnPreSave_Click(object sender, EventArgs e)
        {
            if (CurrentRule == null) return;
            if (_draft != null)
            {
                _draft.IsNew = true;
                _rules.Add(_draft);
                _selectedIndex = _rules.Count - 1;
                _draft = null;
            }
            else
            {
                _rules[_selectedIndex].IsNew = true;
            }
            _refreshing = true;
            RefreshRuleList();
            _lstRules.SelectedIndex = _selectedIndex;
            _refreshing = false;
            RefreshClassDisabledState();
            MatchRuleStaging.SaveStaged(_currentKey, _rules);   // 预保存即写入临时目录（与默认排除字段同目录），退出可恢复
            _dirty = true;
            _lblStatus.Text = "已预保存（绿底高亮），待「保存到数据库」统一落库";
        }

        private void _btnLoad_Click(object sender, EventArgs e)
        {
            if (_dirty)
            {
                var r = MessageBox.Show("有未保存的改动，重新加载将丢弃这些改动，继续吗？",
                    "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes) return;
            }
            MatchRuleStaging.ClearStaged(_currentKey);   // 放弃改动，同步清掉对应类型的临时暂存
            LoadData();
            _dirty = false;
        }

        private void _btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void Save()
        {
            try
            {
                EnsureDraftCommitted();   // 保存前把未预保存草稿一起提交，避免丢失
                // 先将当前类型的规则暂存回根 JSON，再整体保存（保证所有类型独立落库）
                WriteCurrentRulesToRoot();

                JObject root = _originalRoot;
                if (root == null)
                    root = JObject.Parse(DefaultMatchRuleJson);

                string json = root.ToString(Formatting.None);
                _db.SaveMatchRuleContent("part", json, _ruleOwnerType, _ruleOwnerObjectId, _ruleVersion);
                _originalRoot = root;
                _dirty = false;
                // 落库后所有规则均已持久化，清除「未保存」标记使绿色高亮取消（绿色仅标识尚未保存数据库的数据）
                foreach (var r in _rules)
                    r.IsNew = false;
                MatchRuleStaging.ClearAllStaged();   // 全部落库，清掉临时暂存（避免下次打开重复还原）
                RefreshRuleList();
                MessageBox.Show("已保存到数据库。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatusForCurrentKey();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private JArray BuildClassificationsArray(List<MatchRuleClassification> rules)
        {
            var arr = new JArray();
            foreach (var r in rules)
            {
                var o = new JObject();
                o["object_id"] = r.ObjectId;
                o["strict_match_flag"] = r.StrictMatchFlag;
                var fs = new JArray();
                foreach (var f in r.Fields)
                    fs.Add(f);
                o["fields"] = fs;
                arr.Add(o);
            }
            return arr;
        }

        private void _btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_dirty)
            {
                var r = MessageBox.Show("有未保存的改动，确定关闭吗？",
                    "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes)
                    e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}