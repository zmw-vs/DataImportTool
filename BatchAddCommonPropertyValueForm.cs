using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DataImportTool
{
    /// <summary>
    /// 批量追加通用属性值：给每个分类下的每个属性都挂同一个属性值，只写数据表，不动表单模板。
    /// 写 t_property_value（同文本只插一次，多个属性复用）+ t_property_to_property_value 建关联。
    /// 先预览再执行，已经挂过的属性跳过；后台线程跑，带进度条、可取消。
    /// </summary>
    public class BatchAddCommonPropertyValueForm : Form
    {
        // ====== 字段 ======
        private readonly DbHelper _db;
        private TextBox _txtValue;
        private TextBox _txtNum;
        private RadioButton _rbAll;
        private RadioButton _rbSpecified;
        private ComboBox _cmbCls;
        private CheckBox _chkSkip;
        private Button _btnPreview;
        private Button _btnExec;
        private Button _btnClose;
        private DataGridView _dgv;
        private ProgressBar _prog;
        private Label _lblStatus;
        private Label _lblTitle;
        private Label _lblVal;
        private Label _lblNum;
        private BackgroundWorker _bw;

        private List<PropValTask> _tasks = new List<PropValTask>();
        private bool _busy = false;

        // ====== 内部数据结构 ======
        private class PropValTask
        {
            public long ClsId;
            public string ClsName;
            public long PropId;
            public string PropName;
            public bool WillSkip;
        }

        private class ExecArg
        {
            public string ValueText;
            public string PropNum;
        }

        private class ExecResult
        {
            public bool Success;
            public string Message;
            public int Added;
            public int Skipped;
        }

        private class ProgressInfo
        {
            public int Added;
            public int Skipped;
            public int Done;
            public int Total;
        }

        // ====== 构造 ======
        public BatchAddCommonPropertyValueForm(DbHelper db)
        {
            _db = db;
            InitializeComponent();
            WireUpEvents();
            LoadClassifications();
        }

        private void LoadClassifications()
        {
            var list = _db.GetAllClassifications();
            _cmbCls.DisplayMember = "Name";
            _cmbCls.ValueMember = "ObjectId";
            _cmbCls.DataSource = list;
            _cmbCls.Enabled = false;
        }

        // ====== 事件绑定（显式 +=，禁用 lambda）======
        private void WireUpEvents()
        {
            _rbAll.CheckedChanged += ScopeChanged;
            _rbSpecified.CheckedChanged += ScopeChanged;
            _btnPreview.Click += BtnPreview_Click;
            _btnExec.Click += BtnExec_Click;
            _btnClose.Click += BtnClose_Click;
            _bw.DoWork += Bw_DoWork;
            _bw.ProgressChanged += Bw_ProgressChanged;
            _bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
        }

        private void ScopeChanged(object sender, EventArgs e)
        {
            _cmbCls.Enabled = _rbSpecified.Checked;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ====== 预览 ======
        private void BtnPreview_Click(object sender, EventArgs e)
        {
            if (_busy) return;
            string valueText = _txtValue.Text.Trim();
            if (string.IsNullOrEmpty(valueText))
            {
                MessageBox.Show("请先填写通用属性值文本。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _tasks = BuildTasks(valueText);
            _dgv.DataSource = null;
            _dgv.DataSource = _tasks.Select(t => new
            {
                分类 = t.ClsName,
                属性 = t.PropName,
                状态 = t.WillSkip ? "已存在(跳过)" : "将新增"
            }).ToList();

            int addCount = _tasks.Count(t => !t.WillSkip);
            int skipCount = _tasks.Count - addCount;
            _lblStatus.Text = string.Format("预览完成：共 {0} 个属性，将新增 {1} 个，跳过 {2} 个。",
                _tasks.Count, addCount, skipCount);
            _btnExec.Enabled = addCount > 0;
        }

        private List<PropValTask> BuildTasks(string valueText)
        {
            var result = new List<PropValTask>();
            List<ClassificationGroup> clsList;
            if (_rbSpecified.Checked)
            {
                if (_cmbCls.SelectedItem == null)
                {
                    MessageBox.Show("请先选择一个指定分类。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return result;
                }
                clsList = new List<ClassificationGroup> { (ClassificationGroup)_cmbCls.SelectedItem };
            }
            else
            {
                // 从 t_property 的去重 CLS_ID 取"实际有属性的分类"；不从 t_object_classification 全表遍历（那边多数分类查不出属性）
                clsList = _db.GetClsIdsHavingProperties();
            }

            foreach (var cls in clsList)
            {
                // 仅按 CLS_ID 过滤，去掉 CLS_INTERNAL_NAME 非空限制，覆盖 CLS_ID 有值但 CLS_INTERNAL_NAME 为空的数据
                var props = _db.GetPropertiesByClsId(cls.ObjectId);
                foreach (var prop in props)
                {
                    bool linked = _db.PropertyValueTextLinkedToProperty(prop.ObjectId, valueText);
                    result.Add(new PropValTask
                    {
                        ClsId = cls.ObjectId,
                        ClsName = cls.Name,
                        PropId = prop.ObjectId,
                        PropName = prop.Name,
                        WillSkip = linked
                    });
                }
            }
            return result;
        }

        // ====== 执行 ======
        private void BtnExec_Click(object sender, EventArgs e)
        {
            if (_busy) return;
            if (_tasks == null || _tasks.Count == 0)
            {
                BtnPreview_Click(sender, e);
                if (_tasks == null || _tasks.Count == 0) return;
            }

            string valueText = _txtValue.Text.Trim();
            if (string.IsNullOrEmpty(valueText)) return;
            string propNum = _txtNum.Text.Trim();

            _busy = true;
            SetControlsEnabled(false);
            _prog.Value = 0;
            _lblStatus.Text = "正在执行...";
            _btnExec.Enabled = false;
            _bw.RunWorkerAsync(new ExecArg { ValueText = valueText, PropNum = propNum });
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var arg = (ExecArg)e.Argument;
            int total = _tasks.Count;
            int done = 0;
            int added = 0;
            int skipped = 0;

            // 值级别复用：先查是否已有该值文本，避免 t_property_value 堆重复行
            long sharedValId = _db.FindPropertyValueByText(arg.ValueText);

            foreach (var t in _tasks)
            {
                if (_bw.CancellationPending) { e.Cancel = true; break; }
                try
                {
                    if (_chkSkip.Checked && t.WillSkip)
                    {
                        skipped++;
                    }
                    else
                    {
                        long pvId = sharedValId;
                        if (pvId == 0)
                        {
                            pvId = _db.InsertPropertyValue(arg.PropNum, arg.ValueText);
                            sharedValId = pvId; // 首个创建后，后续属性复用同一值 OBJECT_ID
                        }
                        if (!_db.PropertyValueTextLinkedToProperty(t.PropId, arg.ValueText))
                        {
                            _db.InsertPropertyToPropertyValue(t.PropId, pvId);
                        }
                        added++;
                    }
                }
                catch (Exception ex)
                {
                    e.Result = new ExecResult
                    {
                        Success = false,
                        Message = "属性[" + t.PropName + "]出错：" + ex.Message,
                        Added = added,
                        Skipped = skipped
                    };
                    return;
                }

                done++;
                int pct = total == 0 ? 100 : (done * 100 / total);
                _bw.ReportProgress(pct, new ProgressInfo { Added = added, Skipped = skipped, Done = done, Total = total });
            }

            e.Result = new ExecResult { Success = true, Added = added, Skipped = skipped };
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _prog.Value = e.ProgressPercentage;
            var info = (ProgressInfo)e.UserState;
            _lblStatus.Text = string.Format("进度 {0}/{1}  新增 {2}  跳过 {3}",
                info.Done, info.Total, info.Added, info.Skipped);
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _busy = false;
            _btnExec.Enabled = true;
            SetControlsEnabled(true);

            if (e.Cancelled)
            {
                _lblStatus.Text = "已取消（已处理部分已写入）。";
                MessageBox.Show("操作已取消。已处理的部分已写入数据库。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (e.Error != null)
            {
                _lblStatus.Text = "执行出错：" + e.Error.Message;
                MessageBox.Show("执行出错：" + e.Error.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var r = (ExecResult)e.Result;
                _lblStatus.Text = string.Format("完成：新增 {0}，跳过 {1}。", r.Added, r.Skipped);
                MessageBox.Show(string.Format("执行完成。\n新增关联 {0} 个，跳过 {1} 个。", r.Added, r.Skipped),
                    "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BtnPreview_Click(null, null); // 刷新预览，使已处理的显示"已存在(跳过)"
            }
            _prog.Value = _prog.Maximum;
        }

        private void SetControlsEnabled(bool enabled)
        {
            _txtValue.Enabled = enabled;
            _txtNum.Enabled = enabled;
            _rbAll.Enabled = enabled;
            _rbSpecified.Enabled = enabled;
            _cmbCls.Enabled = enabled && _rbSpecified.Checked;
            _chkSkip.Enabled = enabled;
            _btnPreview.Enabled = enabled;
            _btnClose.Enabled = enabled;
        }

        // ====== 手写 UI（四处齐全：new + 属性 + Controls.Add + 字段）======
        private void InitializeComponent()
        {
            this._lblTitle = new System.Windows.Forms.Label();
            this._lblVal = new System.Windows.Forms.Label();
            this._txtValue = new System.Windows.Forms.TextBox();
            this._lblNum = new System.Windows.Forms.Label();
            this._txtNum = new System.Windows.Forms.TextBox();
            this._rbAll = new System.Windows.Forms.RadioButton();
            this._rbSpecified = new System.Windows.Forms.RadioButton();
            this._cmbCls = new System.Windows.Forms.ComboBox();
            this._chkSkip = new System.Windows.Forms.CheckBox();
            this._btnPreview = new System.Windows.Forms.Button();
            this._btnExec = new System.Windows.Forms.Button();
            this._btnClose = new System.Windows.Forms.Button();
            this._dgv = new System.Windows.Forms.DataGridView();
            this._prog = new System.Windows.Forms.ProgressBar();
            this._lblStatus = new System.Windows.Forms.Label();
            this._bw = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this._dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // _lblTitle
            // 
            this._lblTitle.Location = new System.Drawing.Point(16, 14);
            this._lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new System.Drawing.Size(821, 37);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "为每个分类下的每个属性，批量追加同一个通用属性值（仅写数据表，不同步表单模板）。";
            // 
            // _lblVal
            // 
            this._lblVal.AutoSize = true;
            this._lblVal.Location = new System.Drawing.Point(16, 65);
            this._lblVal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblVal.Name = "_lblVal";
            this._lblVal.Size = new System.Drawing.Size(127, 15);
            this._lblVal.TabIndex = 1;
            this._lblVal.Text = "通用属性值文本：";
            // 
            // _txtValue
            // 
            this._txtValue.Location = new System.Drawing.Point(187, 61);
            this._txtValue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._txtValue.Name = "_txtValue";
            this._txtValue.Size = new System.Drawing.Size(617, 25);
            this._txtValue.TabIndex = 2;
            // 
            // _lblNum
            // 
            this._lblNum.AutoSize = true;
            this._lblNum.Location = new System.Drawing.Point(16, 97);
            this._lblNum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblNum.Name = "_lblNum";
            this._lblNum.Size = new System.Drawing.Size(128, 15);
            this._lblNum.TabIndex = 3;
            this._lblNum.Text = "属性编号(可选)：";
            // 
            // _txtNum
            // 
            this._txtNum.Location = new System.Drawing.Point(187, 93);
            this._txtNum.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._txtNum.Name = "_txtNum";
            this._txtNum.Size = new System.Drawing.Size(292, 25);
            this._txtNum.TabIndex = 4;
            // 
            // _rbAll
            // 
            this._rbAll.AutoSize = true;
            this._rbAll.Checked = true;
            this._rbAll.Location = new System.Drawing.Point(16, 134);
            this._rbAll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._rbAll.Name = "_rbAll";
            this._rbAll.Size = new System.Drawing.Size(88, 19);
            this._rbAll.TabIndex = 0;
            this._rbAll.TabStop = true;
            this._rbAll.Text = "全部分类";
            this._rbAll.UseVisualStyleBackColor = true;
            // 
            // _rbSpecified
            // 
            this._rbSpecified.AutoSize = true;
            this._rbSpecified.Location = new System.Drawing.Point(187, 134);
            this._rbSpecified.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._rbSpecified.Name = "_rbSpecified";
            this._rbSpecified.Size = new System.Drawing.Size(88, 19);
            this._rbSpecified.TabIndex = 1;
            this._rbSpecified.Text = "指定分类";
            this._rbSpecified.UseVisualStyleBackColor = true;
            // 
            // _cmbCls
            // 
            this._cmbCls.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbCls.FormattingEnabled = true;
            this._cmbCls.Location = new System.Drawing.Point(325, 130);
            this._cmbCls.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._cmbCls.Name = "_cmbCls";
            this._cmbCls.Size = new System.Drawing.Size(479, 23);
            this._cmbCls.TabIndex = 5;
            // 
            // _chkSkip
            // 
            this._chkSkip.AutoSize = true;
            this._chkSkip.Checked = true;
            this._chkSkip.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkSkip.Location = new System.Drawing.Point(16, 168);
            this._chkSkip.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._chkSkip.Name = "_chkSkip";
            this._chkSkip.Size = new System.Drawing.Size(165, 19);
            this._chkSkip.TabIndex = 2;
            this._chkSkip.Text = "已存在则跳过(推荐)";
            this._chkSkip.UseVisualStyleBackColor = true;
            // 
            // _btnPreview
            // 
            this._btnPreview.Location = new System.Drawing.Point(16, 208);
            this._btnPreview.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnPreview.Name = "_btnPreview";
            this._btnPreview.Size = new System.Drawing.Size(160, 32);
            this._btnPreview.TabIndex = 3;
            this._btnPreview.Text = "预览";
            this._btnPreview.UseVisualStyleBackColor = true;
            // 
            // _btnExec
            // 
            this._btnExec.Enabled = false;
            this._btnExec.Location = new System.Drawing.Point(200, 208);
            this._btnExec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnExec.Name = "_btnExec";
            this._btnExec.Size = new System.Drawing.Size(160, 32);
            this._btnExec.TabIndex = 4;
            this._btnExec.Text = "执行";
            this._btnExec.UseVisualStyleBackColor = true;
            // 
            // _btnClose
            // 
            this._btnClose.Location = new System.Drawing.Point(704, 208);
            this._btnClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(133, 32);
            this._btnClose.TabIndex = 5;
            this._btnClose.Text = "关闭";
            this._btnClose.UseVisualStyleBackColor = true;
            // 
            // _dgv
            // 
            this._dgv.AllowUserToAddRows = false;
            this._dgv.AllowUserToDeleteRows = false;
            this._dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dgv.Location = new System.Drawing.Point(16, 254);
            this._dgv.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._dgv.Name = "_dgv";
            this._dgv.ReadOnly = true;
            this._dgv.RowHeadersVisible = false;
            this._dgv.RowHeadersWidth = 51;
            this._dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._dgv.Size = new System.Drawing.Size(821, 300);
            this._dgv.TabIndex = 6;
            // 
            // _prog
            // 
            this._prog.Location = new System.Drawing.Point(16, 572);
            this._prog.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._prog.Name = "_prog";
            this._prog.Size = new System.Drawing.Size(661, 23);
            this._prog.TabIndex = 7;
            // 
            // _lblStatus
            // 
            this._lblStatus.AutoSize = true;
            this._lblStatus.Location = new System.Drawing.Point(16, 605);
            this._lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(247, 15);
            this._lblStatus.TabIndex = 8;
            this._lblStatus.Text = "填写通用属性值文本后点「预览」。";
            // 
            // _bw
            // 
            this._bw.WorkerReportsProgress = true;
            this._bw.WorkerSupportsCancellation = true;
            this._bw.DoWork += new System.ComponentModel.DoWorkEventHandler(this._bw_DoWork);
            // 
            // BatchAddCommonPropertyValueForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(853, 646);
            this.Controls.Add(this._lblTitle);
            this.Controls.Add(this._lblVal);
            this.Controls.Add(this._txtValue);
            this.Controls.Add(this._lblNum);
            this.Controls.Add(this._txtNum);
            this.Controls.Add(this._rbAll);
            this.Controls.Add(this._rbSpecified);
            this.Controls.Add(this._cmbCls);
            this.Controls.Add(this._chkSkip);
            this.Controls.Add(this._btnPreview);
            this.Controls.Add(this._btnExec);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._dgv);
            this.Controls.Add(this._prog);
            this.Controls.Add(this._lblStatus);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "BatchAddCommonPropertyValueForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "批量追加通用属性值";
            ((System.ComponentModel.ISupportInitialize)(this._dgv)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void _bw_DoWork(object sender, DoWorkEventArgs e)
        {

        }
    }
}
