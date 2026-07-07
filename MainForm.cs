using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fx3u
{
    public partial class MainForm : Form
    {
        private Fx3uSerialComm _comm;
        private Panel[] _xLeds = new Panel[16];
        private Panel[] _yLeds = new Panel[16];
        private Button[] _yButtons = new Button[16];
        private Label[] _xLabels = new Label[16];
        private Label[] _yLabels = new Label[16];
        private bool _isPolling = false;
        private bool _isConnectionNormal = false;

        private readonly Color _colorStartFlowBack = Color.FromArgb(94, 92, 230);
        private readonly Color _colorRunSequenceBack = Color.FromArgb(52, 199, 89);
        private readonly Color _colorStandbyBack = Color.FromArgb(255, 159, 10);
        private readonly Color _colorSingleJobBack = Color.FromArgb(175, 82, 222);
        private readonly Color _colorFlipOnlyBack = Color.FromArgb(255, 45, 85);
        private readonly Color _colorContinuousJobBack = Color.FromArgb(48, 176, 199);

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeCustomUI();
            RefreshComPorts();
            
            // 初始化題目選擇選單
            cmbQuestions.Items.Add("請選擇題目...");
            cmbQuestions.Items.Add("第 1 題 (機電丙一)");
            cmbQuestions.Items.Add("第 2 題 (機電丙二)");
            cmbQuestions.Items.Add("第 3 題 (機電丙三)");
            cmbQuestions.Items.Add("第 4 題 (機電丙四)");
            cmbQuestions.Items.Add("第 5 題 (機電丙五)");
            cmbQuestions.SelectedIndex = 0;

            // 依最寬項目自動調整 ComboBox 寬度
            AdjustComboBoxWidth(cmbQuestions);

            // 依顯示的文字修正按鈕寬度
            AdjustButtonWidth(btnConnect);
            AdjustButtonWidth(btnDisconnect);
            AdjustButtonWidth(btnStartFlow);
            AdjustButtonWidth(btnRunSequence);
            AdjustButtonWidth(btnStandby);
            AdjustButtonWidth(btnSingleJob);
            AdjustButtonWidth(btnFlipOnly);
            AdjustButtonWidth(btnContinuousJob);

            // 初始將按鈕視覺設為灰色停用
            UpdateButtonsVisualState(false);
            SetControlsEnabled(false);

            AppendLog("INFO", "程式啟動。請選擇通訊埠與題目，並點擊「PLC連線」開始監控。");
        }

        private void AdjustButtonWidth(Button btn)
        {
            using (Graphics g = btn.CreateGraphics())
            {
                SizeF size = g.MeasureString(btn.Text, btn.Font);
                btn.Width = (int)Math.Ceiling(size.Width) + 24;
            }
        }

        private void RefreshComPorts()
        {
            cmbPorts.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                foreach (string port in ports)
                {
                    cmbPorts.Items.Add(port);
                }
                cmbPorts.SelectedIndex = 0;
            }
            else
            {
                cmbPorts.Items.Add("COM1");
                cmbPorts.Items.Add("COM2");
                cmbPorts.SelectedIndex = 0;
                AppendLog("WARNING", "系統未偵測到任何實體 COM Port。已載入模擬虛擬埠。");
            }

            // 依最寬項目自動調整 ComboBox 寬度
            AdjustComboBoxWidth(cmbPorts);
        }

        private void InitializeCustomUI()
        {
            tblX.Controls.Clear();
            tblY.Controls.Clear();

            // 動態配置 X0~X7 與 X10~X17 控制項到 tblX (2列8行)
            for (int row = 0; row < 8; row++)
            {
                // X0~X7 (左半欄)
                int indexLeft = row;
                string octalLeft = indexLeft.ToString();
                var pnlLeft = CreateLedItemPanel("X" + octalLeft, indexLeft, false);
                tblX.Controls.Add(pnlLeft, 0, row);

                // X10~X17 (右半欄)
                int indexRight = row + 8;
                string octalRight = (row + 10).ToString();
                var pnlRight = CreateLedItemPanel("X" + octalRight, indexRight, false);
                tblX.Controls.Add(pnlRight, 1, row);
            }

            // 動態配置 Y0~Y7 與 Y10~Y17 控制項到 tblY (2列8行)
            for (int row = 0; row < 8; row++)
            {
                // Y0~Y7 (左半欄)
                int indexLeft = row;
                string octalLeft = indexLeft.ToString();
                var pnlLeft = CreateLedItemPanel("Y" + octalLeft, indexLeft, true);
                tblY.Controls.Add(pnlLeft, 0, row);

                // Y10~Y17 (右半欄)
                int indexRight = row + 8;
                string octalRight = (row + 10).ToString();
                var pnlRight = CreateLedItemPanel("Y" + octalRight, indexRight, true);
                tblY.Controls.Add(pnlRight, 1, row);
            }
        }

        private Panel CreateLedItemPanel(string name, int index, bool isY)
        {
            Panel itemPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                BackColor = Color.FromArgb(44, 44, 46)
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = isY ? 3 : 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35F)); // LED 燈號欄 (固定 35 像素)
            if (isY)
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // 名稱欄 (自適應)
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75F));  // 按鈕欄 (固定 75 像素)
            }
            else
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // 名稱欄 (自適應)
            }
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // LED 燈圓形 Panel
            Panel ledPanel = new Panel
            {
                Size = new Size(18, 18),
                Anchor = AnchorStyles.None,
                Tag = false
            };
            ledPanel.Paint += LedPanel_Paint;

            if (isY)
            {
                _yLeds[index] = ledPanel;
            }
            else
            {
                _xLeds[index] = ledPanel;
            }

            // 文字 Label
            Label nameLabel = new Label
            {
                Text = name,
                Anchor = AnchorStyles.Left,
                ForeColor = Color.FromArgb(242, 242, 247),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(5, 0, 0, 0)
            };

            if (isY)
            {
                _yLabels[index] = nameLabel;
            }
            else
            {
                _xLabels[index] = nameLabel;
            }

            layout.Controls.Add(ledPanel, 0, 0);
            layout.Controls.Add(nameLabel, 1, 0);

            // 如果是 Y，加上強制 ON/OFF 的切換按鈕
            if (isY)
            {
                Button btn = new Button
                {
                    Text = "切換",
                    Size = new Size(65, 24),
                    Anchor = AnchorStyles.Right,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(58, 58, 60),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft JhengHei UI", 8.5F, FontStyle.Bold),
                    Tag = index,
                    Margin = new Padding(0, 0, 5, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(72, 72, 74);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 100, 105);
                btn.Click += YButton_Click;

                _yButtons[index] = btn;
                layout.Controls.Add(btn, 2, 0);
            }

            itemPanel.Controls.Add(layout);
            return itemPanel;
        }

        private void LedPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel p = (Panel)sender;
            bool isOn = p.Tag != null && (bool)p.Tag;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Color ledColor;
            Color lightBorderColor;
            Color lightGradientColor;

            if (isOn)
            {
                if (p.Name == "pnlRL")
                {
                    ledColor = Color.FromArgb(255, 69, 58); // 紅色燈
                    lightGradientColor = Color.FromArgb(255, 140, 130);
                    lightBorderColor = Color.FromArgb(255, 180, 180);
                }
                else if (p.Name == "pnlYL")
                {
                    ledColor = Color.FromArgb(255, 214, 10); // 黃色燈
                    lightGradientColor = Color.FromArgb(255, 240, 150);
                    lightBorderColor = Color.FromArgb(255, 240, 180);
                }
                else
                {
                    // 預設為綠色燈 (X/Y 點位與 GL 綠燈)
                    ledColor = Color.FromArgb(48, 209, 88); 
                    lightGradientColor = Color.FromArgb(140, 255, 170);
                    lightBorderColor = Color.FromArgb(140, 255, 170);
                }
            }
            else
            {
                // OFF 狀態暗灰色
                ledColor = Color.FromArgb(72, 72, 74);
                lightGradientColor = Color.FromArgb(100, 100, 105);
                lightBorderColor = Color.FromArgb(84, 84, 88);
            }

            // 繪製 3D 立體球體漸層
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Point(1, 1),
                new Point(p.Width - 2, p.Height - 2),
                lightGradientColor,
                ledColor))
            {
                e.Graphics.FillEllipse(brush, 1, 1, p.Width - 3, p.Height - 3);
            }

            // 繪製反射高光點
            if (isOn)
            {
                using (var highlightBrush = new SolidBrush(Color.FromArgb(180, Color.White)))
                {
                    e.Graphics.FillEllipse(highlightBrush, 4, 4, 3, 3);
                }
            }

            // 繪製邊框
            using (var pen = new Pen(lightBorderColor, 1.2f))
            {
                e.Graphics.DrawEllipse(pen, 1, 1, p.Width - 3, p.Height - 3);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string portName = cmbPorts.Text;
            if (string.IsNullOrEmpty(portName))
            {
                MessageBox.Show("請選擇一個通訊埠！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _comm = new Fx3uSerialComm(portName);
                _comm.LogMessage += Comm_LogMessage;
                _comm.Open();

                lblStatus.Text = $"狀態: 已連線至 {portName}";
                lblStatus.ForeColor = Color.FromArgb(48, 209, 88);
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                cmbPorts.Enabled = false;
                cmbQuestions.Enabled = false; // 鎖定題目

                // 開啟定時輪詢
                pollTimer.Start();

                AppendLog("INFO", "連線建立成功，開始背景輪詢狀態。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"連線失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog("ERROR", $"開啟 {portName} 失敗: {ex.Message}");
                if (_comm != null)
                {
                    _comm.Dispose();
                    _comm = null;
                }
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            pollTimer.Stop();
            _isConnectionNormal = false;
            UpdateButtonsVisualState(false);
            SetControlsEnabled(false);

            if (_comm != null)
            {
                _comm.Close();
                _comm.Dispose();
                _comm = null;
            }

            lblStatus.Text = "狀態: 尚未連線";
            lblStatus.ForeColor = Color.White;
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            cmbPorts.Enabled = true;
            cmbQuestions.Enabled = true; // 解鎖題目
            lblPollTime.Text = "輪詢時間: - ms";

            // 重置所有燈號
            ResetLeds();
            AppendLog("INFO", "中斷連線。");
        }

        private void ResetLeds()
        {
            for (int i = 0; i < 16; i++)
            {
                _xLeds[i].Tag = false;
                _xLeds[i].Invalidate();
                _yLeds[i].Tag = false;
                _yLeds[i].Invalidate();
            }
            pnlRL.Tag = false;
            pnlRL.Invalidate();
            pnlYL.Tag = false;
            pnlYL.Invalidate();
            pnlGL.Tag = false;
            pnlGL.Invalidate();
        }

        private async void pollTimer_Tick(object sender, EventArgs e)
        {
            if (_isPolling || _comm == null || !_comm.IsOpen) return;
            _isPolling = true;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // 非同步讀取 X0~X17 與 Y0~Y17 狀態，確保 UI 不卡頓
                bool[] xStates = await Task.Run(() => _comm.ReadDeviceStates(true));
                bool[] yStates = await Task.Run(() => _comm.ReadDeviceStates(false));

                // 回到 UI 執行緒更新 UI 燈號
                for (int i = 0; i < 16; i++)
                {
                    if (_xLeds[i].Tag == null || (bool)_xLeds[i].Tag != xStates[i])
                    {
                        _xLeds[i].Tag = xStates[i];
                        _xLeds[i].Invalidate();
                    }

                    if (_yLeds[i].Tag == null || (bool)_yLeds[i].Tag != yStates[i])
                    {
                        _yLeds[i].Tag = yStates[i];
                        _yLeds[i].Invalidate();
                    }
                }

                // 首次成功取得 PLC 數值回應，切換按鈕為彩色並啟用
                if (!_isConnectionNormal)
                {
                    _isConnectionNormal = true;
                    UpdateButtonsVisualState(true);
                    SetControlsEnabled(true);
                    AppendLog("INFO", "PLC 通訊正常取得數據，已啟用控制功能。");
                }

                // 同步三色燈面板與 Y15 (RL), Y16 (YL), Y17 (GL)
                if (pnlRL.Tag == null || (bool)pnlRL.Tag != yStates[13])
                {
                    pnlRL.Tag = yStates[13];
                    pnlRL.Invalidate();
                }
                if (pnlYL.Tag == null || (bool)pnlYL.Tag != yStates[14])
                {
                    pnlYL.Tag = yStates[14];
                    pnlYL.Invalidate();
                }
                if (pnlGL.Tag == null || (bool)pnlGL.Tag != yStates[15])
                {
                    pnlGL.Tag = yStates[15];
                    pnlGL.Invalidate();
                }

                // 第 2 題 Y17 (待機綠燈) 狀態聯鎖動態判定：
                // 條件: (Y0~Y10皆為off) AND (X5 off) AND (X1、X3、X4皆on) AND (X6 OR X7為on)
                if (cmbQuestions.SelectedIndex == 2 && btnRunSequence.Enabled)
                {
                    bool condY0_10Off = true;
                    // Y0~Y7 (0~7), Y10 (8)
                    for (int j = 0; j <= 8; j++)
                    {
                        if (yStates[j])
                        {
                            condY0_10Off = false;
                            break;
                        }
                    }
                    bool condX5Off = !xStates[5];
                    bool condX134On = xStates[1] && xStates[3] && xStates[4];
                    bool condX67On = xStates[6] || xStates[7];

                    bool targetY17 = condY0_10Off && condX5Off && condX134On && condX67On;

                    if (yStates[15] != targetY17)
                    {
                        await Task.Run(() => _comm.ForceCoil(15, targetY17));
                        yStates[15] = targetY17;
                        _yLeds[15].Tag = targetY17;
                        _yLeds[15].Invalidate();
                        pnlGL.Tag = targetY17;
                        pnlGL.Invalidate();
                        AppendLog("INFO", $"第2題待機聯鎖觸發，自動設定 Y17 待機綠燈為 {(targetY17 ? "ON" : "OFF")}。");
                    }
                }

                // 第 1 題 Y16 (待機黃燈) 狀態聯鎖動態判定：
                // 條件: (Y0~Y7皆為off) AND (X1、X2皆 on) AND (X7~X12皆為off)
                if (cmbQuestions.SelectedIndex == 1 && btnRunSequence.Enabled)
                {
                    bool condY0_7Off = true;
                    for (int j = 0; j <= 7; j++)
                    {
                        if (yStates[j])
                        {
                            condY0_7Off = false;
                            break;
                        }
                    }
                    bool condX12On = xStates[1] && xStates[2];
                    bool condX7_12Off = true;
                    for (int j = 7; j <= 12; j++)
                    {
                        if (xStates[j])
                        {
                            condX7_12Off = false;
                            break;
                        }
                    }

                    bool targetY16 = condY0_7Off && condX12On && condX7_12Off;

                    if (yStates[14] != targetY16)
                    {
                        await Task.Run(() => _comm.ForceCoil(14, targetY16));
                        yStates[14] = targetY16;
                        _yLeds[14].Tag = targetY16;
                        _yLeds[14].Invalidate();
                        pnlYL.Tag = targetY16;
                        pnlYL.Invalidate();
                        AppendLog("INFO", $"第1題待機聯鎖觸發，自動設定 Y16 待機黃燈為 {(targetY16 ? "ON" : "OFF")}。");
                    }
                }

                watch.Stop();
                lblPollTime.Text = $"輪詢時間: {watch.ElapsedMilliseconds} ms";
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"狀態更新失敗: {ex.Message}");
                if (_isConnectionNormal)
                {
                    _isConnectionNormal = false;
                    UpdateButtonsVisualState(false);
                    SetControlsEnabled(false);
                }
                if (_comm == null || !_comm.IsOpen)
                {
                    btnDisconnect_Click(this, EventArgs.Empty);
                    MessageBox.Show("通訊發生異常，已自動斷開連線。\n原因: " + ex.Message, "連線中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                _isPolling = false;
            }
        }

        private async void YButton_Click(object sender, EventArgs e)
        {
            if (_comm == null || !_comm.IsOpen) return;

            Button btn = (Button)sender;
            int index = (int)btn.Tag;
            bool currentVal = _yLeds[index].Tag != null && (bool)_yLeds[index].Tag;
            bool targetVal = !currentVal;

            btn.Enabled = false; // 防連擊
            try
            {
                string octStr = GetOctalString(index);
                AppendLog("INFO", $"發送強制寫入 Y{octStr} 為 {(targetVal ? "ON" : "OFF")}...");
                await Task.Run(() => _comm.ForceCoil(index, targetVal));
                
                // 成功後先行更新 UI 給予立即回饋
                _yLeds[index].Tag = targetVal;
                _yLeds[index].Invalidate();
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"控制 Y{GetOctalString(index)} 失敗: {ex.Message}");
                MessageBox.Show($"控制寫入指令失敗:\n{ex.Message}", "寫入失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn.Enabled = true;
            }
        }

        private void Comm_LogMessage(string type, string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string, string>(Comm_LogMessage), type, msg);
                return;
            }

            if ((type == "TX" || type == "RX") && !chkShowHex.Checked)
            {
                return;
            }

            AppendLog(type, msg);
        }

        private void AppendLog(string type, string msg)
        {
            if (txtLog.IsDisposed) return;

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string prefix = $"[{timestamp}] [{type}] ";
            
            txtLog.AppendText(prefix + msg + Environment.NewLine);

            // 控制日誌緩衝行數在 500 行內
            if (txtLog.Lines.Length > 500)
            {
                string[] newLines = new string[300];
                Array.Copy(txtLog.Lines, txtLog.Lines.Length - 300, newLines, 0, 300);
                txtLog.Lines = newLines;
            }

            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            pollTimer.Stop();
            if (_comm != null)
            {
                _comm.Close();
                _comm.Dispose();
                _comm = null;
            }
        }

        private string GetOctalString(int index)
        {
            if (index < 8)
            {
                return index.ToString();
            }
            else
            {
                return (index + 2).ToString(); // 例如 8 -> Y10, 15 -> Y17
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            splitContainer.SplitterDistance = (int)(splitContainer.Width * 0.45);
        }

        private async void btnStartFlow_Click(object sender, EventArgs e)
        {
            if (_comm == null || !_comm.IsOpen) return;

            btnStartFlow.Enabled = false;
            AppendLog("INFO", "=== 啟動三色燈指示流程 (RL -> YL -> GL) ===");
            try
            {
                // 1. RL (Y15) ON 1s -> OFF
                AppendLog("INFO", "步驟 [1/3]: Y15 (RL 紅色指示燈) ON...");
                await Task.Run(() => _comm.ForceCoil(13, true));
                pnlRL.Tag = true;
                pnlRL.Invalidate();
                _yLeds[13].Tag = true;
                _yLeds[13].Invalidate();
                await Task.Delay(1000);

                AppendLog("INFO", "步驟 [1/3]: Y15 (RL 紅色指示燈) OFF...");
                await Task.Run(() => _comm.ForceCoil(13, false));
                pnlRL.Tag = false;
                pnlRL.Invalidate();
                _yLeds[13].Tag = false;
                _yLeds[13].Invalidate();

                // 2. YL (Y16) ON 1s -> OFF
                AppendLog("INFO", "步驟 [2/3]: Y16 (YL 黃色指示燈) ON...");
                await Task.Run(() => _comm.ForceCoil(14, true));
                pnlYL.Tag = true;
                pnlYL.Invalidate();
                _yLeds[14].Tag = true;
                _yLeds[14].Invalidate();
                await Task.Delay(1000);

                AppendLog("INFO", "步驟 [2/3]: Y16 (YL 黃色指示燈) OFF...");
                await Task.Run(() => _comm.ForceCoil(14, false));
                pnlYL.Tag = false;
                pnlYL.Invalidate();
                _yLeds[14].Tag = false;
                _yLeds[14].Invalidate();

                // 3. GL (Y17) ON 1s -> OFF
                AppendLog("INFO", "步驟 [3/3]: Y17 (GL 綠色指示燈) ON...");
                await Task.Run(() => _comm.ForceCoil(15, true));
                pnlGL.Tag = true;
                pnlGL.Invalidate();
                _yLeds[15].Tag = true;
                _yLeds[15].Invalidate();
                await Task.Delay(1000);

                AppendLog("INFO", "步驟 [3/3]: Y17 (GL 綠色指示燈) OFF...");
                await Task.Run(() => _comm.ForceCoil(15, false));
                pnlGL.Tag = false;
                pnlGL.Invalidate();
                _yLeds[15].Tag = false;
                _yLeds[15].Invalidate();

                AppendLog("INFO", "=== 三色燈指示流程圓滿完成 ===");
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"流程異常中斷: {ex.Message}");
                MessageBox.Show($"流程控制執行失敗:\n{ex.Message}", "流程中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try
                {
                    await Task.Run(() =>
                    {
                        _comm.ForceCoil(13, false);
                        _comm.ForceCoil(14, false);
                        _comm.ForceCoil(15, false);
                    });
                }
                catch { }
                ResetLeds();
            }
            finally
            {
                if (_comm != null && _comm.IsOpen)
                {
                    btnStartFlow.Enabled = true;
                }
            }
        }

        private static readonly string[] QuestionIoData = new string[]
        {
            // 第 1 題
            @"X0 a1下
X1 a0上
X2 s0進料
X3 s1圓方料
X4 p0入料位
X5 p1圓料
X6 p2方料
X7 PB1輸送帶
X10 PB2 A缸
X11 PB3 左移
X12 PB4 右移
X13 1工作/復歸
X14 2L指定/單
X15 2R連續/單
X16 ST/RST
X17 EMS急停

Y0 A 垂直下

Y2 M1 輸送帶

Y4 M2+ 左移
Y5 M2- 右移
Y6 B+ 夾
Y7 B- 放

Y15 紅運轉
Y16 黃復歸
Y17 綠待機",

            // 第 2 題
            @"X0 a1上
X1 a0下
X2 b1進料端
X3 b0出料端
X4 c0夾開
X5 ps1真空
X6 e1正轉
X7 e0反轉
X10 s0進料有無
X11 s1姿勢上下
X12 s2顏色紅黑
X13 1工作復歸
X14 2L指定單一
X15 2R連續單一
X16 ST/RST
X17 EMS

Y0 A+上
Y1 A-下
Y2 B迴轉進料
Y3 C+夾住
Y4 C-放鬆
Y5 D+吸真空
Y6 M+正轉
Y7 M-反轉
Y10 D-放真空

Y15 紅燈運轉
Y16 黃燈復歸
Y17 綠燈待機",

            // 第 3 題
            @"X0 a1進
X1 a0退
X2 b0零度
X3 b1正90度
X4 b2負90度
X5 ps1真空壓
X6 d0上
X7 d1下
X10 s1孔 感測

X13 1工作/復歸
X14 2L指定/單
X15 2R連續/單
X16 ST/RST
X17 EMS急停

Y0 A 進

Y2 R2零度
Y3 B1正90度
Y4 B2負90度
Y5 C+真空吸
Y6 C-真空放
Y7 M+R1升降

Y15 紅燈運轉
Y16 黃燈復歸
Y17 綠燈待機",

            // 第 4 題
            @"X0 a1印下
X1 a0印上
X2 b1鑽孔下
X3 b0鑽孔上
X4 c0夾後-上
X5 c1夾前-下
X6 s0分度盤
X7 s1鋁/塑料
X10 p0進料
X11 p1出料
X12 STOP
X13 1工作/復歸
X14 2L指定/單
X15 2R連續/單
X16 ST/RST
X17 EMS急停

Y0 A 印下

Y2 B+鑽孔下
Y3 B-鑽孔上
Y4 C-夾縮-上
Y5 C+夾伸-下
Y6 M1+轉盤
Y7 M2+鑽孔

Y15 紅燈運轉
Y16 黃燈復歸
Y17 綠燈待機",

            // 第 5 題
            @"X0 a0頂料上
X1 b1水平前
X2 b0水平後
X3 c1垂直下
X4 c0垂直上
X5 p0出料0
X6 p1/2出料12
X7 p3出料3
X10 s0進料
X11 s1重量上
X12 s2重量下
X13 1工作/復歸
X14 2L指定/單
X15 2R連續/單
X16 ST/RST
X17 EMS急停

Y0 A 頂料下
Y1 B+水平前
Y2 B-水平後
Y3 C 垂直下

Y5 M+ 整列+
Y6 M- 整列-
Y10 D+ 夾
Y11 D- 放

Y15 紅運轉
Y16 黃復歸
Y17 綠待機"
        };

        private void cmbQuestions_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = cmbQuestions.SelectedIndex;
            if (selectedIndex <= 0)
            {
                ResetIoNames();
                AppendLog("INFO", "重置 IO 點位名稱為預設值。");
                return;
            }

            LoadIoNamesFromFile(selectedIndex);

            // 若已連線，動態切換各特定題目按鈕啟用狀態
            if (_comm != null && _comm.IsOpen)
            {
                bool isQ1OrQ2 = (selectedIndex == 1 || selectedIndex == 2);
                btnStandby.Enabled = isQ1OrQ2;
                btnSingleJob.Enabled = isQ1OrQ2;
                btnFlipOnly.Enabled = isQ1OrQ2;
                btnContinuousJob.Enabled = isQ1OrQ2;
            }
        }

        private void LoadIoNamesFromFile(int index)
        {
            string filename = $"IO表-機丙{index}.txt";
            string[] searchPaths = new string[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", filename),
                Path.Combine(Environment.CurrentDirectory, filename),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", filename),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", filename)
            };

            string foundPath = null;
            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                {
                    foundPath = path;
                    break;
                }
            }

            if (foundPath != null)
            {
                try
                {
                    string text = File.ReadAllText(foundPath, Encoding.UTF8);
                    if (text.Contains("\uFFFD"))
                    {
                        text = File.ReadAllText(foundPath, Encoding.Default);
                    }
                    LoadIoNamesFromText(text);
                    AppendLog("INFO", $"成功自檔案載入第 {index} 題 IO 設定：{Path.GetFileName(foundPath)}");
                }
                catch (Exception ex)
                {
                    AppendLog("ERROR", $"讀取 IO 設定檔 {filename} 失敗: {ex.Message}，載入內置設定。");
                    LoadIoNamesFromText(QuestionIoData[index - 1]);
                }
            }
            else
            {
                AppendLog("WARNING", $"找不到外部檔案 {filename}，改載入內置備用設定。");
                LoadIoNamesFromText(QuestionIoData[index - 1]);
            }
        }

        private void LoadIoNamesFromText(string text)
        {
            string[] xNames = new string[16];
            string[] yNames = new string[16];

            string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                string[] parts = trimmed.Split(new char[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                string ioCode = parts[0].ToUpper();
                string ioName = parts[1].Trim();

                if (ioCode.StartsWith("X"))
                {
                    int index = ParseIoIndex(ioCode);
                    if (index >= 0 && index < 16)
                    {
                        xNames[index] = ioName;
                    }
                }
                else if (ioCode.StartsWith("Y"))
                {
                    int index = ParseIoIndex(ioCode);
                    if (index >= 0 && index < 16)
                    {
                        yNames[index] = ioName;
                    }
                }
            }

            // 更新 UI Labels 與調整按鈕寬度
            for (int i = 0; i < 16; i++)
            {
                string octStr = GetOctalString(i);
                _xLabels[i].Text = string.IsNullOrEmpty(xNames[i]) ? $"X{octStr}" : $"X{octStr} {xNames[i]}";
                _yLabels[i].Text = string.IsNullOrEmpty(yNames[i]) ? $"Y{octStr}" : $"Y{octStr} {yNames[i]}";
            }
        }

        private void ResetIoNames()
        {
            for (int i = 0; i < 16; i++)
            {
                string octStr = GetOctalString(i);
                _xLabels[i].Text = $"X{octStr}";
                _yLabels[i].Text = $"Y{octStr}";
            }
        }

        private int ParseIoIndex(string ioCode)
        {
            if (ioCode.Length < 2) return -1;
            string numStr = ioCode.Substring(1);
            if (int.TryParse(numStr, out int octalNum))
            {
                if (octalNum >= 0 && octalNum <= 7)
                {
                    return octalNum;
                }
                if (octalNum >= 10 && octalNum <= 17)
                {
                    return 8 + (octalNum - 10);
                }
            }
            return -1;
        }

        private void AdjustComboBoxWidth(ComboBox comboBox)
        {
            int maxWidth = 0;
            using (Graphics g = comboBox.CreateGraphics())
            {
                foreach (var item in comboBox.Items)
                {
                    string text = item.ToString();
                    int width = (int)g.MeasureString(text, comboBox.Font).Width + 30;
                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
            }
            if (maxWidth > 0)
            {
                comboBox.Width = maxWidth;
                comboBox.DropDownWidth = maxWidth;
            }
        }

        private async void btnRunSequence_Click(object sender, EventArgs e)
        {
            await RunSequenceAsync(txtSequence.Text);
        }

        private List<string> ParseSequence(string input)
        {
            List<string> steps = new List<string>();
            int i = 0;
            while (i < input.Length)
            {
                char c = input[i];
                if (c == 'A' || c == 'B' || c == 'C' || c == 'D')
                {
                    if (i + 1 < input.Length && (input[i + 1] == '+' || input[i + 1] == '-'))
                    {
                        steps.Add(input.Substring(i, 2));
                        i += 2;
                    }
                    else
                    {
                        throw new Exception($"無效的氣壓缸指令於第 {i + 1} 個字元：缺少 '+' 或 '-'。");
                    }
                }
                else if (c == 'T')
                {
                    int start = i + 1;
                    int length = 0;
                    while (start + length < input.Length && char.IsDigit(input[start + length]))
                    {
                        length++;
                    }
                    if (length == 0)
                    {
                        throw new Exception($"無效的暫停指令於第 {i + 1} 個字元：缺少秒數。");
                    }
                    steps.Add(input.Substring(i, 1 + length));
                    i += 1 + length;
                }
                else if (char.IsWhiteSpace(c))
                {
                    i++;
                }
                else
                {
                    throw new Exception($"無法辨識的字元 '{c}' 於第 {i + 1} 個字元。");
                }
            }
            return steps;
        }

        private async Task<bool> WaitForInputStateAsync(int xIndex, bool expectedState, int timeoutSeconds = 15)
        {
            int loops = timeoutSeconds * 10;
            for (int i = 0; i < loops; i++)
            {
                if (_xLeds[xIndex].Tag != null && (bool)_xLeds[xIndex].Tag == expectedState)
                {
                    return true;
                }
                await Task.Delay(100);
            }
            return false;
        }

        private async Task RunSequenceAsync(string seqText)
        {
            List<string> steps;
            try
            {
                steps = ParseSequence(seqText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"動作序列語法錯誤：\n{ex.Message}", "語法錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行動作序列！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 停用所有按鈕防止衝擊
            SetControlsEnabled(false);
            AppendLog("INFO", $"=== 開始執行動作序列: {seqText} ===");

            try
            {
                for (int stepIdx = 0; stepIdx < steps.Count; stepIdx++)
                {
                    string step = steps[stepIdx];
                    AppendLog("INFO", $"步驟 [{stepIdx + 1}/{steps.Count}]: 正在執行 {step}...");

                    if (step.StartsWith("T"))
                    {
                        int seconds = int.Parse(step.Substring(1));
                        AppendLog("INFO", $"暫停 {seconds} 秒...");
                        await Task.Delay(seconds * 1000);
                    }
                    else
                    {
                        char cylinder = step[0];
                        char action = step[1];

                        int outputY1 = -1;
                        int outputY2 = -1;
                        bool stateY1 = false;
                        bool stateY2 = false;
                        
                        int waitX = -1;
                        bool expectedX = false;
                        int cleanupY = -1;

                        if (cylinder == 'A')
                        {
                            if (action == '+')
                            {
                                outputY1 = 0;   // Y0
                                stateY1 = true;
                                outputY2 = 1;   // Y1
                                stateY2 = false;
                                
                                waitX = 0;      // X0 (a1)
                                expectedX = true;
                                cleanupY = 0;   // 到位後釋放 Y0
                            }
                            else // '-'
                            {
                                outputY1 = 1;   // Y1
                                stateY1 = true;
                                outputY2 = 0;   // Y0
                                stateY2 = false;
                                
                                waitX = 1;      // X1 (a0)
                                expectedX = true;
                                cleanupY = 1;   // 到位後釋放 Y1
                            }
                        }
                        else if (cylinder == 'B')
                        {
                            if (action == '+')
                            {
                                outputY1 = 2;   // Y2
                                stateY1 = true;
                                waitX = 2;      // X2 (b1)
                                expectedX = true;
                                cleanupY = -1;  // 單閥維持狀態
                            }
                            else // '-'
                            {
                                outputY1 = 2;   // Y2
                                stateY1 = false;
                                waitX = 3;      // X3 (b0)
                                expectedX = true;
                                cleanupY = -1;
                            }
                        }
                        else if (cylinder == 'C')
                        {
                            if (action == '+')
                            {
                                outputY1 = 3;   // Y3
                                stateY1 = true;
                                outputY2 = 4;   // Y4
                                stateY2 = false;
                                
                                waitX = 4;      // X4 off (c0 離去)
                                expectedX = false;
                                cleanupY = 3;
                            }
                            else // '-'
                            {
                                outputY1 = 4;   // Y4
                                stateY1 = true;
                                outputY2 = 3;   // Y3
                                stateY2 = false;
                                
                                waitX = 4;      // X4 on (c0 夾開)
                                expectedX = true;
                                cleanupY = 4;
                            }
                        }
                        else if (cylinder == 'D')
                        {
                            if (action == '+')
                            {
                                outputY1 = 5;   // Y5
                                stateY1 = true;
                                outputY2 = 8;   // Y10 (index 8)
                                stateY2 = false;
                                
                                waitX = 5;      // X5 on (ps1 真空)
                                expectedX = true;
                                cleanupY = 5;
                            }
                            else // '-'
                            {
                                outputY1 = 8;   // Y10 (index 8)
                                stateY1 = true;
                                outputY2 = 5;   // Y5
                                stateY2 = false;
                                
                                waitX = 5;      // X5 off
                                expectedX = false;
                                cleanupY = 8;
                            }
                        }

                        // 下發 PLC 指令
                        if (outputY1 != -1)
                        {
                            int yVal1 = outputY1;
                            bool sVal1 = stateY1;
                            await Task.Run(() => _comm.ForceCoil(yVal1, sVal1));
                        }
                        if (outputY2 != -1)
                        {
                            int yVal2 = outputY2;
                            bool sVal2 = stateY2;
                            await Task.Run(() => _comm.ForceCoil(yVal2, sVal2));
                        }

                        // 等待極限開關狀態
                        if (waitX != -1)
                        {
                            string waitName = (waitX == 4) ? (expectedX ? "X4(c0 on)" : "X4(c0 off)")
                                            : (waitX == 5) ? (expectedX ? "X5(ps1 on)" : "X5(ps1 off)")
                                            : $"X{waitX}";
                            
                            AppendLog("INFO", $"等待極限開關 {waitName} 到達目標狀態: {expectedX}...");
                            bool waitSuccess = await WaitForInputStateAsync(waitX, expectedX, 15);
                            if (!waitSuccess)
                            {
                                throw new Exception($"等待步驟 {step} 的極限開關 {waitName} 超時！");
                            }
                            AppendLog("INFO", $"步驟 {step} 順利到位。");
                        }

                        // 到位後關閉雙閥線圈
                        if (cleanupY != -1)
                        {
                            int cleanY = cleanupY;
                            await Task.Run(() => _comm.ForceCoil(cleanY, false));
                        }
                    }
                }

                AppendLog("INFO", "=== 動作序列順利執行完畢 ===");
                MessageBox.Show("動作序列已全部執行完畢！", "執行成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"動作序列執行失敗: {ex.Message}");
                MessageBox.Show($"動作序列在中途發生錯誤而中斷：\n{ex.Message}", "執行中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 安全關斷所有閥件
                try
                {
                    await Task.Run(() =>
                    {
                        for (int i = 0; i <= 8; i++) _comm.ForceCoil(i, false);
                    });
                }
                catch { }
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private async void btnStandby_Click(object sender, EventArgs e)
        {
            if (cmbQuestions.SelectedIndex == 1)
            {
                await RunStandbySequenceQ1Async();
            }
            else if (cmbQuestions.SelectedIndex == 2)
            {
                await RunStandbySequenceAsync();
            }
        }

        private async Task RunStandbySequenceQ1Async()
        {
            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行待機復歸！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(false);
            AppendLog("INFO", "=== 開始執行第 1 題待機復歸流程 ===");
            CancellationTokenSource y16BlinkCts = null;

            try
            {
                // Y16 閃爍
                y16BlinkCts = new CancellationTokenSource();
                CancellationToken token = y16BlinkCts.Token;
                _ = Task.Run(async () => {
                    try
                    {
                        bool state = true;
                        while (!token.IsCancellationRequested)
                        {
                            _comm.ForceCoil(14, state);
                            state = !state;
                            await Task.Delay(500, token);
                        }
                    }
                    catch { }
                }, token);

                // 1. 所有 Y 先 off 1秒
                AppendLog("INFO", "復歸步驟 [1/4]: 所有 Y 點位 OFF 1秒...");
                await Task.Run(() => {
                    for (int i = 0; i < 16; i++) _comm.ForceCoil(i, false);
                });
                await Task.Delay(1000);

                // 2. Y7 (B- 放) ON 1秒鐘
                AppendLog("INFO", "復歸步驟 [2/4]: Y7 (B-) ON 1秒...");
                await Task.Run(() => _comm.ForceCoil(7, true));
                await Task.Delay(1000);
                await Task.Run(() => _comm.ForceCoil(7, false));

                // 3. Y5 (M2- 右移) ON 直到 X2 (s0進料) ON
                AppendLog("INFO", "復歸步驟 [3/4]: Y5 ON 直到 X2 ON...");
                await Task.Run(() => {
                    _comm.ForceCoil(5, true);
                    _comm.ForceCoil(8, false); // Y10 off
                });
                if (!await WaitForInputStateAsync(2, true, 15))
                {
                    throw new Exception("等待 X2 ON 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(5, false));

                // 4. 關閉黃燈閃爍，點亮待機綠燈 (Y17)
                AppendLog("INFO", "復歸步驟 [4/4]: 停止黃燈閃爍，點亮待機綠燈 (Y17)...");
                if (y16BlinkCts != null)
                {
                    y16BlinkCts.Cancel();
                    y16BlinkCts.Dispose();
                    y16BlinkCts = null;
                }
                await Task.Run(() => {
                    _comm.ForceCoil(14, false);
                    _comm.ForceCoil(15, true);
                });

                AppendLog("INFO", "=== 第 1 題待機復歸流程成功完成 ===");
                MessageBox.Show("機構已成功回到機械原點！", "復歸成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"第 1 題待機復歸中斷: {ex.Message}");
                MessageBox.Show($"第 1 題待機復歸執行失敗：\n{ex.Message}", "復歸中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (y16BlinkCts != null)
                {
                    y16BlinkCts.Cancel();
                    y16BlinkCts.Dispose();
                    y16BlinkCts = null;
                }
                try
                {
                    await Task.Run(() => {
                        for (int i = 0; i < 16; i++) _comm.ForceCoil(i, false);
                        _comm.ForceCoil(15, true); // 保全待機
                    });
                }
                catch { }
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private async Task RunStandbySequenceAsync()
        {
            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行待機復歸！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(false);
            AppendLog("INFO", "=== 開始執行待機復歸流程 ===");
            CancellationTokenSource y16BlinkCts = null;

            try
            {
                y16BlinkCts = new CancellationTokenSource();
                CancellationToken token = y16BlinkCts.Token;
                _ = Task.Run(async () => {
                    try
                    {
                        bool state = true;
                        while (!token.IsCancellationRequested)
                        {
                            _comm.ForceCoil(14, state);
                            state = !state;
                            await Task.Delay(500, token);
                        }
                    }
                    catch { }
                }, token);

                // 1. 所有 Y 接點先 off 0.5 秒
                AppendLog("INFO", "復歸步驟 [1/9]: 所有 Y 接點 OFF 0.5 秒...");
                await Task.Run(() => {
                    for (int i = 0; i < 16; i++) _comm.ForceCoil(i, false);
                });
                await Task.Delay(500);

                // 2. Y2 off 到 X3 on
                AppendLog("INFO", "復歸步驟 [2/9]: Y2 OFF，等待 B缸退回 X3...");
                await Task.Run(() => _comm.ForceCoil(2, false));
                if (!await WaitForInputStateAsync(3, true, 15))
                {
                    throw new Exception("等待 X3(b0) ON 逾時！");
                }

                // 3. T1 (暫停 1 秒)
                AppendLog("INFO", "復歸步驟 [3/9]: 暫停 1 秒...");
                await Task.Delay(1000);

                // 4. Y0 on 直到 X0 on
                AppendLog("INFO", "復歸步驟 [4/9]: Y0 ON (A+)，等待 X0 ON...");
                await Task.Run(() => {
                    _comm.ForceCoil(0, true);
                    _comm.ForceCoil(1, false);
                });
                if (!await WaitForInputStateAsync(0, true, 15))
                {
                    throw new Exception("等待 X0(a1) ON 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(0, false));

                // 5. Y6 on 直到 X6 on
                AppendLog("INFO", "復歸步驟 [5/9]: Y6 ON (M+)，等待 X6 ON...");
                await Task.Run(() => {
                    _comm.ForceCoil(6, true);
                    _comm.ForceCoil(5, false);
                });
                if (!await WaitForInputStateAsync(6, true, 15))
                {
                    throw new Exception("等待 X6 ON 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(6, false));

                // 6. T1 (暫停 1 秒)
                AppendLog("INFO", "復歸步驟 [6/9]: 暫停 1 秒...");
                await Task.Delay(1000);

                // 7. Y1 on 直到 X1 on
                AppendLog("INFO", "復歸步驟 [7/9]: Y1 ON (A-)，等待 X1 ON...");
                await Task.Run(() => {
                    _comm.ForceCoil(1, true);
                    _comm.ForceCoil(0, false);
                });
                if (!await WaitForInputStateAsync(1, true, 15))
                {
                    throw new Exception("等待 X1(a0) ON 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(1, false));

                // 8. Y4 on 直到 X4 on
                AppendLog("INFO", "復歸步驟 [8/9]: Y4 ON (C-)，等待 X4 ON...");
                await Task.Run(() => {
                    _comm.ForceCoil(4, true);
                    _comm.ForceCoil(3, false);
                });
                if (!await WaitForInputStateAsync(4, true, 15))
                {
                    throw new Exception("等待 X4(c0) ON 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(4, false));

                // 9. Y10 on 直到 ps1 off
                AppendLog("INFO", "復歸步驟 [9/9]: Y10 ON (D-)，等待 X5 OFF...");
                await Task.Run(() => {
                    _comm.ForceCoil(8, true); // Y10
                    _comm.ForceCoil(5, false); // Y5
                });
                if (!await WaitForInputStateAsync(5, false, 15))
                {
                    throw new Exception("等待 X5(ps1) OFF 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(8, false));

                // 結束黃燈閃爍
                if (y16BlinkCts != null)
                {
                    y16BlinkCts.Cancel();
                    y16BlinkCts.Dispose();
                    y16BlinkCts = null;
                }
                await Task.Run(() => _comm.ForceCoil(14, false));

                AppendLog("INFO", "=== 待機復歸流程成功完成，已進入待機監控 ====");
                MessageBox.Show("機構已成功回到機械原點！", "復歸成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"待機復歸中斷: {ex.Message}");
                MessageBox.Show($"待機復歸執行失敗：\n{ex.Message}", "復歸中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (y16BlinkCts != null)
                {
                    y16BlinkCts.Cancel();
                    y16BlinkCts.Dispose();
                    y16BlinkCts = null;
                }
                try
                {
                    await Task.Run(() => {
                        for (int i = 0; i < 16; i++) _comm.ForceCoil(i, false);
                    });
                }
                catch { }
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private async void btnSingleJob_Click(object sender, EventArgs e)
        {
            if (cmbQuestions.SelectedIndex == 1)
            {
                await RunJobQ1Async(false); // 第1題單一作業
            }
            else if (cmbQuestions.SelectedIndex == 2)
            {
                await RunSingleJobAsync();
            }
        }

        private async Task RunSingleJobAsync()
        {
            // 執行前提：Y17 ON 且 X10 ON
            bool isY17On = pnlGL.Tag != null && (bool)pnlGL.Tag;
            bool isX10On = _xLeds[8].Tag != null && (bool)_xLeds[8].Tag; // X10 index 8

            if (!isY17On || !isX10On)
            {
                MessageBox.Show("無法執行單一作業！\n前提條件：必須在綠燈待機 (Y17 ON) 且有進料 (X10 ON) 狀態下才能執行！", "前提條件不符", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行單一作業！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(false);
            AppendLog("INFO", "=== 開始執行單一作業流程 ===");
            CancellationTokenSource blinkCts = null;

            try
            {
                // Y17 off, Y15 on
                await Task.Run(() => {
                    _comm.ForceCoil(15, false); // GL off
                    _comm.ForceCoil(13, true);  // RL on
                });

                // 2. 判斷 X12 (顏色)
                bool isX12 = _xLeds[10].Tag != null && (bool)_xLeds[10].Tag; // X12 index 10
                if (!isX12)
                {
                    AppendLog("INFO", "偵測到 X12 為 OFF (黑色料)，啟用 Y15 閃爍...");
                    blinkCts = new CancellationTokenSource();
                    CancellationToken token = blinkCts.Token;
                    _ = Task.Run(async () => {
                        try
                        {
                            bool state = true;
                            while (!token.IsCancellationRequested)
                            {
                                _comm.ForceCoil(13, state);
                                state = !state;
                                await Task.Delay(500, token);
                            }
                        }
                        catch { }
                    }, token);
                }
                else
                {
                    AppendLog("INFO", "偵測到 X12 為 ON (紅色料)，Y15 保持恆亮。");
                }

                // 3. 判斷 X11 (姿勢)
                bool isX11 = _xLeds[9].Tag != null && (bool)_xLeds[9].Tag; // X11 index 9
                if (isX11)
                {
                    AppendLog("INFO", "X11 為 ON，直接進入移料步驟。");
                }
                else
                {
                    AppendLog("INFO", "X11 為 OFF，執行翻轉步驟...");
                    await ExecuteFlipSequenceAsync();
                }

                // 4. 移料步驟
                await ExecuteMoveMaterialSequenceAsync();

                // 5. 結束作業，關閉 Y15 閃爍，點亮 Y17
                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                    blinkCts = null;
                }

                AppendLog("INFO", "單一作業結束，點亮待機綠燈 (Y17)，熄滅紅燈 (Y15)...");
                await Task.Run(() => {
                    _comm.ForceCoil(13, false);
                    _comm.ForceCoil(15, true);
                });

                AppendLog("INFO", "=== 單一作業流程成功執行完畢 ===");
                MessageBox.Show("單一作業已成功執行完畢！", "作業成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"單一作業中斷: {ex.Message}");
                MessageBox.Show($"單一作業執行失敗：\n{ex.Message}", "作業中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                    blinkCts = null;
                }
                await RunSafeShutdownAsync();
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private async void btnFlipOnly_Click(object sender, EventArgs e)
        {
            if (cmbQuestions.SelectedIndex == 1)
            {
                await RunFlipOnlyQ1Async(); // 第1題指定作業
            }
            else if (cmbQuestions.SelectedIndex == 2)
            {
                await RunFlipOnlyAsync();
            }
        }

        private async Task RunFlipOnlyAsync()
        {
            // 執行前提：Y17 ON 且 X10 ON
            bool isY17On = pnlGL.Tag != null && (bool)pnlGL.Tag;
            bool isX10On = _xLeds[8].Tag != null && (bool)_xLeds[8].Tag;

            if (!isY17On || !isX10On)
            {
                MessageBox.Show("無法執行指定作業！\n前提條件：必須在綠燈待機 (Y17 ON) 且有進料 (X10 ON) 狀態下才能執行！", "前提條件不符", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行指定作業！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(false);
            AppendLog("INFO", "=== 開始執行指定作業流程 ===");
            CancellationTokenSource blinkCts = null;

            try
            {
                // Y17 off, Y15 開始閃爍 (每秒一次)
                await Task.Run(() => _comm.ForceCoil(15, false));
                blinkCts = new CancellationTokenSource();
                CancellationToken token = blinkCts.Token;
                _ = Task.Run(async () => {
                    try
                    {
                        bool state = true;
                        while (!token.IsCancellationRequested)
                        {
                            _comm.ForceCoil(13, state);
                            state = !state;
                            await Task.Delay(500, token);
                        }
                    }
                    catch { }
                }, token);

                // 2. 執行翻轉
                await ExecuteFlipSequenceAsync();

                // 3. Y4 ON 復歸夾具 (C-)
                AppendLog("INFO", "指定作業 [最後]: Y4 ON (C-)，等待 X4(c0) ON...");
                await Task.Run(() => {
                    _comm.ForceCoil(4, true);
                    _comm.ForceCoil(3, false);
                });
                if (!await WaitForInputStateAsync(4, true, 15))
                {
                    throw new Exception("等待 X4(c0) ON 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(4, false));

                // 結束閃爍，還原燈號
                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                    blinkCts = null;
                }

                AppendLog("INFO", "指定作業結束，點亮待機綠燈 (Y17)，關閉紅燈 (Y15)...");
                await Task.Run(() => {
                    _comm.ForceCoil(13, false);
                    _comm.ForceCoil(15, true);
                });

                AppendLog("INFO", "=== 指定作業流程成功執行完畢 ===");
                MessageBox.Show("指定作業已成功執行完畢！", "指定作業成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"指定作業中斷: {ex.Message}");
                MessageBox.Show($"指定作業執行失敗：\n{ex.Message}", "作業中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                    blinkCts = null;
                }
                await RunSafeShutdownAsync();
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private async void btnContinuousJob_Click(object sender, EventArgs e)
        {
            if (cmbQuestions.SelectedIndex == 1)
            {
                await RunJobQ1Async(true); // 第1題連續作業
            }
            else if (cmbQuestions.SelectedIndex == 2)
            {
                await RunContinuousJobAsync();
            }
        }

        private async Task RunContinuousJobAsync()
        {
            // 執行前提：Y17 ON 且 X10 ON
            bool isY17On = pnlGL.Tag != null && (bool)pnlGL.Tag;
            bool isX10On = _xLeds[8].Tag != null && (bool)_xLeds[8].Tag;

            if (!isY17On || !isX10On)
            {
                MessageBox.Show("無法執行連續作業！\n前提條件：必須在綠燈待機 (Y17 ON) 且有進料 (X10 ON) 狀態下才能執行！", "前提條件不符", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行連續作業！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(false);
            AppendLog("INFO", "=== 開始執行連續作業流程 ===");
            CancellationTokenSource blinkCts = null;

            try
            {
                while (true)
                {
                    // 1. Y17 off, Y15 on
                    await Task.Run(() => {
                        _comm.ForceCoil(15, false); // GL off
                        _comm.ForceCoil(13, true);  // RL on
                    });

                    // 2. 判斷 X12 (顏色)
                    bool isX12 = _xLeds[10].Tag != null && (bool)_xLeds[10].Tag;
                    if (!isX12)
                    {
                        AppendLog("INFO", "偵測到 X12 為 OFF (黑色料)，啟用 Y15 閃爍...");
                        if (blinkCts == null)
                        {
                            blinkCts = new CancellationTokenSource();
                            CancellationToken token = blinkCts.Token;
                            _ = Task.Run(async () => {
                                try
                                {
                                    bool state = true;
                                    while (!token.IsCancellationRequested)
                                    {
                                        _comm.ForceCoil(13, state);
                                        state = !state;
                                        await Task.Delay(500, token);
                                    }
                                }
                                catch { }
                            }, token);
                        }
                    }
                    else
                    {
                        AppendLog("INFO", "偵測到 X12 為 ON (紅色料)，Y15 保持恆亮。");
                        if (blinkCts != null)
                        {
                            blinkCts.Cancel();
                            blinkCts.Dispose();
                            blinkCts = null;
                        }
                        await Task.Run(() => _comm.ForceCoil(13, true));
                    }

                    // 3. 判斷 X11 (姿勢)
                    bool isX11 = _xLeds[9].Tag != null && (bool)_xLeds[9].Tag;
                    if (isX11)
                    {
                        AppendLog("INFO", "X11 為 ON，直接進入移料步驟。");
                    }
                    else
                    {
                        AppendLog("INFO", "X11 為 OFF，執行翻轉步驟...");
                        await ExecuteFlipSequenceAsync();
                    }

                    // 4. 移料步驟
                    await ExecuteMoveMaterialSequenceAsync();

                    // 5. 移料完成後，先關閉閃爍，恢復 Y15 恆亮
                    if (blinkCts != null)
                    {
                        blinkCts.Cancel();
                        blinkCts.Dispose();
                        blinkCts = null;
                    }
                    await Task.Run(() => _comm.ForceCoil(13, true)); // 恢復恆亮

                    // 6. 監控 X10 進料狀態
                    AppendLog("INFO", "本輪連續作業移料完成。開始監控 X10 進料狀態...");
                    bool nextRound = false;
                    bool isX10Now = _xLeds[8].Tag != null && (bool)_xLeds[8].Tag;

                    if (isX10Now)
                    {
                        AppendLog("INFO", "X10 已為 ON，暫停 3 秒 (T3) 後重複連續作業。");
                        await Task.Delay(3000);
                        nextRound = true;
                    }
                    else
                    {
                        AppendLog("INFO", "X10 為 OFF，啟動 10 秒進料等待...");
                        for (int i = 0; i < 100; i++) // 100 * 100ms = 10秒
                        {
                            await Task.Delay(100);
                            if (_xLeds[8].Tag != null && (bool)_xLeds[8].Tag)
                            {
                                AppendLog("INFO", "在 10 秒內偵測到進料 (X10 ON)！暫停 3 秒 (T3) 後重複連續作業。");
                                await Task.Delay(3000);
                                nextRound = true;
                                break;
                            }
                        }
                    }

                    if (!nextRound)
                    {
                        AppendLog("INFO", "連續作業結束：超過 10 秒無進料 (X10 OFF)。熄滅運轉燈，點亮待機綠燈...");
                        await Task.Run(() => {
                            _comm.ForceCoil(13, false); // RL off
                            _comm.ForceCoil(15, true);  // GL on (待機)
                        });
                        MessageBox.Show("連續作業因超過 10 秒無進料而自動結束並復歸待機狀態。", "連續作業結束", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break; // 退出迴圈
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"連續作業中斷: {ex.Message}");
                MessageBox.Show($"連續作業執行失敗：\n{ex.Message}", "連續作業中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                    blinkCts = null;
                }
                await RunSafeShutdownAsync();
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        // Q2 翻轉副流程
        private async Task ExecuteFlipSequenceAsync()
        {
            // Y2 off (B-), 等待退回 X3
            AppendLog("INFO", "翻轉步驟 [1/5]: Y2 OFF...");
            await Task.Run(() => _comm.ForceCoil(2, false));
            if (!await WaitForInputStateAsync(3, true, 15))
            {
                throw new Exception("等待 B缸退回 X3(b0) 逾時！");
            }

            // Y3 on (C+) 直到 X4 off
            AppendLog("INFO", "翻轉步驟 [2/5]: Y3 ON (C+)，等待 X4 OFF...");
            await Task.Run(() => {
                _comm.ForceCoil(3, true);
                _comm.ForceCoil(4, false);
            });
            if (!await WaitForInputStateAsync(4, false, 15))
            {
                throw new Exception("等待夾具夾緊 X4 OFF 逾時！");
            }
            await Task.Run(() => _comm.ForceCoil(3, false));

            // T1 (1秒)
            AppendLog("INFO", "翻轉步驟 [3/5]: 暫停 1 秒...");
            await Task.Delay(1000);

            // Y0 on (A+) 直到 X0 on
            AppendLog("INFO", "翻轉步驟 [4/5]: Y0 ON (A+)，等待 X0 ON...");
            await Task.Run(() => {
                _comm.ForceCoil(0, true);
                _comm.ForceCoil(1, false);
            });
            if (!await WaitForInputStateAsync(0, true, 15))
            {
                throw new Exception("等待 A缸頂起 X0(a1) ON 逾時！");
            }
            await Task.Run(() => _comm.ForceCoil(0, false));

            // 旋轉判斷 X6 / X7
            bool isX6 = _xLeds[6].Tag != null && (bool)_xLeds[6].Tag;
            bool isX7 = _xLeds[7].Tag != null && (bool)_xLeds[7].Tag;

            if (isX6)
            {
                AppendLog("INFO", "翻轉分支 [X6 ON]: Y7 ON，等待 X7 ON...");
                await Task.Run(() => {
                    _comm.ForceCoil(7, true);
                    _comm.ForceCoil(6, false);
                });
                if (!await WaitForInputStateAsync(7, true, 15))
                {
                    throw new Exception("旋轉等待 X7 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(7, false));
            }
            else if (isX7)
            {
                AppendLog("INFO", "翻轉分支 [X7 ON]: Y6 ON，等待 X6 ON...");
                await Task.Run(() => {
                    _comm.ForceCoil(6, true);
                    _comm.ForceCoil(7, false);
                });
                if (!await WaitForInputStateAsync(6, true, 15))
                {
                    throw new Exception("旋轉等待 X6 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(6, false));
            }
            else
            {
                AppendLog("WARNING", "未偵測到 X6 或 X7 ON，略過旋轉步驟。");
            }

            // T1 (1秒)
            AppendLog("INFO", "翻轉後等待: 暫停 1 秒...");
            await Task.Delay(1000);

            // Y1 on (A-) 直到 X1 on
            AppendLog("INFO", "翻轉步驟 [5/5]: Y1 ON (A-)，等待 X1 ON...");
            await Task.Run(() => {
                _comm.ForceCoil(1, true);
                _comm.ForceCoil(0, false);
            });
            if (!await WaitForInputStateAsync(1, true, 15))
            {
                throw new Exception("等待 A缸退回 X1(a0) ON 逾時！");
            }
            await Task.Run(() => _comm.ForceCoil(1, false));

            AppendLog("INFO", "翻轉子流程順利執行完畢。");
        }

        // Q2 移料副流程
        private async Task ExecuteMoveMaterialSequenceAsync()
        {
            // Y3 on (C+) 直到 X4 off
            AppendLog("INFO", "移料步驟 [1/6]: Y3 ON (C+)，等待 X4 OFF...");
            await Task.Run(() => {
                _comm.ForceCoil(3, true);
                _comm.ForceCoil(4, false);
            });
            if (!await WaitForInputStateAsync(4, false, 15))
            {
                throw new Exception("等待夾緊 X4 OFF 逾時！");
            }
            await Task.Run(() => _comm.ForceCoil(3, false));

            // Y2 持續 ON
            AppendLog("INFO", "移料步驟 [2/6]: Y2 (B+) 持續 ON...");
            await Task.Run(() => _comm.ForceCoil(2, true));

            // 等待 X2 on
            AppendLog("INFO", "移料步驟 [3/6]: 等待 B缸到位 X2 ON...");
            if (!await WaitForInputStateAsync(2, true, 15))
            {
                throw new Exception("等待 X2 ON 逾時！");
            }

            // 當 X2 on，Y5 on (D+) 直到 X5 on (ps1)
            AppendLog("INFO", "移料步驟 [4/6]: Y5 ON (D+)，等待真空吸附 X5(ps1) ON...");
            await Task.Run(() => {
                _comm.ForceCoil(5, true);
                _comm.ForceCoil(8, false); // Y10 off
            });
            if (!await WaitForInputStateAsync(5, true, 15))
            {
                throw new Exception("等待吸住 X5 ON 逾時！");
            }
            await Task.Run(() => _comm.ForceCoil(5, false));

            // 當 X5 on，Y4 on (C-) 直到 X4 off
            AppendLog("INFO", "移料步驟 [5/6]: Y4 ON (C-) 釋放夾具，等待 X4 OFF...");
            await Task.Run(() => {
                _comm.ForceCoil(4, true);
                _comm.ForceCoil(3, false);
            });
            if (!await WaitForInputStateAsync(4, false, 15))
            {
                throw new Exception("等待夾具離去 X4 OFF 逾時！");
            }
            await Task.Run(() => _comm.ForceCoil(4, false));

            // 當 X4 off 時，Y2 off (B-)
            AppendLog("INFO", "移料步驟 [6/6]: Y2 OFF (B-)...");
            await Task.Run(() => _comm.ForceCoil(2, false));

            // 等待 X3 on，Y10 on (D-) 直到 X5 off
            AppendLog("INFO", "等待 B缸回到 X3(b0) ON...");
            if (!await WaitForInputStateAsync(3, true, 15))
            {
                throw new Exception("等待 B缸退回 X3 ON 逾時！");
            }

            AppendLog("INFO", "移料完畢釋放：Y10 ON (D-)，等待真空釋放 X5 OFF...");
            await Task.Run(() => {
                _comm.ForceCoil(8, true); // Y10 ON
                _comm.ForceCoil(5, false); // Y5 off
            });
            if (!await WaitForInputStateAsync(5, false, 15))
            {
                throw new Exception("等待真空壓釋放 X5 OFF 逾時！");
            }
            await Task.Run(() => _comm.ForceCoil(8, false));

            AppendLog("INFO", "移料子流程順利執行完畢。");
        }

        // Q1 專屬單一/連續流程
        private async Task RunJobQ1Async(bool isContinuous)
        {
            // 執行前提：Y16 (待機) 為 ON
            bool isY16On = _yLeds[14].Tag != null && (bool)_yLeds[14].Tag; // Y16 index 14
            if (!isY16On)
            {
                string jobName = isContinuous ? "連續作業" : "單一作業";
                MessageBox.Show($"無法執行{jobName}！\n前提條件：必須在待機 (Y16 ON) 狀態下才能執行！\n請先進行「復歸」作業。", "前提條件不符", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行作業！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(false);
            string flowName = isContinuous ? "連續作業" : "單一作業";
            AppendLog("INFO", $"=== 開始執行第 1 題{flowName}流程 ===");

            try
            {
                while (true)
                {
                    // 1. Y16 off, Y15 on (紅運轉燈亮)
                    await Task.Run(() => {
                        _comm.ForceCoil(14, false); // Y16 off
                        _comm.ForceCoil(13, true);  // Y15 on
                    });

                    // 2. Y2 on 十秒，若十秒內 X2 on 後再一秒才 Y2 off；若超時則關閉 Y2 並回到待機
                    AppendLog("INFO", "步驟 [1/5]: Y2 (M1輸送帶) ON，等待 X2 ON (最多等待10秒)...");
                    await Task.Run(() => _comm.ForceCoil(2, true));
                    bool x2Success = false;
                    for (int t = 0; t < 100; t++)
                    {
                        await Task.Delay(100);
                        if (_xLeds[2].Tag != null && (bool)_xLeds[2].Tag)
                        {
                            AppendLog("INFO", "偵測到 X2 ON，繼續運轉 1 秒後關閉 Y2...");
                            await Task.Delay(1000);
                            x2Success = true;
                            break;
                        }
                    }
                    await Task.Run(() => _comm.ForceCoil(2, false));

                    if (!x2Success)
                    {
                        AppendLog("WARNING", "超過 10 秒未偵測到 X2 ON，作業中斷並退回待機。");
                        await Task.Run(() => {
                            _comm.ForceCoil(13, false);
                            _comm.ForceCoil(14, true);
                        });
                        MessageBox.Show("超時無回應 (未於10秒內收到 X2 ON 訊號)！", "作業中斷", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 3. 判斷 X3 狀態決定排料點
                    bool isX3 = _xLeds[3].Tag != null && (bool)_xLeds[3].Tag; // X3 index 3
                    int dischargePoint = isX3 ? 5 : 6;
                    AppendLog("INFO", $"步驟 [2/5]: X3 狀態為 {(isX3 ? "ON" : "OFF")}，排料分流點為 X{dischargePoint}");

                    // 4. Y0 on, X0 on 後 0.5 秒再 Y6 一秒，Y0 off
                    AppendLog("INFO", "步驟 [3/5]: Y0 (A缸下) ON，等待 X0 ON...");
                    await Task.Run(() => _comm.ForceCoil(0, true));
                    if (!await WaitForInputStateAsync(0, true, 15))
                    {
                        throw new Exception("等待 X0 ON 逾時！");
                    }
                    AppendLog("INFO", "X0 已為 ON，暫停 0.5 秒後開啟夾具 Y6...");
                    await Task.Delay(500);
                    AppendLog("INFO", "Y6 (B+夾) ON 保持 1 秒，並關閉 Y0...");
                    await Task.Run(() => {
                        _comm.ForceCoil(6, true);
                        _comm.ForceCoil(0, false);
                    });
                    await Task.Delay(1000);
                    await Task.Run(() => _comm.ForceCoil(6, false));

                    // 5. 移到排料點
                    if (dischargePoint == 5)
                    {
                        // 排料至 X5
                        AppendLog("INFO", "步驟 [4/5]: 目標排料點為 X5，Y7 (B-放) ON 1秒...");
                        await Task.Run(() => _comm.ForceCoil(7, true));
                        await Task.Delay(1000);
                        await Task.Run(() => _comm.ForceCoil(7, false));

                        AppendLog("INFO", "步驟 [5/5]: Y5 (M2-右移) ON 直到 X2 ON...");
                        await Task.Run(() => {
                            _comm.ForceCoil(5, true);
                            _comm.ForceCoil(8, false); // Y10 off
                        });
                        if (!await WaitForInputStateAsync(2, true, 15))
                        {
                            throw new Exception("等待 X2 ON 逾時！");
                        }
                        await Task.Run(() => _comm.ForceCoil(5, false));
                    }
                    else
                    {
                        // 排料至 X6
                        AppendLog("INFO", "步驟 [4/5]: 目標排料點為 X6，Y0 ON...");
                        await Task.Run(() => _comm.ForceCoil(0, true));
                        if (!await WaitForInputStateAsync(0, true, 15))
                        {
                            throw new Exception("等待 X0 ON 逾時！");
                        }
                        AppendLog("INFO", "X0 ON，延遲 0.5 秒開啟 Y7...”");
                        await Task.Delay(500);
                        AppendLog("INFO", "Y7 (B-放) ON 保持 1 秒，並關閉 Y0...");
                        await Task.Run(() => {
                            _comm.ForceCoil(7, true);
                            _comm.ForceCoil(0, false);
                        });
                        await Task.Delay(1000);
                        await Task.Run(() => _comm.ForceCoil(7, false));

                        AppendLog("INFO", "步驟 [5/5]: 等待 A缸頂起 X1 ON...");
                        if (!await WaitForInputStateAsync(1, true, 15))
                        {
                            throw new Exception("等待 X1 ON 逾時！");
                        }
                        AppendLog("INFO", "Y5 ON 直到 X2 ON...");
                        await Task.Run(() => {
                            _comm.ForceCoil(5, true);
                            _comm.ForceCoil(8, false);
                        });
                        if (!await WaitForInputStateAsync(2, true, 15))
                        {
                            throw new Exception("等待 X2 ON 逾時！");
                        }
                        await Task.Run(() => _comm.ForceCoil(5, false));
                    }

                    AppendLog("INFO", "排料完成。");

                    if (!isContinuous)
                    {
                        // 單一作業：結束並復歸待機
                        await Task.Run(() => {
                            _comm.ForceCoil(13, false); // Y15 off
                            _comm.ForceCoil(14, true);  // Y16 on
                        });
                        MessageBox.Show("單一作業已成功執行完畢！", "作業成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }
                    else
                    {
                        AppendLog("INFO", "連續作業：等待 1 秒後開始下一輪流程...");
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"第 1 題作業中斷: {ex.Message}");
                MessageBox.Show($"作業執行失敗：\n{ex.Message}", "作業中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await RunSafeShutdownAsync();
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        // Q1 指定作業 (指定翻轉)
        private async Task RunFlipOnlyQ1Async()
        {
            bool isY16On = _yLeds[14].Tag != null && (bool)_yLeds[14].Tag;
            if (!isY16On)
            {
                MessageBox.Show("無法執行指定作業！\n前提條件：必須在待機 (Y16 ON) 狀態下才能執行！", "前提條件不符", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行指定作業！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(false);
            AppendLog("INFO", "=== 開始執行第 1 題指定作業 ===");
            try
            {
                await Task.Run(() => {
                    _comm.ForceCoil(14, false);
                    _comm.ForceCoil(13, true);
                });

                bool[] xStates = await Task.Run(() => _comm.ReadDeviceStates(true));

                bool y2 = xStates[7];
                bool y0 = xStates[10] && xStates[2];
                bool y4 = xStates[11] && !xStates[6];
                bool y5 = xStates[12] && !xStates[2];

                AppendLog("INFO", $"指定邏輯判定下發：Y2={y2}, Y0={y0}, Y4={y4}, Y5={y5}");

                await Task.Run(() => {
                    _comm.ForceCoil(2, y2);
                    _comm.ForceCoil(0, y0);
                    _comm.ForceCoil(4, y4);
                    _comm.ForceCoil(5, y5);
                });

                await Task.Delay(1000); // 運轉維持1秒

                // 清除所有輸出回到待機
                await Task.Run(() => {
                    _comm.ForceCoil(2, false);
                    _comm.ForceCoil(0, false);
                    _comm.ForceCoil(4, false);
                    _comm.ForceCoil(5, false);
                    _comm.ForceCoil(13, false);
                    _comm.ForceCoil(14, true);
                });

                AppendLog("INFO", "=== 第 1 題指定作業成功執行完畢 ===");
                MessageBox.Show("指定作業已成功執行完畢！", "指定作業成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"指定作業失敗: {ex.Message}");
                MessageBox.Show($"指定作業執行失敗：\n{ex.Message}", "作業中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await RunSafeShutdownAsync();
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private async Task RunSafeShutdownAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i <= 8; i++) _comm.ForceCoil(i, false);
                    if (cmbQuestions.SelectedIndex == 1)
                    {
                        _comm.ForceCoil(13, false);
                        _comm.ForceCoil(14, true); // Q1 待機黃
                    }
                    else if (cmbQuestions.SelectedIndex == 2)
                    {
                        _comm.ForceCoil(13, false);
                        _comm.ForceCoil(15, true); // Q2 待機綠
                    }
                });
            }
            catch { }
            ResetLeds();
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnRunSequence.Enabled = enabled && _isConnectionNormal;
            btnStartFlow.Enabled = enabled && _isConnectionNormal;
            txtSequence.Enabled = enabled;

            if (enabled && _comm != null && _comm.IsOpen && _isConnectionNormal)
            {
                bool isQ1OrQ2 = (cmbQuestions.SelectedIndex == 1 || cmbQuestions.SelectedIndex == 2);
                btnStandby.Enabled = isQ1OrQ2;
                btnSingleJob.Enabled = isQ1OrQ2;
                btnFlipOnly.Enabled = isQ1OrQ2;
                btnContinuousJob.Enabled = isQ1OrQ2;
            }
            else
            {
                btnStandby.Enabled = false;
                btnSingleJob.Enabled = false;
                btnFlipOnly.Enabled = false;
                btnContinuousJob.Enabled = false;
            }
        }

        private void UpdateButtonsVisualState(bool isNormal)
        {
            if (isNormal)
            {
                btnStartFlow.BackColor = _colorStartFlowBack;
                btnStartFlow.ForeColor = Color.White;
                
                btnRunSequence.BackColor = _colorRunSequenceBack;
                btnRunSequence.ForeColor = Color.White;
                
                btnStandby.BackColor = _colorStandbyBack;
                btnStandby.ForeColor = Color.White;
                
                btnSingleJob.BackColor = _colorSingleJobBack;
                btnSingleJob.ForeColor = Color.White;
                
                btnFlipOnly.BackColor = _colorFlipOnlyBack;
                btnFlipOnly.ForeColor = Color.White;
                
                btnContinuousJob.BackColor = _colorContinuousJobBack;
                btnContinuousJob.ForeColor = Color.White;
            }
            else
            {
                Color grayBg = Color.FromArgb(142, 142, 147);
                Color blackText = Color.Black;
                
                btnStartFlow.BackColor = grayBg;
                btnStartFlow.ForeColor = blackText;
                
                btnRunSequence.BackColor = grayBg;
                btnRunSequence.ForeColor = blackText;
                
                btnStandby.BackColor = grayBg;
                btnStandby.ForeColor = blackText;
                
                btnSingleJob.BackColor = grayBg;
                btnSingleJob.ForeColor = blackText;
                
                btnFlipOnly.BackColor = grayBg;
                btnFlipOnly.ForeColor = blackText;
                
                btnContinuousJob.BackColor = grayBg;
                btnContinuousJob.ForeColor = blackText;
            }
        }

        private void chkShowLog_CheckedChanged(object sender, EventArgs e)
        {
            panelBottom.Visible = chkShowLog.Checked;
            if (chkShowLog.Checked)
            {
                this.MinimumSize = new System.Drawing.Size(1024, 670);
                this.Height = 670;
            }
            else
            {
                this.MinimumSize = new System.Drawing.Size(1024, 470);
                this.Height = 470;
            }
        }
    }
}
