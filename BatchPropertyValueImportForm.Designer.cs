using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    public partial class BatchPropertyValueImportForm : Form
    {
        private System.ComponentModel.IContainer components = null;

        private Label _lblTitle;
        private Label _lblClassification;
        private ComboBox _cmbClassification;
        private Label _lblPropertyCount;
        private TextBox _txtPropertyList;
        private Panel _pnlListContainer;
        private Button _btnSelectExcel;
        private Button _btnImport;
        private Button _btnCancel;
        private ProgressBar _progressBar;
        private Label _lblProgress;
        private Label _lblStatus;
        private Label _lblResult;
        private TextBox _txtErrors;
        private OpenFileDialog _openFileDialog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblTitle = new Label();
            this._lblClassification = new Label();
            this._cmbClassification = new ComboBox();
            this._lblPropertyCount = new Label();
            this._txtPropertyList = new TextBox();
            this._pnlListContainer = new Panel();
            this._btnSelectExcel = new Button();
            this._btnImport = new Button();
            this._btnCancel = new Button();
            this._progressBar = new ProgressBar();
            this._lblProgress = new Label();
            this._lblStatus = new Label();
            this._lblResult = new Label();
            this._txtErrors = new TextBox();
            this._openFileDialog = new OpenFileDialog();
            this.SuspendLayout();
            // 
            // _lblTitle
            // 
            this._lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this._lblTitle.ForeColor = Color.FromArgb(64, 64, 64);
            this._lblTitle.Location = new Point(20, 20);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new Size(540, 30);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "批量属性值导入";
            this._lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lblClassification
            // 
            this._lblClassification.AutoSize = true;
            this._lblClassification.Font = new Font("Segoe UI", 10F);
            this._lblClassification.Location = new Point(20, 65);
            this._lblClassification.Name = "_lblClassification";
            this._lblClassification.Size = new Size(68, 19);
            this._lblClassification.TabIndex = 1;
            this._lblClassification.Text = "选择分类：";
            // 
            // _cmbClassification
            // 
            this._cmbClassification.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cmbClassification.Font = new Font("Segoe UI", 10F);
            this._cmbClassification.Location = new Point(90, 62);
            this._cmbClassification.Name = "_cmbClassification";
            this._cmbClassification.Size = new Size(350, 25);
            this._cmbClassification.TabIndex = 2;
            // 
            // _lblPropertyCount
            // 
            this._lblPropertyCount.AutoSize = true;
            this._lblPropertyCount.Font = new Font("Segoe UI", 10F);
            this._lblPropertyCount.Location = new Point(20, 102);
            this._lblPropertyCount.Name = "_lblPropertyCount";
            this._lblPropertyCount.Size = new Size(120, 19);
            this._lblPropertyCount.TabIndex = 3;
            this._lblPropertyCount.Text = "该分类下属性：";
            // 
            // _pnlListContainer
            // 
            this._pnlListContainer.BackColor = Color.FromArgb(248, 249, 250);
            this._pnlListContainer.BorderStyle = BorderStyle.FixedSingle;
            this._pnlListContainer.Location = new Point(20, 126);
            this._pnlListContainer.Name = "_pnlListContainer";
            this._pnlListContainer.Size = new Size(540, 120);
            this._pnlListContainer.TabIndex = 4;
            // 
            // _txtPropertyList
            // 
            this._txtPropertyList.BackColor = Color.FromArgb(248, 249, 250);
            this._txtPropertyList.BorderStyle = BorderStyle.None;
            this._txtPropertyList.Font = new Font("Segoe UI", 9F);
            this._txtPropertyList.Location = new Point(10, 10);
            this._txtPropertyList.Multiline = true;
            this._txtPropertyList.Name = "_txtPropertyList";
            this._txtPropertyList.ReadOnly = true;
            this._txtPropertyList.Size = new Size(515, 95);
            this._txtPropertyList.TabIndex = 5;
            this._txtPropertyList.Text = "请选择分类以查看属性";
            // 
            // _btnSelectExcel
            // 
            this._btnSelectExcel.BackColor = Color.FromArgb(0, 120, 215);
            this._btnSelectExcel.FlatAppearance.BorderSize = 0;
            this._btnSelectExcel.FlatStyle = FlatStyle.Flat;
            this._btnSelectExcel.Font = new Font("Segoe UI", 10F);
            this._btnSelectExcel.ForeColor = Color.White;
            this._btnSelectExcel.Location = new Point(20, 260);
            this._btnSelectExcel.Name = "_btnSelectExcel";
            this._btnSelectExcel.Size = new Size(200, 36);
            this._btnSelectExcel.TabIndex = 6;
            this._btnSelectExcel.Text = "选择 Excel 并导入";
            this._btnSelectExcel.UseVisualStyleBackColor = false;
            this._btnSelectExcel.Click += new System.EventHandler(this.BtnSelectExcel_Click);
            // 
            // _btnImport
            // 
            this._btnImport.BackColor = Color.FromArgb(40, 167, 69);
            this._btnImport.FlatAppearance.BorderSize = 0;
            this._btnImport.FlatStyle = FlatStyle.Flat;
            this._btnImport.Font = new Font("Segoe UI", 10F);
            this._btnImport.ForeColor = Color.White;
            this._btnImport.Location = new Point(230, 260);
            this._btnImport.Name = "_btnImport";
            this._btnImport.Size = new Size(160, 36);
            this._btnImport.TabIndex = 7;
            this._btnImport.Text = "开始导入";
            this._btnImport.UseVisualStyleBackColor = false;
            // 
            // _btnCancel
            // 
            this._btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            this._btnCancel.FlatAppearance.BorderSize = 0;
            this._btnCancel.FlatStyle = FlatStyle.Flat;
            this._btnCancel.Font = new Font("Segoe UI", 10F);
            this._btnCancel.ForeColor = Color.White;
            this._btnCancel.Location = new Point(400, 260);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new Size(160, 36);
            this._btnCancel.TabIndex = 8;
            this._btnCancel.Text = "取消";
            this._btnCancel.UseVisualStyleBackColor = false;
            this._btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // _progressBar
            // 
            this._progressBar.Location = new Point(20, 310);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new Size(540, 25);
            this._progressBar.Style = ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 9;
            // 
            // _lblProgress
            // 
            this._lblProgress.AutoSize = true;
            this._lblProgress.Font = new Font("Segoe UI", 9F);
            this._lblProgress.Location = new Point(20, 340);
            this._lblProgress.Name = "_lblProgress";
            this._lblProgress.Size = new Size(30, 15);
            this._lblProgress.TabIndex = 10;
            this._lblProgress.Text = "0/0";
            // 
            // _lblStatus
            // 
            this._lblStatus.AutoSize = true;
            this._lblStatus.Font = new Font("Segoe UI", 9F);
            this._lblStatus.ForeColor = Color.FromArgb(100, 100, 100);
            this._lblStatus.Location = new Point(200, 340);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new Size(30, 15);
            this._lblStatus.TabIndex = 11;
            this._lblStatus.Text = "";
            // 
            // _lblResult
            // 
            this._lblResult.AutoSize = true;
            this._lblResult.Font = new Font("Segoe UI", 10F);
            this._lblResult.Location = new Point(20, 360);
            this._lblResult.Name = "_lblResult";
            this._lblResult.Size = new Size(100, 19);
            this._lblResult.TabIndex = 12;
            this._lblResult.Text = "成功：0  失败：0";
            // 
            // _txtErrors
            // 
            this._txtErrors.BackColor = Color.FromArgb(255, 245, 245);
            this._txtErrors.Font = new Font("Consolas", 9F);
            this._txtErrors.Location = new Point(20, 385);
            this._txtErrors.Multiline = true;
            this._txtErrors.Name = "_txtErrors";
            this._txtErrors.ReadOnly = true;
            this._txtErrors.ScrollBars = ScrollBars.Vertical;
            this._txtErrors.Size = new Size(540, 100);
            this._txtErrors.TabIndex = 13;
            this._txtErrors.Text = "";
            this._txtErrors.Visible = false;
            // 
            // _openFileDialog
            // 
            this._openFileDialog.DefaultExt = "xlsx";
            this._openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
            this._openFileDialog.Title = "选择属性值导入 Excel";
            // 
            // BatchPropertyValueImportForm
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(580, 510);
            this.Controls.Add(this._txtErrors);
            this.Controls.Add(this._lblResult);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._lblProgress);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnImport);
            this.Controls.Add(this._btnSelectExcel);
            this._pnlListContainer.Controls.Add(this._txtPropertyList);
            this.Controls.Add(this._pnlListContainer);
            this.Controls.Add(this._lblPropertyCount);
            this.Controls.Add(this._cmbClassification);
            this.Controls.Add(this._lblClassification);
            this.Controls.Add(this._lblTitle);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "BatchPropertyValueImportForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "批量属性值导入";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
