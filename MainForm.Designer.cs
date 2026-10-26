using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataImportTool
{
    partial class MainForm
    {
        // ══════════════════════════════════════════════════════════════
        // 字段声明（必须严格按使用顺序排列）
        // ══════════════════════════════════════════════════════════════

        private IContainer components = null;

        // 状态栏
        private Label _lblStatusDb;
        private Label _lblStatusInfo;
        private Label _lblVersion;

        // 卡片1: 分类组导入
        private Panel _pnlCard1;
        private Panel _card1Bar;
        private Label _card1Icon;
        private Label _card1Title;
        private Label _card1Desc;

        // 卡片2: 分类插入
        private Panel _pnlCard2;
        private Panel _card2Bar;
        private Label _card2Icon;
        private Label _card2Title;
        private Label _card2Desc;


        // 卡片4: 插入属性
        private Panel _pnlCard4;
        private Panel _card4Bar;
        private Label _card4Icon;
        private Label _card4Title;
        private Label _card4Desc;

        // 卡片5: 属性值导入
        private Panel _pnlCard5;
        private Panel _card5Bar;
        private Label _card5Icon;
        private Label _card5Title;
        private Label _card5Desc;

        // 卡片6: 设置
        private Panel _pnlCard6;
        private Panel _card6Bar;
        private Label _card6Icon;
        private Label _card6Title;
        private Label _card6Desc;

        // 卡片7: 属性值批量导入
        private Panel _pnlCard7;
        private Panel _card7Bar;
        private Label _card7Icon;
        private Label _card7Title;
        private Label _card7Desc;

        // 卡片9: 批量导入
        private Panel _pnlCard9;
        private Panel _card9Bar;
        private Label _card9Icon;
        private Label _card9Title;
        private Label _card9Desc;
        // 卡片8: 超级批量导入
        private Panel _pnlCard8;
        private Panel _card8Bar;
        private Label _card8Icon;
        private Label _card8Title;
        private Label _card8Desc;

        // 卡片10: 去除工作流模板脚本
        private Panel _pnlCard10;
        private Panel _card10Bar;
        private Label _card10Icon;
        private Label _card10Title;
        private Label _card10Desc;

        // 卡片11: 查看日志
        private Panel _pnlCard11;
        private Panel _card11Bar;
        private Label _card11Icon;
        private Label _card11Title;
        private Label _card11Desc;

        // ══════════════════════════════════════════════════════════════
        // Designer 生成代码
        // ══════════════════════════════════════════════════════════════
        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this._lblStatusDb = new System.Windows.Forms.Label();
            this._lblStatusInfo = new System.Windows.Forms.Label();
            this._lblVersion = new System.Windows.Forms.Label();
            this._pnlCard1 = new System.Windows.Forms.Panel();
            this._card1Bar = new System.Windows.Forms.Panel();
            this._card1Icon = new System.Windows.Forms.Label();
            this._card1Title = new System.Windows.Forms.Label();
            this._card1Desc = new System.Windows.Forms.Label();
            this._pnlCard2 = new System.Windows.Forms.Panel();
            this._card2Bar = new System.Windows.Forms.Panel();
            this._card2Icon = new System.Windows.Forms.Label();
            this._card2Title = new System.Windows.Forms.Label();
            this._card2Desc = new System.Windows.Forms.Label();
            this._pnlCard4 = new System.Windows.Forms.Panel();
            this._card4Bar = new System.Windows.Forms.Panel();
            this._card4Icon = new System.Windows.Forms.Label();
            this._card4Title = new System.Windows.Forms.Label();
            this._card4Desc = new System.Windows.Forms.Label();
            this._pnlCard5 = new System.Windows.Forms.Panel();
            this._card5Title = new System.Windows.Forms.Label();
            this._card5Bar = new System.Windows.Forms.Panel();
            this._card5Icon = new System.Windows.Forms.Label();
            this._card5Desc = new System.Windows.Forms.Label();
            this._pnlCard6 = new System.Windows.Forms.Panel();
            this._card6Bar = new System.Windows.Forms.Panel();
            this._card6Icon = new System.Windows.Forms.Label();
            this._card6Title = new System.Windows.Forms.Label();
            this._card6Desc = new System.Windows.Forms.Label();
            this._pnlCard7 = new System.Windows.Forms.Panel();
            this._card7Bar = new System.Windows.Forms.Panel();
            this._card7Icon = new System.Windows.Forms.Label();
            this._card7Title = new System.Windows.Forms.Label();
            this._card7Desc = new System.Windows.Forms.Label();
            this._pnlCard9 = new System.Windows.Forms.Panel();
            this._card9Bar = new System.Windows.Forms.Panel();
            this._card9Icon = new System.Windows.Forms.Label();
            this._card9Title = new System.Windows.Forms.Label();
            this._card9Desc = new System.Windows.Forms.Label();
            this._pnlCard8 = new System.Windows.Forms.Panel();
            this._card8Bar = new System.Windows.Forms.Panel();
            this._card8Icon = new System.Windows.Forms.Label();
            this._card8Title = new System.Windows.Forms.Label();
            this._card8Desc = new System.Windows.Forms.Label();
            this._pnlCard10 = new System.Windows.Forms.Panel();
            this._card10Bar = new System.Windows.Forms.Panel();
            this._card10Icon = new System.Windows.Forms.Label();
            this._card10Title = new System.Windows.Forms.Label();
            this._card10Desc = new System.Windows.Forms.Label();
            this._pnlCard11 = new System.Windows.Forms.Panel();
            this._card11Bar = new System.Windows.Forms.Panel();
            this._card11Icon = new System.Windows.Forms.Label();
            this._card11Title = new System.Windows.Forms.Label();
            this._card11Desc = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this._pnlCard1.SuspendLayout();
            this._pnlCard2.SuspendLayout();
            this._pnlCard4.SuspendLayout();
            this._pnlCard5.SuspendLayout();
            this._pnlCard6.SuspendLayout();
            this._pnlCard7.SuspendLayout();
            this._pnlCard9.SuspendLayout();
            this._pnlCard8.SuspendLayout();
            this._pnlCard10.SuspendLayout();
            this._pnlCard11.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblStatusDb
            // 
            this._lblStatusDb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._lblStatusDb.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this._lblStatusDb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblStatusDb.Location = new System.Drawing.Point(8, 599);
            this._lblStatusDb.Name = "_lblStatusDb";
            this._lblStatusDb.Size = new System.Drawing.Size(114, 20);
            this._lblStatusDb.TabIndex = 14;
            this._lblStatusDb.Text = "状态：未知";
            this._lblStatusDb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblStatusInfo
            // 
            this._lblStatusInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._lblStatusInfo.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this._lblStatusInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(80)))), ((int)(((byte)(87)))));
            this._lblStatusInfo.Location = new System.Drawing.Point(294, 599);
            this._lblStatusInfo.Name = "_lblStatusInfo";
            this._lblStatusInfo.Size = new System.Drawing.Size(400, 20);
            this._lblStatusInfo.TabIndex = 15;
            this._lblStatusInfo.Text = "版本信息";
            this._lblStatusInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblVersion
            // 
            this._lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._lblVersion.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this._lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._lblVersion.Location = new System.Drawing.Point(946, 599);
            this._lblVersion.Name = "_lblVersion";
            this._lblVersion.Size = new System.Drawing.Size(100, 20);
            this._lblVersion.TabIndex = 16;
            this._lblVersion.Text = "v1.1";
            this._lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _pnlCard1
            // 
            this._pnlCard1.BackColor = System.Drawing.Color.White;
            this._pnlCard1.Controls.Add(this._card1Bar);
            this._pnlCard1.Controls.Add(this._card1Icon);
            this._pnlCard1.Controls.Add(this._card1Title);
            this._pnlCard1.Controls.Add(this._card1Desc);
            this._pnlCard1.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlCard1.Location = new System.Drawing.Point(16, 24);
            this._pnlCard1.Name = "_pnlCard1";
            this._pnlCard1.Size = new System.Drawing.Size(280, 57);
            this._pnlCard1.TabIndex = 6;
            this._pnlCard1.Click += new System.EventHandler(this.btnGroupImport_Click);
            this._pnlCard1.Paint += new System.Windows.Forms.PaintEventHandler(this._pnlCard1_Paint);
            this._pnlCard1.MouseEnter += new System.EventHandler(this.Card_MouseEnter);
            this._pnlCard1.MouseLeave += new System.EventHandler(this.Card_MouseLeave);
            // 
            // _card1Bar
            // 
            this._card1Bar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this._card1Bar.Dock = System.Windows.Forms.DockStyle.Left;
            this._card1Bar.Location = new System.Drawing.Point(0, 0);
            this._card1Bar.Name = "_card1Bar";
            this._card1Bar.Size = new System.Drawing.Size(4, 57);
            this._card1Bar.TabIndex = 0;
            // 
            // _card1Icon
            // 
            this._card1Icon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._card1Icon.Location = new System.Drawing.Point(9, 1);
            this._card1Icon.Name = "_card1Icon";
            this._card1Icon.Size = new System.Drawing.Size(30, 33);
            this._card1Icon.TabIndex = 1;
            this._card1Icon.Text = "📁";
            this._card1Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _card1Title
            // 
            this._card1Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._card1Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._card1Title.Location = new System.Drawing.Point(43, 5);
            this._card1Title.Name = "_card1Title";
            this._card1Title.Size = new System.Drawing.Size(103, 27);
            this._card1Title.TabIndex = 2;
            this._card1Title.Text = "分类组导入";
            // 
            // _card1Desc
            // 
            this._card1Desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._card1Desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card1Desc.Location = new System.Drawing.Point(42, 29);
            this._card1Desc.Name = "_card1Desc";
            this._card1Desc.Size = new System.Drawing.Size(211, 17);
            this._card1Desc.TabIndex = 3;
            this._card1Desc.Text = "从 Excel 批量导入分类组和分类";
            // 
            // _pnlCard2
            // 
            this._pnlCard2.BackColor = System.Drawing.Color.White;
            this._pnlCard2.Controls.Add(this._card2Bar);
            this._pnlCard2.Controls.Add(this._card2Icon);
            this._pnlCard2.Controls.Add(this._card2Title);
            this._pnlCard2.Controls.Add(this._card2Desc);
            this._pnlCard2.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlCard2.Location = new System.Drawing.Point(306, 24);
            this._pnlCard2.Name = "_pnlCard2";
            this._pnlCard2.Size = new System.Drawing.Size(323, 57);
            this._pnlCard2.TabIndex = 7;
            this._pnlCard2.Click += new System.EventHandler(this.btnClsImport_Click);
            this._pnlCard2.MouseEnter += new System.EventHandler(this.Card_MouseEnter);
            this._pnlCard2.MouseLeave += new System.EventHandler(this.Card_MouseLeave);
            // 
            // _card2Bar
            // 
            this._card2Bar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this._card2Bar.Dock = System.Windows.Forms.DockStyle.Left;
            this._card2Bar.Location = new System.Drawing.Point(0, 0);
            this._card2Bar.Name = "_card2Bar";
            this._card2Bar.Size = new System.Drawing.Size(4, 57);
            this._card2Bar.TabIndex = 0;
            // 
            // _card2Icon
            // 
            this._card2Icon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._card2Icon.Location = new System.Drawing.Point(7, 2);
            this._card2Icon.Name = "_card2Icon";
            this._card2Icon.Size = new System.Drawing.Size(30, 30);
            this._card2Icon.TabIndex = 1;
            this._card2Icon.Text = "📂";
            this._card2Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _card2Title
            // 
            this._card2Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._card2Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._card2Title.Location = new System.Drawing.Point(46, 8);
            this._card2Title.Name = "_card2Title";
            this._card2Title.Size = new System.Drawing.Size(157, 21);
            this._card2Title.TabIndex = 2;
            this._card2Title.Text = "分类导入";
            // 
            // _card2Desc
            // 
            this._card2Desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._card2Desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card2Desc.Location = new System.Drawing.Point(48, 29);
            this._card2Desc.Name = "_card2Desc";
            this._card2Desc.Size = new System.Drawing.Size(198, 17);
            this._card2Desc.TabIndex = 3;
            this._card2Desc.Text = "在指定分类组下批量插入分类";
            // 
            // _pnlCard4
            // 
            this._pnlCard4.BackColor = System.Drawing.Color.White;
            this._pnlCard4.Controls.Add(this._card4Bar);
            this._pnlCard4.Controls.Add(this._card4Icon);
            this._pnlCard4.Controls.Add(this._card4Title);
            this._pnlCard4.Controls.Add(this._card4Desc);
            this._pnlCard4.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlCard4.Location = new System.Drawing.Point(15, 25);
            this._pnlCard4.Name = "_pnlCard4";
            this._pnlCard4.Size = new System.Drawing.Size(280, 57);
            this._pnlCard4.TabIndex = 9;
            this._pnlCard4.Click += new System.EventHandler(this.btnPropertyImport_Click);
            this._pnlCard4.MouseEnter += new System.EventHandler(this.Card_MouseEnter);
            this._pnlCard4.MouseLeave += new System.EventHandler(this.Card_MouseLeave);
            // 
            // _card4Bar
            // 
            this._card4Bar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._card4Bar.Dock = System.Windows.Forms.DockStyle.Left;
            this._card4Bar.Location = new System.Drawing.Point(0, 0);
            this._card4Bar.Name = "_card4Bar";
            this._card4Bar.Size = new System.Drawing.Size(4, 57);
            this._card4Bar.TabIndex = 0;
            // 
            // _card4Icon
            // 
            this._card4Icon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._card4Icon.Location = new System.Drawing.Point(7, 1);
            this._card4Icon.Name = "_card4Icon";
            this._card4Icon.Size = new System.Drawing.Size(41, 42);
            this._card4Icon.TabIndex = 1;
            this._card4Icon.Text = "🏷";
            this._card4Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _card4Title
            // 
            this._card4Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._card4Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._card4Title.Location = new System.Drawing.Point(54, 5);
            this._card4Title.Name = "_card4Title";
            this._card4Title.Size = new System.Drawing.Size(208, 21);
            this._card4Title.TabIndex = 2;
            this._card4Title.Text = "分类下批量添加/删除属性";
            // 
            // _card4Desc
            // 
            this._card4Desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._card4Desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card4Desc.Location = new System.Drawing.Point(54, 31);
            this._card4Desc.Name = "_card4Desc";
            this._card4Desc.Size = new System.Drawing.Size(211, 20);
            this._card4Desc.TabIndex = 3;
            this._card4Desc.Text = "选择分类后批量设置其关联属性";
            // 
            // _pnlCard5
            // 
            this._pnlCard5.BackColor = System.Drawing.Color.White;
            this._pnlCard5.Controls.Add(this._card5Title);
            this._pnlCard5.Controls.Add(this._card5Bar);
            this._pnlCard5.Controls.Add(this._card5Icon);
            this._pnlCard5.Controls.Add(this._card5Desc);
            this._pnlCard5.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlCard5.Location = new System.Drawing.Point(638, 24);
            this._pnlCard5.Name = "_pnlCard5";
            this._pnlCard5.Size = new System.Drawing.Size(368, 58);
            this._pnlCard5.TabIndex = 10;
            this._pnlCard5.Click += new System.EventHandler(this.btnPropertyValueImport_Click);
            this._pnlCard5.MouseEnter += new System.EventHandler(this.Card_MouseEnter);
            this._pnlCard5.MouseLeave += new System.EventHandler(this.Card_MouseLeave);
            // 
            // _card5Title
            // 
            this._card5Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._card5Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._card5Title.Location = new System.Drawing.Point(40, 6);
            this._card5Title.Name = "_card5Title";
            this._card5Title.Size = new System.Drawing.Size(120, 19);
            this._card5Title.TabIndex = 2;
            this._card5Title.Text = "属性值导入";
            // 
            // _card5Bar
            // 
            this._card5Bar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._card5Bar.Dock = System.Windows.Forms.DockStyle.Left;
            this._card5Bar.Location = new System.Drawing.Point(0, 0);
            this._card5Bar.Name = "_card5Bar";
            this._card5Bar.Size = new System.Drawing.Size(4, 58);
            this._card5Bar.TabIndex = 0;
            // 
            // _card5Icon
            // 
            this._card5Icon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._card5Icon.Location = new System.Drawing.Point(8, 4);
            this._card5Icon.Name = "_card5Icon";
            this._card5Icon.Size = new System.Drawing.Size(30, 30);
            this._card5Icon.TabIndex = 1;
            this._card5Icon.Text = "📋";
            this._card5Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _card5Desc
            // 
            this._card5Desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._card5Desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card5Desc.Location = new System.Drawing.Point(38, 32);
            this._card5Desc.Name = "_card5Desc";
            this._card5Desc.Size = new System.Drawing.Size(327, 25);
            this._card5Desc.TabIndex = 3;
            this._card5Desc.Text = "批量导入属性值，与标准功能重合，用处不大";
            // 
            // _pnlCard6
            // 
            this._pnlCard6.BackColor = System.Drawing.Color.White;
            this._pnlCard6.Controls.Add(this._card6Bar);
            this._pnlCard6.Controls.Add(this._card6Icon);
            this._pnlCard6.Controls.Add(this._card6Title);
            this._pnlCard6.Controls.Add(this._card6Desc);
            this._pnlCard6.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlCard6.Location = new System.Drawing.Point(16, 25);
            this._pnlCard6.Name = "_pnlCard6";
            this._pnlCard6.Size = new System.Drawing.Size(145, 58);
            this._pnlCard6.TabIndex = 13;
            this._pnlCard6.Click += new System.EventHandler(this.btnSettings_Click);
            this._pnlCard6.MouseEnter += new System.EventHandler(this.Card_MouseEnter);
            this._pnlCard6.MouseLeave += new System.EventHandler(this.Card_MouseLeave);
            // 
            // _card6Bar
            // 
            this._card6Bar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card6Bar.Dock = System.Windows.Forms.DockStyle.Left;
            this._card6Bar.Location = new System.Drawing.Point(0, 0);
            this._card6Bar.Name = "_card6Bar";
            this._card6Bar.Size = new System.Drawing.Size(4, 58);
            this._card6Bar.TabIndex = 0;
            // 
            // _card6Icon
            // 
            this._card6Icon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._card6Icon.Location = new System.Drawing.Point(8, 2);
            this._card6Icon.Name = "_card6Icon";
            this._card6Icon.Size = new System.Drawing.Size(30, 41);
            this._card6Icon.TabIndex = 1;
            this._card6Icon.Text = "⚙";
            this._card6Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _card6Title
            // 
            this._card6Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._card6Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._card6Title.Location = new System.Drawing.Point(38, 2);
            this._card6Title.Name = "_card6Title";
            this._card6Title.Size = new System.Drawing.Size(89, 20);
            this._card6Title.TabIndex = 2;
            this._card6Title.Text = "设置";
            // 
            // _card6Desc
            // 
            this._card6Desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._card6Desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card6Desc.Location = new System.Drawing.Point(34, 22);
            this._card6Desc.Name = "_card6Desc";
            this._card6Desc.Size = new System.Drawing.Size(108, 21);
            this._card6Desc.TabIndex = 3;
            this._card6Desc.Text = "配置数据库连接";
            // 
            // _pnlCard7
            // 
            this._pnlCard7.BackColor = System.Drawing.Color.White;
            this._pnlCard7.Controls.Add(this._card7Bar);
            this._pnlCard7.Controls.Add(this._card7Icon);
            this._pnlCard7.Controls.Add(this._card7Title);
            this._pnlCard7.Controls.Add(this._card7Desc);
            this._pnlCard7.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlCard7.Location = new System.Drawing.Point(306, 24);
            this._pnlCard7.Name = "_pnlCard7";
            this._pnlCard7.Size = new System.Drawing.Size(326, 58);
            this._pnlCard7.TabIndex = 11;
            this._pnlCard7.Click += new System.EventHandler(this.btnBatchPropertyImport_Click);
            this._pnlCard7.MouseEnter += new System.EventHandler(this.Card_MouseEnter);
            this._pnlCard7.MouseLeave += new System.EventHandler(this.Card_MouseLeave);
            // 
            // _card7Bar
            // 
            this._card7Bar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._card7Bar.Dock = System.Windows.Forms.DockStyle.Left;
            this._card7Bar.Location = new System.Drawing.Point(0, 0);
            this._card7Bar.Name = "_card7Bar";
            this._card7Bar.Size = new System.Drawing.Size(4, 58);
            this._card7Bar.TabIndex = 0;
            // 
            // _card7Icon
            // 
            this._card7Icon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._card7Icon.Location = new System.Drawing.Point(8, 3);
            this._card7Icon.Name = "_card7Icon";
            this._card7Icon.Size = new System.Drawing.Size(30, 30);
            this._card7Icon.TabIndex = 1;
            this._card7Icon.Text = "📝";
            this._card7Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _card7Title
            // 
            this._card7Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._card7Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._card7Title.Location = new System.Drawing.Point(41, 4);
            this._card7Title.Name = "_card7Title";
            this._card7Title.Size = new System.Drawing.Size(185, 23);
            this._card7Title.TabIndex = 2;
            this._card7Title.Text = "分类下属性值批量导入";
            // 
            // _card7Desc
            // 
            this._card7Desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._card7Desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card7Desc.Location = new System.Drawing.Point(41, 33);
            this._card7Desc.Name = "_card7Desc";
            this._card7Desc.Size = new System.Drawing.Size(282, 20);
            this._card7Desc.TabIndex = 3;
            this._card7Desc.Text = "适用于批量导入一个分类下所有的属性的值";
            // 
            // _pnlCard9
            // 
            this._pnlCard9.BackColor = System.Drawing.Color.White;
            this._pnlCard9.Controls.Add(this._card9Bar);
            this._pnlCard9.Controls.Add(this._card9Icon);
            this._pnlCard9.Controls.Add(this._card9Title);
            this._pnlCard9.Controls.Add(this._card9Desc);
            this._pnlCard9.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlCard9.Location = new System.Drawing.Point(664, 113);
            this._pnlCard9.Name = "_pnlCard9";
            this._pnlCard9.Size = new System.Drawing.Size(280, 56);
            this._pnlCard9.TabIndex = 14;
            this._pnlCard9.Click += new System.EventHandler(this.btnBatchImport_Click);
            this._pnlCard9.MouseEnter += new System.EventHandler(this.Card_MouseEnter);
            this._pnlCard9.MouseLeave += new System.EventHandler(this.Card_MouseLeave);
            // 
            // _card9Bar
            // 
            this._card9Bar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(66)))), ((int)(((byte)(193)))));
            this._card9Bar.Dock = System.Windows.Forms.DockStyle.Left;
            this._card9Bar.Location = new System.Drawing.Point(0, 0);
            this._card9Bar.Name = "_card9Bar";
            this._card9Bar.Size = new System.Drawing.Size(4, 56);
            this._card9Bar.TabIndex = 0;
            // 
            // _card9Icon
            // 
            this._card9Icon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._card9Icon.Location = new System.Drawing.Point(3, 2);
            this._card9Icon.Name = "_card9Icon";
            this._card9Icon.Size = new System.Drawing.Size(41, 43);
            this._card9Icon.TabIndex = 1;
            this._card9Icon.Text = "📥";
            this._card9Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _card9Title
            // 
            this._card9Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._card9Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._card9Title.Location = new System.Drawing.Point(45, 8);
            this._card9Title.Name = "_card9Title";
            this._card9Title.Size = new System.Drawing.Size(188, 20);
            this._card9Title.TabIndex = 2;
            this._card9Title.Text = "元模型字段批量导入";
            // 
            // _card9Desc
            // 
            this._card9Desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._card9Desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card9Desc.Location = new System.Drawing.Point(46, 32);
            this._card9Desc.Name = "_card9Desc";
            this._card9Desc.Size = new System.Drawing.Size(223, 17);
            this._card9Desc.TabIndex = 3;
            this._card9Desc.Text = "适用于批量导入属性至meta的场景";
            // 
            // _pnlCard8
            // 
            this._pnlCard8.BackColor = System.Drawing.Color.White;
            this._pnlCard8.Controls.Add(this._card8Bar);
            this._pnlCard8.Controls.Add(this._card8Icon);
            this._pnlCard8.Controls.Add(this._card8Title);
            this._pnlCard8.Controls.Add(this._card8Desc);
            this._pnlCard8.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlCard8.Location = new System.Drawing.Point(15, 25);
            this._pnlCard8.Name = "_pnlCard8";
            this._pnlCard8.Size = new System.Drawing.Size(275, 56);
            this._pnlCard8.TabIndex = 12;
            this._pnlCard8.Click += new System.EventHandler(this.btnSuperBatchImport_Click);
            this._pnlCard8.Paint += new System.Windows.Forms.PaintEventHandler(this._pnlCard8_Paint);
            this._pnlCard8.MouseEnter += new System.EventHandler(this.Card_MouseEnter);
            this._pnlCard8.MouseLeave += new System.EventHandler(this.Card_MouseLeave);
            // 
            // _card8Bar
            // 
            this._card8Bar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(66)))), ((int)(((byte)(193)))));
            this._card8Bar.Dock = System.Windows.Forms.DockStyle.Left;
            this._card8Bar.Location = new System.Drawing.Point(0, 0);
            this._card8Bar.Name = "_card8Bar";
            this._card8Bar.Size = new System.Drawing.Size(4, 56);
            this._card8Bar.TabIndex = 0;
            // 
            // _card8Icon
            // 
            this._card8Icon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._card8Icon.Location = new System.Drawing.Point(14, 4);
            this._card8Icon.Name = "_card8Icon";
            this._card8Icon.Size = new System.Drawing.Size(30, 37);
            this._card8Icon.TabIndex = 1;
            this._card8Icon.Text = "🚀";
            this._card8Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _card8Title
            // 
            this._card8Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._card8Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._card8Title.Location = new System.Drawing.Point(54, 10);
            this._card8Title.Name = "_card8Title";
            this._card8Title.Size = new System.Drawing.Size(119, 18);
            this._card8Title.TabIndex = 2;
            this._card8Title.Text = "超级批量导入";
            // 
            // _card8Desc
            // 
            this._card8Desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._card8Desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card8Desc.Location = new System.Drawing.Point(54, 28);
            this._card8Desc.Name = "_card8Desc";
            this._card8Desc.Size = new System.Drawing.Size(220, 21);
            this._card8Desc.TabIndex = 3;
            this._card8Desc.Text = "Excel 一次性导入分类/属性/属性值";
            // 
            // _pnlCard10
            // 
            this._pnlCard10.BackColor = System.Drawing.Color.White;
            this._pnlCard10.Controls.Add(this._card10Bar);
            this._pnlCard10.Controls.Add(this._card10Icon);
            this._pnlCard10.Controls.Add(this._card10Title);
            this._pnlCard10.Controls.Add(this._card10Desc);
            this._pnlCard10.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlCard10.Location = new System.Drawing.Point(32, 25);
            this._pnlCard10.Name = "_pnlCard10";
            this._pnlCard10.Size = new System.Drawing.Size(678, 58);
            this._pnlCard10.TabIndex = 15;
            this._pnlCard10.Click += new System.EventHandler(this.btnWftScriptRemove_Click);
            this._pnlCard10.MouseEnter += new System.EventHandler(this.Card_MouseEnter);
            this._pnlCard10.MouseLeave += new System.EventHandler(this.Card_MouseLeave);
            // 
            // _card10Bar
            // 
            this._card10Bar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(126)))), ((int)(((byte)(20)))));
            this._card10Bar.Dock = System.Windows.Forms.DockStyle.Left;
            this._card10Bar.Location = new System.Drawing.Point(0, 0);
            this._card10Bar.Name = "_card10Bar";
            this._card10Bar.Size = new System.Drawing.Size(4, 58);
            this._card10Bar.TabIndex = 0;
            // 
            // _card10Icon
            // 
            this._card10Icon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._card10Icon.Location = new System.Drawing.Point(4, 3);
            this._card10Icon.Name = "_card10Icon";
            this._card10Icon.Size = new System.Drawing.Size(46, 41);
            this._card10Icon.TabIndex = 1;
            this._card10Icon.Text = "✂";
            this._card10Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _card10Title
            // 
            this._card10Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._card10Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._card10Title.Location = new System.Drawing.Point(46, 3);
            this._card10Title.Name = "_card10Title";
            this._card10Title.Size = new System.Drawing.Size(372, 18);
            this._card10Title.TabIndex = 2;
            this._card10Title.Text = "去除工作流元模板脚本";
            // 
            // _card10Desc
            // 
            this._card10Desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._card10Desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card10Desc.Location = new System.Drawing.Point(46, 23);
            this._card10Desc.Name = "_card10Desc";
            this._card10Desc.Size = new System.Drawing.Size(629, 23);
            this._card10Desc.TabIndex = 3;
            this._card10Desc.Text = "适用于批量删除工作流元模板中无法迁移升级的非标脚本；升级项目适用，可将流程记录一并升级。";
            // 
            // _pnlCard11
            // 
            this._pnlCard11.BackColor = System.Drawing.Color.White;
            this._pnlCard11.Controls.Add(this._card11Bar);
            this._pnlCard11.Controls.Add(this._card11Icon);
            this._pnlCard11.Controls.Add(this._card11Title);
            this._pnlCard11.Controls.Add(this._card11Desc);
            this._pnlCard11.Cursor = System.Windows.Forms.Cursors.Hand;
            this._pnlCard11.Location = new System.Drawing.Point(163, 25);
            this._pnlCard11.Name = "_pnlCard11";
            this._pnlCard11.Size = new System.Drawing.Size(141, 58);
            this._pnlCard11.TabIndex = 22;
            this._pnlCard11.Click += new System.EventHandler(this.btnLogViewer_Click);
            this._pnlCard11.MouseEnter += new System.EventHandler(this.Card_MouseEnter);
            this._pnlCard11.MouseLeave += new System.EventHandler(this.Card_MouseLeave);
            // 
            // _card11Bar
            // 
            this._card11Bar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this._card11Bar.Dock = System.Windows.Forms.DockStyle.Left;
            this._card11Bar.Location = new System.Drawing.Point(0, 0);
            this._card11Bar.Name = "_card11Bar";
            this._card11Bar.Size = new System.Drawing.Size(4, 58);
            this._card11Bar.TabIndex = 0;
            // 
            // _card11Icon
            // 
            this._card11Icon.Font = new System.Drawing.Font("Segoe UI", 16F);
            this._card11Icon.Location = new System.Drawing.Point(6, 3);
            this._card11Icon.Name = "_card11Icon";
            this._card11Icon.Size = new System.Drawing.Size(34, 34);
            this._card11Icon.TabIndex = 1;
            this._card11Icon.Text = "📋";
            this._card11Icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _card11Title
            // 
            this._card11Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._card11Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this._card11Title.Location = new System.Drawing.Point(43, 3);
            this._card11Title.Name = "_card11Title";
            this._card11Title.Size = new System.Drawing.Size(83, 20);
            this._card11Title.TabIndex = 2;
            this._card11Title.Text = "操作日志";
            // 
            // _card11Desc
            // 
            this._card11Desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._card11Desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this._card11Desc.Location = new System.Drawing.Point(35, 23);
            this._card11Desc.Name = "_card11Desc";
            this._card11Desc.Size = new System.Drawing.Size(97, 17);
            this._card11Desc.TabIndex = 3;
            this._card11Desc.Text = "查看操作记录";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(30, 107);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(253, 29);
            this.button1.TabIndex = 20;
            this.button1.Text = "属性编号允许重复的设置方法";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnSetupMethod_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(634, 114);
            this.label2.TabIndex = 19;
            this.label2.Text = "超级批量导入使用步骤：\r\n1、在”维护配置“增加属性集，且将属性集发布\r\n2、增加示例分类、示例表单(通用)\r\n3、调整高级设置，允许属性编号/值重复\r\n4、待工" +
    "具导入完成手动在”维护配置“将属性集关联到part类且发布(让系统自己触发去相关表增加字段)\r\n5、如果是属性集已增加到零件类中，那么导入完成后将part类手动" +
    "发布一次";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(321, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(691, 150);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "使用须知";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._pnlCard8);
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Location = new System.Drawing.Point(16, 283);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1030, 182);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "超级批量导入";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._pnlCard5);
            this.groupBox3.Controls.Add(this._pnlCard4);
            this.groupBox3.Controls.Add(this._pnlCard7);
            this.groupBox3.Location = new System.Drawing.Point(16, 187);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1012, 90);
            this.groupBox3.TabIndex = 24;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "属性管理";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this._pnlCard1);
            this.groupBox4.Controls.Add(this._pnlCard2);
            this.groupBox4.Location = new System.Drawing.Point(16, 89);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(642, 92);
            this.groupBox4.TabIndex = 25;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "分类管理";
            // 
            // groupBox5
            // 
            this.groupBox5.Location = new System.Drawing.Point(664, 89);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(310, 92);
            this.groupBox5.TabIndex = 26;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "元模型字段导入";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this._pnlCard9);
            this.groupBox6.Controls.Add(this._pnlCard10);
            this.groupBox6.Controls.Add(this.groupBox2);
            this.groupBox6.Controls.Add(this.groupBox4);
            this.groupBox6.Controls.Add(this.groupBox3);
            this.groupBox6.Controls.Add(this.groupBox5);
            this.groupBox6.Location = new System.Drawing.Point(12, 129);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(1046, 471);
            this.groupBox6.TabIndex = 27;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "功能";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this._pnlCard6);
            this.groupBox7.Controls.Add(this._pnlCard11);
            this.groupBox7.Location = new System.Drawing.Point(12, 23);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(310, 100);
            this.groupBox7.TabIndex = 28;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "系统";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1070, 628);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this._lblStatusDb);
            this.Controls.Add(this._lblStatusInfo);
            this.Controls.Add(this._lblVersion);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "数据导入工具";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this._pnlCard1.ResumeLayout(false);
            this._pnlCard2.ResumeLayout(false);
            this._pnlCard4.ResumeLayout(false);
            this._pnlCard5.ResumeLayout(false);
            this._pnlCard6.ResumeLayout(false);
            this._pnlCard7.ResumeLayout(false);
            this._pnlCard9.ResumeLayout(false);
            this._pnlCard8.ResumeLayout(false);
            this._pnlCard10.ResumeLayout(false);
            this._pnlCard11.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        // ══════════════════════════════════════════════════════════════
        // 组件清理
        // ══════════════════════════════════════════════════════════════
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }
        private Button button1;
        private Label label2;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private GroupBox groupBox6;
        private GroupBox groupBox7;
    }
}
