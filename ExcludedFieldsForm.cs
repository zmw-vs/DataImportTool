using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DataImportTool
{
    /// <summary>
    /// 「默认排除字段」配置窗体：展示当前所有属性（meta_element meta_type='property'），
    /// 支持搜索与勾选，勾选的字段即“默认排除字段”。保存后写入临时目录 JSON（跨程序退出保留）。
    /// 已勾选项默认排到列表最前，便于查看；点击「保存」不会关闭窗体，可连续保存。
    /// </summary>
    public partial class ExcludedFieldsForm : Form
    {
        private readonly DbHelper _db;
        private readonly HashSet<string> _excluded;   // 打开窗体时已有的排除集合（大小写不敏感），仅作初始值
        private HashSet<string> _checked;             // 当前会话勾选状态（大小写不敏感），保存时写入
        private List<MetaPropertyOption> _allProps = new List<MetaPropertyOption>();
        private List<string> _names = new List<string>();   // 与 _clb 项一一对应的 internal_name
        private bool _refreshing = false;

        /// <summary>每次点击「保存」成功写入后触发（即便窗体未关闭），供主窗体实时刷新预选。</summary>
        public event Action ExclusionsSaved;

        /// <summary>设计器专用无参构造：只做界面初始化，不访问数据库。</summary>
        public ExcludedFieldsForm()
        {
            _excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            InitializeComponent();
        }

        public ExcludedFieldsForm(DbHelper db)
            : this()
        {
            _db = db;
            foreach (string s in MatchRuleExclusions.GetExcluded())
                _excluded.Add(s);
            _checked = new HashSet<string>(_excluded, StringComparer.OrdinalIgnoreCase);
            LoadProps();
        }

        private void LoadProps()
        {
            try
            {
                _allProps = _db.GetMetaPropertyOptions() ?? new List<MetaPropertyOption>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载属性失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _allProps = new List<MetaPropertyOption>();
            }
            Rebuild();
        }

        private void Rebuild()
        {
            string kw = (_txtSearch.Text ?? string.Empty).Trim();
            string kwLower = kw.ToLowerInvariant();
            _refreshing = true;
            _clb.BeginUpdate();
            _clb.Items.Clear();
            _names.Clear();
            // 已勾选项默认排到最前，其余按 internal_name 升序
            var q = _allProps.AsEnumerable();
            if (kwLower.Length > 0)
                q = q.Where(p => (p.InternalName ?? string.Empty).ToLowerInvariant().Contains(kwLower)
                               || (p.DisplayName ?? string.Empty).IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);
            q = q.OrderBy(p => _checked.Contains(p.InternalName) ? 0 : 1)
                 .ThenBy(p => p.InternalName);
            foreach (var p in q)
            {
                string label = string.IsNullOrEmpty(p.DisplayName) || p.DisplayName == p.InternalName
                    ? p.InternalName
                    : string.Format("{0}  ({1})", p.InternalName, p.DisplayName);
                int idx = _clb.Items.Add(label);
                _names.Add(p.InternalName);
                _clb.SetItemChecked(idx, _checked.Contains(p.InternalName));
            }
            _clb.EndUpdate();
            _refreshing = false;
            _lblCount.Text = string.Format("已选 {0} / 共 {1}", CountChecked(), _allProps.Count);
        }

        private int CountChecked()
        {
            int c = 0;
            for (int i = 0; i < _clb.Items.Count; i++)
                if (_clb.GetItemChecked(i)) c++;
            return c;
        }

        private void _txtSearch_TextChanged(object sender, EventArgs e) { Rebuild(); }

        private void _clb_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_refreshing || e.Index < 0 || e.Index >= _names.Count) return;
            string name = _names[e.Index];
            if (e.NewValue == CheckState.Checked)
                _checked.Add(name);
            else
                _checked.Remove(name);
            // 勾选后立即重排：已勾选项置顶（BeginInvoke 规避 ItemCheck 内重建控件导致的重入）
            _clb.BeginInvoke(new Action(Rebuild));
        }

        private void SetAll(bool check)
        {
            _checked.Clear();
            if (check)
                foreach (var p in _allProps)
                    if (!string.IsNullOrWhiteSpace(p.InternalName))
                        _checked.Add(p.InternalName);
            Rebuild();
        }

        private void _btnAll_Click(object sender, EventArgs e) { SetAll(true); }

        private void _btnNone_Click(object sender, EventArgs e) { SetAll(false); }

        private void _btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void _btnSave_Click(object sender, EventArgs e)
        {
            // 保存后不关闭窗体，可连续调整并再次保存
            MatchRuleExclusions.Save(_checked);
            _lblStatus.Text = string.Format("已保存 {0} 个默认排除字段", _checked.Count);
            if (ExclusionsSaved != null)
                ExclusionsSaved();
        }
    }
}
