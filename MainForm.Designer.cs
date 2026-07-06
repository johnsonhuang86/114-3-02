namespace fx3u
{
    partial class MainForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblBaudRate = new System.Windows.Forms.Label();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.cmbPorts = new System.Windows.Forms.ComboBox();
            this.lblComPort = new System.Windows.Forms.Label();
            this.lblFlowControl = new System.Windows.Forms.Label();
            this.btnStartFlow = new System.Windows.Forms.Button();
            this.pnlRL = new System.Windows.Forms.Panel();
            this.lblRL = new System.Windows.Forms.Label();
            this.pnlYL = new System.Windows.Forms.Panel();
            this.lblYL = new System.Windows.Forms.Label();
            this.pnlGL = new System.Windows.Forms.Panel();
            this.lblGL = new System.Windows.Forms.Label();
            this.lblQuestion = new System.Windows.Forms.Label();
            this.cmbQuestions = new System.Windows.Forms.ComboBox();
            this.tblHeader = new System.Windows.Forms.TableLayoutPanel();
            this.flowHeaderTop = new System.Windows.Forms.FlowLayoutPanel();
            this.flowHeaderBottom = new System.Windows.Forms.FlowLayoutPanel();
            this.lblSequence = new System.Windows.Forms.Label();
            this.txtSequence = new System.Windows.Forms.TextBox();
            this.btnRunSequence = new System.Windows.Forms.Button();
            this.btnStandby = new System.Windows.Forms.Button();
            this.btnSingleJob = new System.Windows.Forms.Button();
            this.btnFlipOnly = new System.Windows.Forms.Button();
            this.btnContinuousJob = new System.Windows.Forms.Button();
            this.panelIndicators = new System.Windows.Forms.Panel();
            this.panelMain = new System.Windows.Forms.Panel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.grpX = new System.Windows.Forms.GroupBox();
            this.tblX = new System.Windows.Forms.TableLayoutPanel();
            this.grpY = new System.Windows.Forms.GroupBox();
            this.tblY = new System.Windows.Forms.TableLayoutPanel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panelLogButtons = new System.Windows.Forms.Panel();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.chkShowHex = new System.Windows.Forms.CheckBox();
            this.pollTimer = new System.Windows.Forms.Timer(this.components);
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblPollTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelHeader.SuspendLayout();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.grpX.SuspendLayout();
            this.grpY.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.panelLogButtons.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.panelHeader.Controls.Add(this.tblHeader);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1008, 80);
            this.panelHeader.TabIndex = 0;
            // 
            // tblHeader
            // 
            this.tblHeader.BackColor = System.Drawing.Color.Transparent;
            this.tblHeader.ColumnCount = 1;
            this.tblHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblHeader.Controls.Add(this.flowHeaderTop, 0, 0);
            this.tblHeader.Controls.Add(this.flowHeaderBottom, 0, 1);
            this.tblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblHeader.Location = new System.Drawing.Point(0, 0);
            this.tblHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tblHeader.Name = "tblHeader";
            this.tblHeader.RowCount = 2;
            this.tblHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tblHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tblHeader.Size = new System.Drawing.Size(1008, 80);
            this.tblHeader.TabIndex = 0;
            // 
            // flowHeaderTop
            // 
            this.flowHeaderTop.BackColor = System.Drawing.Color.Transparent;
            this.flowHeaderTop.Controls.Add(this.lblComPort);
            this.flowHeaderTop.Controls.Add(this.cmbPorts);
            this.flowHeaderTop.Controls.Add(this.lblBaudRate);
            this.flowHeaderTop.Controls.Add(this.lblQuestion);
            this.flowHeaderTop.Controls.Add(this.cmbQuestions);
            this.flowHeaderTop.Controls.Add(this.btnConnect);
            this.flowHeaderTop.Controls.Add(this.btnDisconnect);
            this.flowHeaderTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowHeaderTop.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowHeaderTop.Location = new System.Drawing.Point(0, 0);
            this.flowHeaderTop.Margin = new System.Windows.Forms.Padding(0);
            this.flowHeaderTop.Name = "flowHeaderTop";
            this.flowHeaderTop.Padding = new System.Windows.Forms.Padding(12, 8, 12, 0);
            this.flowHeaderTop.Size = new System.Drawing.Size(1008, 40);
            this.flowHeaderTop.TabIndex = 0;
            this.flowHeaderTop.WrapContents = false;
            // 
            // flowHeaderBottom
            // 
            this.flowHeaderBottom.BackColor = System.Drawing.Color.Transparent;
            this.flowHeaderBottom.Controls.Add(this.lblFlowControl);
            this.flowHeaderBottom.Controls.Add(this.btnStartFlow);
            this.flowHeaderBottom.Controls.Add(this.panelIndicators);
            this.flowHeaderBottom.Controls.Add(this.lblSequence);
            this.flowHeaderBottom.Controls.Add(this.txtSequence);
            this.flowHeaderBottom.Controls.Add(this.btnRunSequence);
            this.flowHeaderBottom.Controls.Add(this.btnStandby);
            this.flowHeaderBottom.Controls.Add(this.btnSingleJob);
            this.flowHeaderBottom.Controls.Add(this.btnFlipOnly);
            this.flowHeaderBottom.Controls.Add(this.btnContinuousJob);
            this.flowHeaderBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowHeaderBottom.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowHeaderBottom.Location = new System.Drawing.Point(0, 40);
            this.flowHeaderBottom.Margin = new System.Windows.Forms.Padding(0);
            this.flowHeaderBottom.Name = "flowHeaderBottom";
            this.flowHeaderBottom.Padding = new System.Windows.Forms.Padding(12, 6, 12, 0);
            this.flowHeaderBottom.Size = new System.Drawing.Size(1008, 40);
            this.flowHeaderBottom.TabIndex = 1;
            this.flowHeaderBottom.WrapContents = false;
            // 
            // lblComPort
            // 
            this.lblComPort.AutoSize = true;
            this.lblComPort.ForeColor = System.Drawing.Color.White;
            this.lblComPort.Margin = new System.Windows.Forms.Padding(0, 6, 5, 0);
            this.lblComPort.Name = "lblComPort";
            this.lblComPort.Size = new System.Drawing.Size(57, 15);
            this.lblComPort.TabIndex = 0;
            this.lblComPort.Text = "通訊埠 :";
            // 
            // cmbPorts
            // 
            this.cmbPorts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(30)))));
            this.cmbPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPorts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbPorts.ForeColor = System.Drawing.Color.White;
            this.cmbPorts.FormattingEnabled = true;
            this.cmbPorts.Margin = new System.Windows.Forms.Padding(0, 2, 15, 0);
            this.cmbPorts.Name = "cmbPorts";
            this.cmbPorts.Size = new System.Drawing.Size(70, 23);
            this.cmbPorts.TabIndex = 1;
            // 
            // lblBaudRate
            // 
            this.lblBaudRate.AutoSize = true;
            this.lblBaudRate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblBaudRate.Margin = new System.Windows.Forms.Padding(0, 6, 20, 0);
            this.lblBaudRate.Name = "lblBaudRate";
            this.lblBaudRate.Size = new System.Drawing.Size(127, 15);
            this.lblBaudRate.TabIndex = 4;
            this.lblBaudRate.Text = "參數: 9600, 7, E, 1";
            // 
            // lblQuestion
            // 
            this.lblQuestion.AutoSize = true;
            this.lblQuestion.ForeColor = System.Drawing.Color.White;
            this.lblQuestion.Margin = new System.Windows.Forms.Padding(0, 6, 5, 0);
            this.lblQuestion.Name = "lblQuestion";
            this.lblQuestion.Size = new System.Drawing.Size(69, 15);
            this.lblQuestion.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblQuestion.TabIndex = 10;
            this.lblQuestion.Text = "機電整合:";
            // 
            // cmbQuestions
            // 
            this.cmbQuestions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(30)))));
            this.cmbQuestions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbQuestions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbQuestions.ForeColor = System.Drawing.Color.White;
            this.cmbQuestions.FormattingEnabled = true;
            this.cmbQuestions.Margin = new System.Windows.Forms.Padding(0, 2, 20, 0);
            this.cmbQuestions.Name = "cmbQuestions";
            this.cmbQuestions.Size = new System.Drawing.Size(115, 23);
            this.cmbQuestions.TabIndex = 11;
            this.cmbQuestions.SelectedIndexChanged += new System.EventHandler(this.cmbQuestions_SelectedIndexChanged);
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(132)))), ((int)(((byte)(255)))));
            this.btnConnect.FlatAppearance.BorderSize = 0;
            this.btnConnect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(192)))));
            this.btnConnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(255)))));
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnConnect.ForeColor = System.Drawing.Color.White;
            this.btnConnect.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(80, 26);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "進行連線";
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(60)))));
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.FlatAppearance.BorderSize = 0;
            this.btnDisconnect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.btnDisconnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(72)))), ((int)(((byte)(74)))));
            this.btnDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDisconnect.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDisconnect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(69)))), ((int)(((byte)(58)))));
            this.btnDisconnect.Margin = new System.Windows.Forms.Padding(0, 0, 25, 0);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(80, 26);
            this.btnDisconnect.TabIndex = 3;
            this.btnDisconnect.Text = "中斷連線";
            this.btnDisconnect.UseVisualStyleBackColor = false;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // lblFlowControl
            // 
            this.lblFlowControl.AutoSize = true;
            this.lblFlowControl.ForeColor = System.Drawing.Color.White;
            this.lblFlowControl.Margin = new System.Windows.Forms.Padding(0, 6, 5, 0);
            this.lblFlowControl.Name = "lblFlowControl";
            this.lblFlowControl.Size = new System.Drawing.Size(127, 15);
            this.lblFlowControl.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFlowControl.TabIndex = 5;
            this.lblFlowControl.Text = "指示燈與流程控制:";
            // 
            // btnStartFlow
            // 
            this.btnStartFlow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(92)))), ((int)(((byte)(230)))));
            this.btnStartFlow.Enabled = false;
            this.btnStartFlow.FlatAppearance.BorderSize = 0;
            this.btnStartFlow.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(68)))), ((int)(((byte)(180)))));
            this.btnStartFlow.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(118)))), ((int)(((byte)(255)))));
            this.btnStartFlow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartFlow.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStartFlow.ForeColor = System.Drawing.Color.White;
            this.btnStartFlow.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.btnStartFlow.Name = "btnStartFlow";
            this.btnStartFlow.Size = new System.Drawing.Size(80, 26);
            this.btnStartFlow.TabIndex = 6;
            this.btnStartFlow.Text = "啟動流程";
            this.btnStartFlow.UseVisualStyleBackColor = false;
            this.btnStartFlow.Click += new System.EventHandler(this.btnStartFlow_Click);
            // 
            // panelIndicators
            // 
            this.panelIndicators.BackColor = System.Drawing.Color.Transparent;
            this.panelIndicators.Controls.Add(this.pnlRL);
            this.panelIndicators.Controls.Add(this.lblRL);
            this.panelIndicators.Controls.Add(this.pnlYL);
            this.panelIndicators.Controls.Add(this.lblYL);
            this.panelIndicators.Controls.Add(this.pnlGL);
            this.panelIndicators.Controls.Add(this.lblGL);
            this.panelIndicators.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.panelIndicators.Name = "panelIndicators";
            this.panelIndicators.Size = new System.Drawing.Size(165, 30);
            this.panelIndicators.TabIndex = 12;
            // 
            // pnlRL
            // 
            this.pnlRL.Location = new System.Drawing.Point(0, 4);
            this.pnlRL.Name = "pnlRL";
            this.pnlRL.Size = new System.Drawing.Size(18, 18);
            this.pnlRL.TabIndex = 7;
            this.pnlRL.Paint += new System.Windows.Forms.PaintEventHandler(this.LedPanel_Paint);
            // 
            // lblRL
            // 
            this.lblRL.AutoSize = true;
            this.lblRL.ForeColor = System.Drawing.Color.White;
            this.lblRL.Location = new System.Drawing.Point(21, 6);
            this.lblRL.Name = "lblRL";
            this.lblRL.Size = new System.Drawing.Size(23, 15);
            this.lblRL.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRL.Text = "RL";
            // 
            // pnlYL
            // 
            this.pnlYL.Location = new System.Drawing.Point(55, 4);
            this.pnlYL.Name = "pnlYL";
            this.pnlYL.Size = new System.Drawing.Size(18, 18);
            this.pnlYL.TabIndex = 8;
            this.pnlYL.Paint += new System.Windows.Forms.PaintEventHandler(this.LedPanel_Paint);
            // 
            // lblYL
            // 
            this.lblYL.AutoSize = true;
            this.lblYL.ForeColor = System.Drawing.Color.White;
            this.lblYL.Location = new System.Drawing.Point(76, 6);
            this.lblYL.Name = "lblYL";
            this.lblYL.Size = new System.Drawing.Size(22, 15);
            this.lblYL.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblYL.Text = "YL";
            // 
            // pnlGL
            // 
            this.pnlGL.Location = new System.Drawing.Point(110, 4);
            this.pnlGL.Name = "pnlGL";
            this.pnlGL.Size = new System.Drawing.Size(18, 18);
            this.pnlGL.TabIndex = 9;
            this.pnlGL.Paint += new System.Windows.Forms.PaintEventHandler(this.LedPanel_Paint);
            // 
            // lblGL
            // 
            this.lblGL.AutoSize = true;
            this.lblGL.ForeColor = System.Drawing.Color.White;
            this.lblGL.Location = new System.Drawing.Point(131, 6);
            this.lblGL.Name = "lblGL";
            this.lblGL.Size = new System.Drawing.Size(24, 15);
            this.lblGL.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblGL.Text = "GL";
            // 
            // lblSequence
            // 
            this.lblSequence.AutoSize = true;
            this.lblSequence.ForeColor = System.Drawing.Color.White;
            this.lblSequence.Margin = new System.Windows.Forms.Padding(25, 6, 5, 0);
            this.lblSequence.Name = "lblSequence";
            this.lblSequence.Size = new System.Drawing.Size(69, 15);
            this.lblSequence.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSequence.TabIndex = 13;
            this.lblSequence.Text = "動作序列:";
            // 
            // txtSequence
            // 
            this.txtSequence.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(30)))));
            this.txtSequence.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSequence.ForeColor = System.Drawing.Color.White;
            this.txtSequence.Margin = new System.Windows.Forms.Padding(0, 2, 10, 0);
            this.txtSequence.Name = "txtSequence";
            this.txtSequence.Size = new System.Drawing.Size(180, 23);
            this.txtSequence.TabIndex = 14;
            this.txtSequence.Text = "A+B+T1B-A-";
            // 
            // btnRunSequence
            // 
            this.btnRunSequence.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(199)))), ((int)(((byte)(89)))));
            this.btnRunSequence.Enabled = false;
            this.btnRunSequence.FlatAppearance.BorderSize = 0;
            this.btnRunSequence.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(150)))), ((int)(((byte)(60)))));
            this.btnRunSequence.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(220)))), ((int)(((byte)(110)))));
            this.btnRunSequence.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunSequence.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRunSequence.ForeColor = System.Drawing.Color.White;
            this.btnRunSequence.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.btnRunSequence.Name = "btnRunSequence";
            this.btnRunSequence.Size = new System.Drawing.Size(80, 26);
            this.btnRunSequence.TabIndex = 15;
            this.btnRunSequence.Text = "啟動序列";
            this.btnRunSequence.UseVisualStyleBackColor = false;
            this.btnRunSequence.Click += new System.EventHandler(this.btnRunSequence_Click);
            // 
            // btnStandby
            // 
            this.btnStandby.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(159)))), ((int)(((byte)(10)))));
            this.btnStandby.Enabled = false;
            this.btnStandby.FlatAppearance.BorderSize = 0;
            this.btnStandby.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(110)))), ((int)(((byte)(0)))));
            this.btnStandby.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(180)))), ((int)(((byte)(60)))));
            this.btnStandby.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStandby.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStandby.ForeColor = System.Drawing.Color.White;
            this.btnStandby.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnStandby.Name = "btnStandby";
            this.btnStandby.Size = new System.Drawing.Size(80, 26);
            this.btnStandby.TabIndex = 16;
            this.btnStandby.Text = "待機";
            this.btnStandby.UseVisualStyleBackColor = false;
            this.btnStandby.Click += new System.EventHandler(this.btnStandby_Click);
            // 
            // btnSingleJob
            // 
            this.btnSingleJob.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(175)))), ((int)(((byte)(82)))), ((int)(((byte)(222)))));
            this.btnSingleJob.Enabled = false;
            this.btnSingleJob.FlatAppearance.BorderSize = 0;
            this.btnSingleJob.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(50)))), ((int)(((byte)(180)))));
            this.btnSingleJob.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(110)))), ((int)(((byte)(240)))));
            this.btnSingleJob.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSingleJob.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSingleJob.ForeColor = System.Drawing.Color.White;
            this.btnSingleJob.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnSingleJob.Name = "btnSingleJob";
            this.btnSingleJob.Size = new System.Drawing.Size(80, 26);
            this.btnSingleJob.TabIndex = 17;
            this.btnSingleJob.Text = "單一作業";
            this.btnSingleJob.UseVisualStyleBackColor = false;
            this.btnSingleJob.Click += new System.EventHandler(this.btnSingleJob_Click);
            // 
            // btnFlipOnly
            // 
            this.btnFlipOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(45)))), ((int)(((byte)(85)))));
            this.btnFlipOnly.Enabled = false;
            this.btnFlipOnly.FlatAppearance.BorderSize = 0;
            this.btnFlipOnly.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(20)))), ((int)(((byte)(60)))));
            this.btnFlipOnly.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(90)))), ((int)(((byte)(120)))));
            this.btnFlipOnly.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFlipOnly.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnFlipOnly.ForeColor = System.Drawing.Color.White;
            this.btnFlipOnly.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnFlipOnly.Name = "btnFlipOnly";
            this.btnFlipOnly.Size = new System.Drawing.Size(80, 26);
            this.btnFlipOnly.TabIndex = 18;
            this.btnFlipOnly.Text = "指定翻轉";
            this.btnFlipOnly.UseVisualStyleBackColor = false;
            this.btnFlipOnly.Click += new System.EventHandler(this.btnFlipOnly_Click);
            // 
            // btnContinuousJob
            // 
            this.btnContinuousJob.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(176)))), ((int)(((byte)(199)))));
            this.btnContinuousJob.Enabled = false;
            this.btnContinuousJob.FlatAppearance.BorderSize = 0;
            this.btnContinuousJob.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(120)))), ((int)(((byte)(140)))));
            this.btnContinuousJob.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(210)))), ((int)(((byte)(230)))));
            this.btnContinuousJob.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnContinuousJob.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnContinuousJob.ForeColor = System.Drawing.Color.White;
            this.btnContinuousJob.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnContinuousJob.Name = "btnContinuousJob";
            this.btnContinuousJob.Size = new System.Drawing.Size(80, 26);
            this.btnContinuousJob.TabIndex = 19;
            this.btnContinuousJob.Text = "連續作業";
            this.btnContinuousJob.UseVisualStyleBackColor = false;
            this.btnContinuousJob.Click += new System.EventHandler(this.btnContinuousJob_Click);
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.splitContainer);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 80);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(10);
            this.panelMain.Size = new System.Drawing.Size(1008, 330);
            this.panelMain.TabIndex = 1;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(10, 10);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.grpX);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.grpY);
            this.splitContainer.Size = new System.Drawing.Size(988, 330);
            this.splitContainer.SplitterDistance = 450;
            this.splitContainer.TabIndex = 0;
            // 
            // grpX
            // 
            this.grpX.Controls.Add(this.tblX);
            this.grpX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpX.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(132)))), ((int)(((byte)(255)))));
            this.grpX.Location = new System.Drawing.Point(0, 0);
            this.grpX.Name = "grpX";
            this.grpX.Padding = new System.Windows.Forms.Padding(10);
            this.grpX.Size = new System.Drawing.Size(450, 330);
            this.grpX.TabIndex = 0;
            this.grpX.TabStop = false;
            this.grpX.Text = "INPUT 狀態監控 (X0~X7, X10~X17)";
            // 
            // tblX
            // 
            this.tblX.ColumnCount = 2;
            this.tblX.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblX.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblX.Location = new System.Drawing.Point(10, 27);
            this.tblX.Name = "tblX";
            this.tblX.RowCount = 8;
            this.tblX.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblX.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblX.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblX.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblX.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblX.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblX.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblX.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblX.Size = new System.Drawing.Size(430, 293);
            this.tblX.TabIndex = 0;
            // 
            // grpY
            // 
            this.grpY.Controls.Add(this.tblY);
            this.grpY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpY.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(209)))), ((int)(((byte)(88)))));
            this.grpY.Location = new System.Drawing.Point(0, 0);
            this.grpY.Name = "grpY";
            this.grpY.Padding = new System.Windows.Forms.Padding(10);
            this.grpY.Size = new System.Drawing.Size(534, 330);
            this.grpY.TabIndex = 0;
            this.grpY.TabStop = false;
            this.grpY.Text = "OUTPUT 控制與監控 (Y0~Y7, Y10~Y17)";
            // 
            // tblY
            // 
            this.tblY.ColumnCount = 2;
            this.tblY.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblY.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblY.Location = new System.Drawing.Point(10, 27);
            this.tblY.Name = "tblY";
            this.tblY.RowCount = 8;
            this.tblY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblY.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblY.Size = new System.Drawing.Size(514, 293);
            this.tblY.TabIndex = 0;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.grpLog);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 410);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(10, 0, 10, 10);
            this.panelBottom.Size = new System.Drawing.Size(1008, 200);
            this.panelBottom.TabIndex = 2;
            // 
            // grpLog
            // 
            this.grpLog.Controls.Add(this.txtLog);
            this.grpLog.Controls.Add(this.panelLogButtons);
            this.grpLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLog.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(214)))), ((int)(((byte)(10)))));
            this.grpLog.Location = new System.Drawing.Point(10, 0);
            this.grpLog.Name = "grpLog";
            this.grpLog.Padding = new System.Windows.Forms.Padding(10);
            this.grpLog.Size = new System.Drawing.Size(988, 190);
            this.grpLog.TabIndex = 0;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "通訊日誌 (Communication Log)";
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(30)))));
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
            this.txtLog.Location = new System.Drawing.Point(10, 56);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(968, 124);
            this.txtLog.TabIndex = 0;
            // 
            // panelLogButtons
            // 
            this.panelLogButtons.Controls.Add(this.btnClearLog);
            this.panelLogButtons.Controls.Add(this.chkShowHex);
            this.panelLogButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLogButtons.Location = new System.Drawing.Point(10, 26);
            this.panelLogButtons.Name = "panelLogButtons";
            this.panelLogButtons.Size = new System.Drawing.Size(968, 30);
            this.panelLogButtons.TabIndex = 1;
            // 
            // btnClearLog
            // 
            this.btnClearLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.btnClearLog.FlatAppearance.BorderSize = 0;
            this.btnClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearLog.ForeColor = System.Drawing.Color.White;
            this.btnClearLog.Location = new System.Drawing.Point(3, 3);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(75, 23);
            this.btnClearLog.TabIndex = 1;
            this.btnClearLog.Text = "清除日誌";
            this.btnClearLog.UseVisualStyleBackColor = false;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // chkShowHex
            // 
            this.chkShowHex.AutoSize = true;
            this.chkShowHex.Checked = true;
            this.chkShowHex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowHex.ForeColor = System.Drawing.Color.White;
            this.chkShowHex.Location = new System.Drawing.Point(90, 6);
            this.chkShowHex.Name = "chkShowHex";
            this.chkShowHex.Size = new System.Drawing.Size(103, 19);
            this.chkShowHex.TabIndex = 0;
            this.chkShowHex.Text = "顯示 Hex 內容";
            this.chkShowHex.UseVisualStyleBackColor = true;
            // 
            // pollTimer
            // 
            this.pollTimer.Interval = 200;
            this.pollTimer.Tick += new System.EventHandler(this.pollTimer_Tick);
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.lblPollTime});
            this.statusStrip.Location = new System.Drawing.Point(0, 610);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1008, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(94, 17);
            this.lblStatus.Text = "狀態: 尚未連線";
            // 
            // lblPollTime
            // 
            this.lblPollTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblPollTime.Name = "lblPollTime";
            this.lblPollTime.Size = new System.Drawing.Size(899, 17);
            this.lblPollTime.Spring = true;
            this.lblPollTime.Text = "輪詢時間: - ms";
            this.lblPollTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(1008, 632);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.statusStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new System.Drawing.Size(1024, 670);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "三菱 PLC FX3U 序列通訊監控系統 (Windows Forms App)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.grpX.ResumeLayout(false);
            this.grpY.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.grpLog.ResumeLayout(false);
            this.grpLog.PerformLayout();
            this.panelLogButtons.ResumeLayout(false);
            this.panelLogButtons.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblBaudRate;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.ComboBox cmbPorts;
        private System.Windows.Forms.Label lblComPort;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.GroupBox grpX;
        private System.Windows.Forms.TableLayoutPanel tblX;
        private System.Windows.Forms.GroupBox grpY;
        private System.Windows.Forms.TableLayoutPanel tblY;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Panel panelLogButtons;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.CheckBox chkShowHex;
        private System.Windows.Forms.Timer pollTimer;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblPollTime;
        private System.Windows.Forms.Label lblFlowControl;
        private System.Windows.Forms.Button btnStartFlow;
        private System.Windows.Forms.Panel pnlRL;
        private System.Windows.Forms.Label lblRL;
        private System.Windows.Forms.Panel pnlYL;
        private System.Windows.Forms.Label lblYL;
        private System.Windows.Forms.Panel pnlGL;
        private System.Windows.Forms.Label lblGL;
        private System.Windows.Forms.Label lblQuestion;
        private System.Windows.Forms.ComboBox cmbQuestions;
        private System.Windows.Forms.TableLayoutPanel tblHeader;
        private System.Windows.Forms.FlowLayoutPanel flowHeaderTop;
        private System.Windows.Forms.FlowLayoutPanel flowHeaderBottom;
        private System.Windows.Forms.Label lblSequence;
        private System.Windows.Forms.TextBox txtSequence;
        private System.Windows.Forms.Button btnRunSequence;
        private System.Windows.Forms.Button btnStandby;
        private System.Windows.Forms.Button btnSingleJob;
        private System.Windows.Forms.Button btnFlipOnly;
        private System.Windows.Forms.Button btnContinuousJob;
        private System.Windows.Forms.Panel panelIndicators;
    }
}
