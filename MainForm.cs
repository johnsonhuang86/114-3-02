using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
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

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeCustomUI();
            RefreshComPorts();
            
            // 初始化題目選擇選單
            cmbQuestions.Items.Add("請選擇題目 (預設)...");
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

            AppendLog("INFO", "程式啟動。請選擇通訊埠並點擊「進行連線」開始監控。");
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
                AppendLog("WARNING", "系統未偵測到任何實體 COM Port。已載入模擬埠。");
            }

            // 依最寬項目自動調整 ComboBox 寬度
            AdjustComboBoxWidth(cmbPorts);
        }

        private void InitializeCustomUI()
        {
            // 動態配置 X0~X7 與 X10~X17 控制項到 tblX (2列8行)
            // 第一欄：X0~X7，第二欄：X10~X17
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
            // 第一欄：Y0~Y7，第二欄：Y10~Y17
            for (int row = 0; row < 8; row++)
            {
                // Y0~Y7
                int indexLeft = row;
                string octalLeft = indexLeft.ToString();
                var pnlLeft = CreateLedItemPanel("Y" + octalLeft, indexLeft, true);
                tblY.Controls.Add(pnlLeft, 0, row);

                // Y10~Y17
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

            // 使用微型 TableLayoutPanel 實現自適應縮放與居中排版
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = isY ? 3 : 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            // 設定欄寬比例或絕對值
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35F)); // LED 燈號欄 (固定 35 像素)
            if (isY)
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // 名稱欄 (佔用剩餘比例)
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75F));  // 按鈕欄 (固定 75 像素)
            }
            else
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // 名稱欄 (佔用剩餘比例)
            }
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // LED 燈圓形 Panel
            Panel ledPanel = new Panel
            {
                Size = new Size(18, 18),
                Anchor = AnchorStyles.None, // 垂直與水平自動置中
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
                Anchor = AnchorStyles.Left, // 靠左垂直置中
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

            // 如果是 Y，需要加上控制按鈕
            if (isY)
            {
                Button btn = new Button
                {
                    Text = "切換",
                    Size = new Size(65, 24),
                    Anchor = AnchorStyles.Right, // 靠右垂直置中
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

            // 決定燈號的主體色彩
            Color ledColor;
            Color lightBorderColor;
            Color lightGradientColor;

            if (isOn)
            {
                if (p.Name == "pnlRL")
                {
                    ledColor = Color.FromArgb(255, 69, 58); // 紅
                    lightGradientColor = Color.FromArgb(255, 140, 130);
                    lightBorderColor = Color.FromArgb(255, 180, 180);
                }
                else if (p.Name == "pnlYL")
                {
                    ledColor = Color.FromArgb(255, 214, 10); // 黃
                    lightGradientColor = Color.FromArgb(255, 240, 150);
                    lightBorderColor = Color.FromArgb(255, 240, 180);
                }
                else
                {
                    // 預設為綠色
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

            // 繪製高質感白光反射點
            if (isOn)
            {
                using (var highlightBrush = new SolidBrush(Color.FromArgb(180, Color.White)))
                {
                    e.Graphics.FillEllipse(highlightBrush, 4, 4, 3, 3);
                }
            }

            // 繪製外圈邊框
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
                btnStartFlow.Enabled = true;
                btnRunSequence.Enabled = true;

                // 復歸、單一作業、指定作業、連續作業為第2題專用
                bool isQ2 = (cmbQuestions.SelectedIndex == 2); // 第2題在 Index 2
                btnStandby.Enabled = isQ2;
                btnSingleJob.Enabled = isQ2;
                btnFlipOnly.Enabled = isQ2;
                btnContinuousJob.Enabled = isQ2;

                AppendLog("INFO", "連線建立成功，開始輪詢狀態。");
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
            btnStartFlow.Enabled = false;
            btnRunSequence.Enabled = false;
            btnStandby.Enabled = false;
            btnSingleJob.Enabled = false;
            btnFlipOnly.Enabled = false;
            btnContinuousJob.Enabled = false;

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

            // 重置所有燈號狀態為 OFF
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
            // 防止重入
            if (_isPolling || _comm == null || !_comm.IsOpen) return;
            _isPolling = true;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // 非同步讀取 X0~X17 與 Y0~Y17 狀態，確保 UI 不會卡頓
                bool[] xStates = await Task.Run(() => _comm.ReadDeviceStates(true));
                bool[] yStates = await Task.Run(() => _comm.ReadDeviceStates(false));

                // 回到 UI 執行緒後更新 UI 燈號
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

                // 同步紅、黃、綠指示燈與 Y15、Y16、Y17 的狀態
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

                // 第 2 題 Y17 (待機綠燈) 狀態動態邏輯判定：
                // 條件: (Y0~Y10皆為off) AND (X5 off) AND (X1、X3、X4皆on) AND (X6 OR X7為on)
                if (cmbQuestions.SelectedIndex == 2 && btnRunSequence.Enabled)
                {
                    bool condY0_10Off = true;
                    for (int j = 0; j <= 8; j++) // Coil 0~8 對應 Y0~Y7, Y10
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
                        AppendLog("INFO", $"第2題待機聯鎖改變，自動設定 Y17 待機綠燈為 {(targetY17 ? "ON" : "OFF")}。");
                    }
                }

                watch.Stop();
                lblPollTime.Text = $"輪詢時間: {watch.ElapsedMilliseconds} ms";
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"狀態更新失敗: {ex.Message}");
                // 如果是嚴重的通訊中斷，我們主動斷連，避免無窮報錯
                if (_comm == null || !_comm.IsOpen)
                {
                    btnDisconnect_Click(this, EventArgs.Empty);
                    MessageBox.Show("通訊發生異常，已自動中斷連線。\n詳細原因: " + ex.Message, "連線中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            btn.Enabled = false; // 暫時停用，避免快速連擊
            try
            {
                string octStr = GetOctalString(index);
                AppendLog("INFO", $"正發送控制 Y{octStr} 為 {(targetVal ? "ON" : "OFF")}...");
                
                // 異步執行強制單點控制，避免按鈕點擊造成視窗短暫凍結
                await Task.Run(() => _comm.ForceCoil(index, targetVal));
                
                // 成功後立即將對應的燈號先更新，提供即時的視覺回饋
                _yLeds[index].Tag = targetVal;
                _yLeds[index].Invalidate();
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"控制 Y{GetOctalString(index)} 失敗: {ex.Message}");
                MessageBox.Show($"寫入控制指令時發生錯誤:\n{ex.Message}", "寫入失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn.Enabled = true;
            }
        }

        private void Comm_LogMessage(string type, string msg)
        {
            // 因 Serial 讀寫程序在 Task.Run 非同步執行緒中，這裡必須使用 Invoke 回到 UI 執行緒
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string, string>(Comm_LogMessage), type, msg);
                return;
            }

            // 判斷是否只顯示一般訊息或 Hex 資料
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
            
            // 加入日誌文字
            txtLog.AppendText(prefix + msg + Environment.NewLine);

            // 控制日誌大小 (防止記憶體無限增長，保持最大約 500 行)
            if (txtLog.Lines.Length > 500)
            {
                string[] newLines = new string[300];
                Array.Copy(txtLog.Lines, txtLog.Lines.Length - 300, newLines, 0, 300);
                txtLog.Lines = newLines;
            }

            // 捲動到最底端
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
            // 在視窗縮放時，動態微調 splitContainer 的分割距離，保持左右群組 45% : 55% 的黃金比例
            splitContainer.SplitterDistance = (int)(splitContainer.Width * 0.45);
        }

        private async void btnStartFlow_Click(object sender, EventArgs e)
        {
            if (_comm == null || !_comm.IsOpen) return;

            btnStartFlow.Enabled = false;
            AppendLog("INFO", "=== 啟動三色燈自動流程控制 ===");
            try
            {
                // 1. Y15 (RL 紅燈) ON 1秒後熄滅
                AppendLog("INFO", "流程 [1/3]: Y15 (RL 紅色指示燈) ON...");
                await Task.Run(() => _comm.ForceCoil(13, true));
                pnlRL.Tag = true;
                pnlRL.Invalidate();
                _yLeds[13].Tag = true;
                _yLeds[13].Invalidate();
                await Task.Delay(1000);

                AppendLog("INFO", "流程 [1/3]: Y15 (RL 紅色指示燈) OFF...");
                await Task.Run(() => _comm.ForceCoil(13, false));
                pnlRL.Tag = false;
                pnlRL.Invalidate();
                _yLeds[13].Tag = false;
                _yLeds[13].Invalidate();

                // 2. Y16 (YL 黃燈) ON 1秒後熄滅
                AppendLog("INFO", "流程 [2/3]: Y16 (YL 黃色指示燈) ON...");
                await Task.Run(() => _comm.ForceCoil(14, true));
                pnlYL.Tag = true;
                pnlYL.Invalidate();
                _yLeds[14].Tag = true;
                _yLeds[14].Invalidate();
                await Task.Delay(1000);

                AppendLog("INFO", "流程 [2/3]: Y16 (YL 黃色指示燈) OFF...");
                await Task.Run(() => _comm.ForceCoil(14, false));
                pnlYL.Tag = false;
                pnlYL.Invalidate();
                _yLeds[14].Tag = false;
                _yLeds[14].Invalidate();

                // 3. Y17 (GL 綠燈) ON 1秒後熄滅
                AppendLog("INFO", "流程 [3/3]: Y17 (GL 綠色指示燈) ON...");
                await Task.Run(() => _comm.ForceCoil(15, true));
                pnlGL.Tag = true;
                pnlGL.Invalidate();
                _yLeds[15].Tag = true;
                _yLeds[15].Invalidate();
                await Task.Delay(1000);

                AppendLog("INFO", "流程 [3/3]: Y17 (GL 綠色指示燈) OFF...");
                await Task.Run(() => _comm.ForceCoil(15, false));
                pnlGL.Tag = false;
                pnlGL.Invalidate();
                _yLeds[15].Tag = false;
                _yLeds[15].Invalidate();

                AppendLog("INFO", "=== 三色燈自動流程控制順利結束 ===");
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"流程控制中斷: {ex.Message}");
                MessageBox.Show($"流程控制執行失敗:\n{ex.Message}", "流程中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // 發生異常時，強制關閉 Y15~Y17 確保硬體安全
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
            // 第 1 題 (機電丙一)
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

            // 第 2 題 (機電丙二)
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

            // 第 3 題 (機電丙三)
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

            // 第 4 題 (機電丙四)
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

            // 第 5 題 (機電丙五)
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
                // 重置點位名稱為預設
                ResetIoNames();
                AppendLog("INFO", "重置 IO 點位名稱為預設值。");
                return;
            }

            try
            {
                // 直接使用已轉為 UTF-8 的內嵌字串資料，免去讀取外部檔案
                string ioDataText = QuestionIoData[selectedIndex - 1];
                LoadIoNamesFromText(ioDataText);
                AppendLog("INFO", $"成功載入第 {selectedIndex} 題 (機電丙{GetChineseNumber(selectedIndex)}) 的 IO 表。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析記憶體中題目 {selectedIndex} 的 IO 表時發生錯誤：\n{ex.Message}", "載入失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog("ERROR", $"解析題目 {selectedIndex} 資料時發生錯誤: {ex.Message}");
                ResetIoNames();
            }
        }

        private void LoadIoNamesFromText(string text)
        {
            string[] xNames = new string[16];
            string[] yNames = new string[16];

            // 解析 UTF-8 字串內容
            string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // 使用空格或定位鍵分割元件編號與名稱
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

            // 更新 UI Labels
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

        private string GetChineseNumber(int num)
        {
            switch (num)
            {
                case 1: return "一";
                case 2: return "二";
                case 3: return "三";
                case 4: return "四";
                case 5: return "五";
                default: return num.ToString();
            }
        }

        private void AdjustComboBoxWidth(ComboBox comboBox)
        {
            int maxWidth = 0;
            using (Graphics g = comboBox.CreateGraphics())
            {
                foreach (var item in comboBox.Items)
                {
                    string text = item.ToString();
                    // 量測文字寬度並加入 30 像素作為下拉箭頭與邊框的 Padding 緩衝
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
                    i++; // 忽略空格
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
            int loops = timeoutSeconds * 10; // 每個 loop 100ms
            for (int i = 0; i < loops; i++)
            {
                if (_xLeds[xIndex].Tag != null && (bool)_xLeds[xIndex].Tag == expectedState)
                {
                    return true;
                }
                await Task.Delay(100);
            }
            return false; // 超時
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

            // 停用 UI 控制項
            btnRunSequence.Enabled = false;
            btnStandby.Enabled = false;
            btnSingleJob.Enabled = false;
            btnFlipOnly.Enabled = false;
            btnContinuousJob.Enabled = false;
            btnStartFlow.Enabled = false;
            txtSequence.Enabled = false;
            AppendLog("INFO", $"=== 開始執行動作序列: {seqText} ===");

            try
            {
                for (int stepIdx = 0; stepIdx < steps.Count; stepIdx++)
                {
                    string step = steps[stepIdx];
                    AppendLog("INFO", $"步驟 [{stepIdx + 1}/{steps.Count}]: 正在執行 {step}...");

                    if (step.StartsWith("T"))
                    {
                        // 暫停延時
                        int seconds = int.Parse(step.Substring(1));
                        AppendLog("INFO", $"暫停 {seconds} 秒...");
                        await Task.Delay(seconds * 1000);
                    }
                    else
                    {
                        char cylinder = step[0];
                        char action = step[1];

                        int outputY1 = -1;
                        int outputY2 = -1; // 用於雙閥的另一個點
                        bool stateY1 = false;
                        bool stateY2 = false;
                        
                        int waitX = -1;
                        bool expectedX = false;
                        
                        // 到位後要復歸為 false 的輸出點 (只適用雙閥)
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
                                cleanupY = 0;   // 到位後關閉 Y0
                            }
                            else // '-'
                            {
                                outputY1 = 1;   // Y1
                                stateY1 = true;
                                outputY2 = 0;   // Y0
                                stateY2 = false;
                                
                                waitX = 1;      // X1 (a0)
                                expectedX = true;
                                cleanupY = 1;   // 到位後關閉 Y1
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
                                // 單閥，到位後不清理，維持 Y2=true 以免彈簧縮回
                                cleanupY = -1; 
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
                                
                                waitX = 4;      // X4 (c0 off)
                                expectedX = false;
                                cleanupY = 3;   // 到位後關閉 Y3
                            }
                            else // '-'
                            {
                                outputY1 = 4;   // Y4
                                stateY1 = true;
                                outputY2 = 3;   // Y3
                                stateY2 = false;
                                
                                waitX = 4;      // X4 (c0 on)
                                expectedX = true;
                                cleanupY = 4;   // 到位後關閉 Y4
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
                                
                                waitX = 5;      // X5 (ps1 on)
                                expectedX = true;
                                cleanupY = 5;   // 到位後關閉 Y5
                            }
                            else // '-'
                            {
                                outputY1 = 8;   // Y10 (index 8)
                                stateY1 = true;
                                outputY2 = 5;   // Y5
                                stateY2 = false;
                                
                                waitX = 5;      // X5 off
                                expectedX = false;
                                cleanupY = 8;   // 到位後關閉 Y10
                            }
                        }

                        // 1. 送出 PLC 指令
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

                        // 2. 等待極限開關狀態
                        if (waitX != -1)
                        {
                            string waitName;
                            if (waitX == 4)
                            {
                                waitName = expectedX ? "X4(c0 on)" : "X4(c0 off)";
                            }
                            else if (waitX == 5)
                            {
                                waitName = expectedX ? "X5(ps1 on)" : "X5(ps1 off)";
                            }
                            else
                            {
                                waitName = $"X{waitX}";
                            }
                            
                            AppendLog("INFO", $"等待極限開關 {waitName} 到達目標狀態: {expectedX}...");
                            
                            bool waitSuccess = await WaitForInputStateAsync(waitX, expectedX, 15); // 15秒超時
                            if (!waitSuccess)
                            {
                                throw new Exception($"等待步驟 {step} 的極限開關 {waitName} 超時 (15秒)！氣壓缸可能卡住或感測器失效。");
                            }
                            AppendLog("INFO", $"步驟 {step} 到位成功。");
                        }

                        // 3. 到位後「停止」動作 (將雙閥 Y 點關閉)
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

                // 安全機制：發生錯誤時，強制將所有相關的輸出 (Y0~Y6, Y10) 關閉，避免硬體損壞
                try
                {
                    await Task.Run(() =>
                    {
                        _comm.ForceCoil(0, false);
                        _comm.ForceCoil(1, false);
                        _comm.ForceCoil(2, false);
                        _comm.ForceCoil(3, false);
                        _comm.ForceCoil(4, false);
                        _comm.ForceCoil(5, false);
                        _comm.ForceCoil(6, false); // Y6
                        _comm.ForceCoil(8, false); // Y10
                    });
                }
                catch { }
            }
            finally
            {
                // 恢復 UI 按鈕狀態
                if (_comm != null && _comm.IsOpen)
                {
                    btnRunSequence.Enabled = true;
                    btnStartFlow.Enabled = true;
                    txtSequence.Enabled = true;

                    bool isQ2 = (cmbQuestions.SelectedIndex == 2);
                    btnStandby.Enabled = isQ2;
                    btnSingleJob.Enabled = isQ2;
                    btnFlipOnly.Enabled = isQ2;
                    btnContinuousJob.Enabled = isQ2;
                }
            }
        }

        private async void btnStandby_Click(object sender, EventArgs e)
        {
            await RunStandbySequenceAsync();
        }

        private async Task RunStandbySequenceAsync()
        {
            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行待機復歸！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 停用 UI 控制項
            btnRunSequence.Enabled = false;
            btnStandby.Enabled = false;
            btnSingleJob.Enabled = false;
            btnFlipOnly.Enabled = false;
            btnContinuousJob.Enabled = false;
            btnStartFlow.Enabled = false;
            txtSequence.Enabled = false;
            AppendLog("INFO", "=== 開始執行待機復歸流程 ===");
            CancellationTokenSource y16BlinkCts = null;

            try
            {
                // 啟動 Y16 (YL 黃燈) 背景閃爍 (每秒一次 on off)
                AppendLog("INFO", "啟動 Y16 運轉黃燈閃爍服務...");
                y16BlinkCts = new CancellationTokenSource();
                CancellationToken token = y16BlinkCts.Token;
                _ = Task.Run(async () => {
                    try
                    {
                        bool y16State = true;
                        while (!token.IsCancellationRequested)
                        {
                            _comm.ForceCoil(14, y16State); // Y16
                            y16State = !y16State;
                            await Task.Delay(500, token);
                        }
                    }
                    catch (TaskCanceledException) { }
                    catch (Exception ex)
                    {
                        AppendLog("ERROR", $"Y16 復歸黃燈閃爍服務出錯: {ex.Message}");
                    }
                }, token);

                // 1. 所有 Y 接點先 off 0.5 秒
                AppendLog("INFO", "復歸步驟 [1/9]: 所有 Y 接點先 off 0.5 秒...");
                await Task.Run(() => {
                    for (int i = 0; i < 16; i++)
                    {
                        _comm.ForceCoil(i, false);
                    }
                });
                await Task.Delay(500);

                // 2. Y2 off 到 X3 on
                AppendLog("INFO", "復歸步驟 [2/9]: Y2 off...");
                await Task.Run(() => _comm.ForceCoil(2, false));
                AppendLog("INFO", "等待極限開關 X3(b0) 為 true...");
                if (!await WaitForInputStateAsync(3, true, 15))
                {
                    throw new Exception("等待 X3(b0) 逾時！");
                }

                // 3. T1 (暫停 1 秒)
                AppendLog("INFO", "復歸步驟 [3/9]: 暫停 1 秒...");
                await Task.Delay(1000);

                // 4. Y0 on 直到 X0 on
                AppendLog("INFO", "復歸步驟 [4/9]: Y0 on (A+)...");
                await Task.Run(() => {
                    _comm.ForceCoil(0, true);
                    _comm.ForceCoil(1, false);
                });
                AppendLog("INFO", "等待極限開關 X0(a1) 為 true...");
                if (!await WaitForInputStateAsync(0, true, 15))
                {
                    throw new Exception("等待 X0(a1) 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(0, false));

                // 5. Y6 on 直到 X6 on
                AppendLog("INFO", "復歸步驟 [5/9]: Y6 on...");
                await Task.Run(() => {
                    _comm.ForceCoil(6, true);
                    _comm.ForceCoil(5, false); // 常規防雙控衝突
                });
                AppendLog("INFO", "等待極限開關 X6 為 true...");
                if (!await WaitForInputStateAsync(6, true, 15))
                {
                    throw new Exception("等待 X6 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(6, false));

                // 6. T1 (暫停 1 秒)
                AppendLog("INFO", "復歸步驟 [6/9]: 暫停 1 秒...");
                await Task.Delay(1000);

                // 7. Y1 on 直到 X1 on
                AppendLog("INFO", "復歸步驟 [7/9]: Y1 on (A-)...");
                await Task.Run(() => {
                    _comm.ForceCoil(1, true);
                    _comm.ForceCoil(0, false);
                });
                AppendLog("INFO", "等待極限開關 X1(a0) 為 true...");
                if (!await WaitForInputStateAsync(1, true, 15))
                {
                    throw new Exception("等待 X1(a0) 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(1, false));

                // 8. Y4 on 直到 X4 on
                AppendLog("INFO", "復歸步驟 [8/9]: Y4 on (C-)...");
                await Task.Run(() => {
                    _comm.ForceCoil(4, true);
                    _comm.ForceCoil(3, false);
                });
                AppendLog("INFO", "等待極限開關 X4(c0) 為 true...");
                if (!await WaitForInputStateAsync(4, true, 15))
                {
                    throw new Exception("等待 X4(c0) 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(4, false));

                // 9. Y10 on 直到 ps1 off
                AppendLog("INFO", "復歸步驟 [9/9]: Y10 on (D-)...");
                await Task.Run(() => {
                    _comm.ForceCoil(8, true); // Y10
                    _comm.ForceCoil(5, false); // Y5
                });
                AppendLog("INFO", "等待極限開關 X5(ps1) 為 false...");
                if (!await WaitForInputStateAsync(5, false, 15))
                {
                    throw new Exception("等待 X5(ps1) 復歸逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(8, false));

                // 順利完成，停止 Y16 閃爍並確保熄滅
                if (y16BlinkCts != null)
                {
                    y16BlinkCts.Cancel();
                    y16BlinkCts.Dispose();
                    y16BlinkCts = null;
                }
                await Task.Run(() => _comm.ForceCoil(14, false)); // Y16 off

                // 待機綠燈 (Y17) 由定時輪詢 pollTimer_Tick 動態運算前提條件並點亮。
                AppendLog("INFO", "=== 待機復歸流程成功完成，已進入待機監控狀態 ===");
                MessageBox.Show("機構已成功回到機械原點 (已進入待機狀態)！", "復歸成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                // 安全機制：發生錯誤時，強制將所有相關的輸出關閉
                try
                {
                    await Task.Run(() =>
                    {
                        _comm.ForceCoil(0, false);
                        _comm.ForceCoil(1, false);
                        _comm.ForceCoil(2, false);
                        _comm.ForceCoil(3, false);
                        _comm.ForceCoil(4, false);
                        _comm.ForceCoil(5, false);
                        _comm.ForceCoil(6, false);
                        _comm.ForceCoil(8, false);
                        _comm.ForceCoil(14, false); // Y16 off
                    });
                }
                catch { }
            }
            finally
            {
                if (y16BlinkCts != null)
                {
                    y16BlinkCts.Cancel();
                    y16BlinkCts.Dispose();
                }

                // 恢復 UI 按鈕狀態
                if (_comm != null && _comm.IsOpen)
                {
                    btnRunSequence.Enabled = true;
                    btnStartFlow.Enabled = true;
                    txtSequence.Enabled = true;

                    bool isQ2 = (cmbQuestions.SelectedIndex == 2);
                    btnStandby.Enabled = isQ2;
                    btnSingleJob.Enabled = isQ2;
                    btnFlipOnly.Enabled = isQ2;
                    btnContinuousJob.Enabled = isQ2;
                }
            }
        }

        private async void btnSingleJob_Click(object sender, EventArgs e)
        {
            await RunSingleJobAsync();
        }

        private async Task RunSingleJobAsync()
        {
            // 前提檢查：必須在綠燈待機 (Y17 on) 狀態下才能執行
            bool isY17On = pnlGL.Tag != null && (bool)pnlGL.Tag;
            if (!isY17On)
            {
                MessageBox.Show("無法執行單一作業！\n前提條件：必須在綠燈待機 (Y17 為 ON) 狀態下才能執行！\n請先按下「復歸」按鈕使機構復歸。", "前提條件不符", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行單一作業！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 停用 UI 控制項
            btnRunSequence.Enabled = false;
            btnStandby.Enabled = false;
            btnSingleJob.Enabled = false;
            btnFlipOnly.Enabled = false;
            btnContinuousJob.Enabled = false;
            btnStartFlow.Enabled = false;
            txtSequence.Enabled = false;

            AppendLog("INFO", "=== 開始執行單一作業流程 ===");
            CancellationTokenSource blinkCts = null;

            try
            {
                // 點亮運轉紅燈 (Y15)，熄滅待機綠燈 (Y17)
                await Task.Run(() => {
                    _comm.ForceCoil(15, false); // Y17 GL off
                    _comm.ForceCoil(13, true);  // Y15 RL on
                });

                // 1. 判斷 X10 (s0進料，十進位 index 8)
                bool isX10 = _xLeds[8].Tag != null && (bool)_xLeds[8].Tag;
                if (!isX10)
                {
                    AppendLog("INFO", "偵測到無進料 (X10 off)，開始 10 秒進料等待...");
                    bool waitX10Success = false;
                    for (int i = 0; i < 100; i++) // 100 * 100ms = 10 秒
                    {
                        await Task.Delay(100);
                        if (_xLeds[8].Tag != null && (bool)_xLeds[8].Tag)
                        {
                            waitX10Success = true;
                            break;
                        }
                    }

                    if (!waitX10Success)
                    {
                        AppendLog("INFO", "進料等待超時 (10 秒)，且 X10 仍為 OFF。直接結束單一作業。");
                        await Task.Run(() => {
                            _comm.ForceCoil(13, false); // Y15 off
                            _comm.ForceCoil(15, true);  // Y17 GL on
                        });
                        MessageBox.Show("無進料 (10秒等待超時)，單一作業已自動結束並復歸待機狀態。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return; // 結束單一作業
                    }
                    else
                    {
                        AppendLog("INFO", "在 10 秒內偵測到進料 (X10 變為 ON)，繼續執行單一作業。");
                    }
                }

                // 2. 當 X10 為 ON
                // 判斷 X12 (s2重量下/顏色紅黑，十進位 index 10)
                bool isX12 = _xLeds[10].Tag != null && (bool)_xLeds[10].Tag;
                if (!isX12)
                {
                    AppendLog("INFO", "偵測到 X12 (顏色紅黑) 為 OFF (黑色料)，啟用 Y15 閃爍 (每秒一次 on/off)。");
                    blinkCts = new CancellationTokenSource();
                    CancellationToken token = blinkCts.Token;
                    
                    // 背景閃爍 Task
                    _ = Task.Run(async () => {
                        try
                        {
                            bool y15State = true;
                            while (!token.IsCancellationRequested)
                            {
                                _comm.ForceCoil(13, y15State);
                                y15State = !y15State;
                                await Task.Delay(500, token); // 每0.5秒反轉一次狀態，即1秒內完成一次完整on-off
                            }
                        }
                        catch (TaskCanceledException) { }
                        catch (Exception ex)
                        {
                            AppendLog("ERROR", $"Y15 閃爍背景服務出錯: {ex.Message}");
                        }
                    }, token);
                }
                else
                {
                    AppendLog("INFO", "偵測到 X12 (顏色紅黑) 為 ON (紅色料)，Y15 保持恆亮。");
                }

                // 3. 判斷 X11 (重量上，十進位 index 9)
                bool isX11 = _xLeds[9].Tag != null && (bool)_xLeds[9].Tag;
                if (isX11)
                {
                    AppendLog("INFO", "條件分支：X11 (重量上) 為 ON，直接進行移料步驟。");
                }
                else
                {
                    AppendLog("INFO", "條件分支：X11 (重量上) 為 OFF，開始執行翻轉步驟。");
                    
                    // 執行翻轉步驟
                    // 2-1. Y2 off (B-)
                    AppendLog("INFO", "翻轉 [1/5]: Y2 off...");
                    await Task.Run(() => _comm.ForceCoil(2, false));
                    AppendLog("INFO", "等待 B缸退回到 X3(b0) 為 true...");
                    if (!await WaitForInputStateAsync(3, true, 15))
                    {
                        throw new Exception("翻轉步驟中，等待退回 X3 逾時！");
                    }

                    // 2-2. Y3 on (C+) 直到 X4 off
                    AppendLog("INFO", "翻轉 [2/5]: Y3 on (C+)...");
                    await Task.Run(() => {
                        _comm.ForceCoil(3, true);
                        _comm.ForceCoil(4, false);
                    });
                    AppendLog("INFO", "等待 X4 off (c0離去)...");
                    if (!await WaitForInputStateAsync(4, false, 15))
                    {
                        throw new Exception("翻轉步驟中，等待 X4 off 逾時！");
                    }
                    await Task.Run(() => _comm.ForceCoil(3, false));

                    // 2-3. T1 (暫停 1 秒)
                    AppendLog("INFO", "翻轉 [3/5]: 暫停 1 秒...");
                    await Task.Delay(1000);

                    // 2-4. Y0 on (A+) 直到 X0 on
                    AppendLog("INFO", "翻轉 [4/5]: Y0 on (A+)...");
                    await Task.Run(() => {
                        _comm.ForceCoil(0, true);
                        _comm.ForceCoil(1, false);
                    });
                    AppendLog("INFO", "等待 X0(a1) 為 true...");
                    if (!await WaitForInputStateAsync(0, true, 15))
                    {
                        throw new Exception("翻轉步驟中，等待 X0(a1) 逾時！");
                    }
                    await Task.Run(() => _comm.ForceCoil(0, false));

                    // 2-5. 判斷 X6/X7 進行旋轉
                    bool isX6 = _xLeds[6].Tag != null && (bool)_xLeds[6].Tag;
                    bool isX7 = _xLeds[7].Tag != null && (bool)_xLeds[7].Tag;

                    if (isX6)
                    {
                        AppendLog("INFO", "翻轉分支 [X6 on]: 執行 Y7 on 直到 X7 on...");
                        await Task.Run(() => {
                            _comm.ForceCoil(7, true);
                            _comm.ForceCoil(6, false);
                        });
                        if (!await WaitForInputStateAsync(7, true, 15))
                        {
                            throw new Exception("翻轉分支 [X6] 等待 X7 逾時！");
                        }
                        await Task.Run(() => _comm.ForceCoil(7, false));
                    }
                    else if (isX7)
                    {
                        AppendLog("INFO", "翻轉分支 [X7 on]: 執行 Y6 on 直到 X6 on...");
                        await Task.Run(() => {
                            _comm.ForceCoil(6, true);
                            _comm.ForceCoil(7, false);
                        });
                        if (!await WaitForInputStateAsync(6, true, 15))
                        {
                            throw new Exception("翻轉分支 [X7] 等待 X6 逾時！");
                        }
                        await Task.Run(() => _comm.ForceCoil(6, false));
                    }
                    else
                    {
                        AppendLog("WARNING", "翻轉分支：未偵測到 X6 或 X7 為 ON，略過方向旋轉。");
                    }

                    // 2-6. 暫停 1 秒 (T1) 
                    AppendLog("INFO", "翻轉後等待: 暫停 1 秒...");
                    await Task.Delay(1000);

                    // 2-7. Y1 on (A-) 直到 X1 on
                    AppendLog("INFO", "翻轉 [5/5]: Y1 on (A-)...");
                    await Task.Run(() => {
                        _comm.ForceCoil(1, true);
                        _comm.ForceCoil(0, false);
                    });
                    AppendLog("INFO", "等待 X1(a0) 為 true...");
                    if (!await WaitForInputStateAsync(1, true, 15))
                    {
                        throw new Exception("翻轉步驟中，等待 X1(a0) 逾時！");
                    }
                    await Task.Run(() => _comm.ForceCoil(1, false));

                    AppendLog("INFO", "翻轉步驟順利完成，準備進行移料。");
                }

                // 4. 移料步驟
                // 3-1. Y3 on (C+) 直到 X4 off
                AppendLog("INFO", "移料步驟 [1/6]: Y3 on (C+)...");
                await Task.Run(() => {
                    _comm.ForceCoil(3, true);
                    _comm.ForceCoil(4, false);
                });
                AppendLog("INFO", "等待 X4 off (c0離去)...");
                if (!await WaitForInputStateAsync(4, false, 15))
                {
                    throw new Exception("移料步驟中，等待 X4 off 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(3, false));

                // 3-2. Y2 持續 ON 
                AppendLog("INFO", "移料步驟 [2/6]: Y2 (B+) 持續 ON...");
                await Task.Run(() => _comm.ForceCoil(2, true));

                // 3-3. 等待 X2 on
                AppendLog("INFO", "移料步驟 [3/6]: 等待 X2 為 true...");
                if (!await WaitForInputStateAsync(2, true, 15))
                {
                    throw new Exception("移料步驟中，等待 X2 逾時！");
                }

                // 3-4. 當 X2 on，Y5 on (D+ / M+) 直到 X5 on (ps1)
                AppendLog("INFO", "移料步驟 [4/6]: Y5 on (D+)...");
                await Task.Run(() => {
                    _comm.ForceCoil(5, true);
                    _comm.ForceCoil(8, false); // Y10 off
                });
                AppendLog("INFO", "等待極限開關 X5(ps1) 為 true...");
                if (!await WaitForInputStateAsync(5, true, 15))
                {
                    throw new Exception("移料步驟中，等待 X5(ps1) 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(5, false));

                // 3-5. 當 X5 on，Y4 on (C-) 直到 X4 off
                AppendLog("INFO", "移料步驟 [5/6]: Y4 on (C-)...");
                await Task.Run(() => {
                    _comm.ForceCoil(4, true);
                    _comm.ForceCoil(3, false);
                });
                AppendLog("INFO", "等待 X4 off (c0離去)...");
                if (!await WaitForInputStateAsync(4, false, 15))
                {
                    throw new Exception("移料步驟中，等待 X4 off 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(4, false));

                // 3-6. 當 X4 off 時，Y2 off (B-)
                AppendLog("INFO", "移料步驟 [6/6]: Y2 (B+) off...");
                await Task.Run(() => _comm.ForceCoil(2, false));

                // 3-7. 等待 X3 on，Y10 on (D-) 直到 X5 off
                AppendLog("INFO", "等待 X3(b0) 為 true...");
                if (!await WaitForInputStateAsync(3, true, 15))
                {
                    throw new Exception("移料步驟中，等待 X3(b0) 逾時！");
                }
                AppendLog("INFO", "移料完成釋放：Y10 on (D-)...");
                await Task.Run(() => {
                    _comm.ForceCoil(8, true); // Y10 (放)
                    _comm.ForceCoil(5, false); // Y5 off
                });
                AppendLog("INFO", "等待 X5(ps1) 復歸變為 false...");
                if (!await WaitForInputStateAsync(5, false, 15))
                {
                    throw new Exception("移料步驟中，等待 X5(ps1) 復歸逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(8, false));

                // 5. 結束 Y15 背景閃爍
                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                    blinkCts = null;
                }

                // 正常完成，點亮綠燈 GL (Y17)，熄滅紅燈 (Y15)
                AppendLog("INFO", "單一作業結束: 點亮待機綠燈 (Y17)，熄滅運轉紅燈 (Y15)...");
                await Task.Run(() => {
                    _comm.ForceCoil(13, false); // Y15 RL off
                    _comm.ForceCoil(15, true);  // Y17 GL on
                });

                AppendLog("INFO", "=== 單一作業流程成功執行完畢 ===");
                MessageBox.Show("單一作業已成功執行完畢！機構已回到待機狀態。", "單一作業成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"單一作業中斷: {ex.Message}");
                MessageBox.Show($"單一作業執行失敗：\n{ex.Message}", "單一作業中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                    blinkCts = null;
                }

                // 安全機制：發生錯誤時，強制將所有相關的輸出關閉
                try
                {
                    await Task.Run(() =>
                    {
                        _comm.ForceCoil(0, false);
                        _comm.ForceCoil(1, false);
                        _comm.ForceCoil(2, false);
                        _comm.ForceCoil(3, false);
                        _comm.ForceCoil(4, false);
                        _comm.ForceCoil(5, false);
                        _comm.ForceCoil(6, false);
                        _comm.ForceCoil(7, false); // Y7
                        _comm.ForceCoil(8, false); // Y10
                        
                        _comm.ForceCoil(13, false); // RL off
                        _comm.ForceCoil(15, true);  // GL on (待機)
                    });
                }
                catch { }
            }
            finally
            {
                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                }

                // 恢復 UI 按鈕狀態
                if (_comm != null && _comm.IsOpen)
                {
                    btnRunSequence.Enabled = true;
                    btnStartFlow.Enabled = true;
                    txtSequence.Enabled = true;

                    bool isQ2 = (cmbQuestions.SelectedIndex == 2);
                    btnStandby.Enabled = isQ2;
                    btnSingleJob.Enabled = isQ2;
                    btnFlipOnly.Enabled = isQ2;
                    btnContinuousJob.Enabled = isQ2;
                }
            }
        }

        private async void btnFlipOnly_Click(object sender, EventArgs e)
        {
            await RunFlipOnlyAsync();
        }

        private async Task RunFlipOnlyAsync()
        {
            // 前提檢查：必須在綠燈待機 (Y17 on) 且有進料 (X10 on) 狀態下才能執行
            bool isY17On = pnlGL.Tag != null && (bool)pnlGL.Tag;
            bool isX10On = _xLeds[8].Tag != null && (bool)_xLeds[8].Tag;

            if (!isY17On || !isX10On)
            {
                MessageBox.Show("無法執行指定作業！\n前提條件：必須在綠燈待機 (Y17 為 ON) 且有進料 (X10 為 ON) 狀態下才能執行！", "前提條件不符", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行指定作業！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 停用 UI 控制項
            btnRunSequence.Enabled = false;
            btnStandby.Enabled = false;
            btnSingleJob.Enabled = false;
            btnFlipOnly.Enabled = false;
            btnContinuousJob.Enabled = false;
            btnStartFlow.Enabled = false;
            txtSequence.Enabled = false;

            AppendLog("INFO", "=== 開始執行指定作業流程 ===");
            CancellationTokenSource blinkCts = null;

            try
            {
                // Y17 off, Y15 開始閃爍 (每秒一次 on/off)
                AppendLog("INFO", "指定作業 [1/9]: 熄滅待機綠燈 (Y17)，啟動運轉紅燈 (Y15) 閃爍服務...");
                await Task.Run(() => _comm.ForceCoil(15, false));

                blinkCts = new CancellationTokenSource();
                CancellationToken token = blinkCts.Token;
                _ = Task.Run(async () => {
                    try
                    {
                        bool y15State = true;
                        while (!token.IsCancellationRequested)
                        {
                            _comm.ForceCoil(13, y15State);
                            y15State = !y15State;
                            await Task.Delay(500, token);
                        }
                    }
                    catch (TaskCanceledException) { }
                    catch (Exception ex)
                    {
                        AppendLog("ERROR", $"指定作業紅燈閃爍出錯: {ex.Message}");
                    }
                }, token);

                // Y2 off (B-), 等待退回 X3
                AppendLog("INFO", "指定作業 [2/9]: Y2 off...");
                await Task.Run(() => _comm.ForceCoil(2, false));
                AppendLog("INFO", "等待 B缸退回到 X3(b0) 為 true...");
                if (!await WaitForInputStateAsync(3, true, 15))
                {
                    throw new Exception("等待 X3(b0) 逾時！");
                }

                // Y3 on (C+) 直到 X4 off
                AppendLog("INFO", "指定作業 [3/9]: Y3 on (C+)...");
                await Task.Run(() => {
                    _comm.ForceCoil(3, true);
                    _comm.ForceCoil(4, false);
                });
                AppendLog("INFO", "等待 X4 off (c0離去)...");
                if (!await WaitForInputStateAsync(4, false, 15))
                {
                    throw new Exception("等待 X4 off 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(3, false));

                // T1 (暫停 1 秒)
                AppendLog("INFO", "指定作業 [4/9]: 暫停 1 秒...");
                await Task.Delay(1000);

                // Y0 on (A+) 直到 X0 on
                AppendLog("INFO", "指定作業 [5/9]: Y0 on (A+)...");
                await Task.Run(() => {
                    _comm.ForceCoil(0, true);
                    _comm.ForceCoil(1, false);
                });
                AppendLog("INFO", "等待 X0(a1) 為 true...");
                if (!await WaitForInputStateAsync(0, true, 15))
                {
                    throw new Exception("等待 X0(a1) 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(0, false));

                // 旋轉判斷 X6 / X7
                bool isX6 = _xLeds[6].Tag != null && (bool)_xLeds[6].Tag;
                bool isX7 = _xLeds[7].Tag != null && (bool)_xLeds[7].Tag;

                if (isX6)
                {
                    AppendLog("INFO", "指定翻轉 [6/9] [X6 on]: 執行 Y7 on 直到 X7 on...");
                    await Task.Run(() => {
                        _comm.ForceCoil(7, true);
                        _comm.ForceCoil(6, false);
                    });
                    if (!await WaitForInputStateAsync(7, true, 15))
                    {
                        throw new Exception("等待 X7 逾時！");
                    }
                    await Task.Run(() => _comm.ForceCoil(7, false));
                }
                else if (isX7)
                {
                    AppendLog("INFO", "指定翻轉 [6/9] [X7 on]: 執行 Y6 on 直到 X6 on...");
                    await Task.Run(() => {
                        _comm.ForceCoil(6, true);
                        _comm.ForceCoil(7, false);
                    });
                    if (!await WaitForInputStateAsync(6, true, 15))
                    {
                        throw new Exception("等待 X6 逾時！");
                    }
                    await Task.Run(() => _comm.ForceCoil(6, false));
                }
                else
                {
                    AppendLog("WARNING", "指定翻轉 [6/9]: 未偵測到 X6 或 X7，略過方向旋轉。");
                }

                // T1 (暫停 1 秒)
                AppendLog("INFO", "指定翻轉 [7/9]: 暫停 1 秒...");
                await Task.Delay(1000);

                // Y1 on (A-) 直到 X1 on
                AppendLog("INFO", "指定翻轉 [8/9]: Y1 on (A-)...");
                await Task.Run(() => {
                    _comm.ForceCoil(1, true);
                    _comm.ForceCoil(0, false);
                });
                AppendLog("INFO", "等待 X1(a0) 為 true...");
                if (!await WaitForInputStateAsync(1, true, 15))
                {
                    throw new Exception("等待 X1(a0) 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(1, false));

                // Y4 on (C-), 復歸到 c0 (X4 on)
                AppendLog("INFO", "指定翻轉 [9/9]: Y4 on (C-)...");
                await Task.Run(() => {
                    _comm.ForceCoil(4, true);
                    _comm.ForceCoil(3, false);
                });
                AppendLog("INFO", "等待 X4(c0) 為 true...");
                if (!await WaitForInputStateAsync(4, true, 15))
                {
                    throw new Exception("等待 X4(c0) 逾時！");
                }
                await Task.Run(() => _comm.ForceCoil(4, false));

                // 正常結束，停止閃爍並還原燈號
                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                    blinkCts = null;
                }

                AppendLog("INFO", "翻轉結束: 點亮待機綠燈 (Y17)，關閉紅燈 (Y15)...");
                await Task.Run(() => {
                    _comm.ForceCoil(13, false); // Y15 off
                    _comm.ForceCoil(15, true);  // Y17 on
                });

                AppendLog("INFO", "=== 指定翻轉流程成功執行完畢 ===");
                MessageBox.Show("指定翻轉流程已成功執行完畢！", "指定翻轉成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"指定翻轉中斷: {ex.Message}");
                MessageBox.Show($"指定翻轉執行失敗：\n{ex.Message}", "指定翻轉中斷", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                    blinkCts = null;
                }

                // 安全機制：發生錯誤時，強制將所有相關的輸出關閉
                try
                {
                    await Task.Run(() =>
                    {
                        _comm.ForceCoil(0, false);
                        _comm.ForceCoil(1, false);
                        _comm.ForceCoil(2, false);
                        _comm.ForceCoil(3, false);
                        _comm.ForceCoil(4, false);
                        _comm.ForceCoil(5, false);
                        _comm.ForceCoil(6, false);
                        _comm.ForceCoil(7, false);
                        _comm.ForceCoil(8, false);
                        
                        _comm.ForceCoil(13, false); // RL off
                        _comm.ForceCoil(15, true);  // GL on
                    });
                }
                catch { }
            }
            finally
            {
                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                }

                // 恢復 UI 按鈕狀態
                if (_comm != null && _comm.IsOpen)
                {
                    btnRunSequence.Enabled = true;
                    btnStandby.Enabled = true;
                    btnSingleJob.Enabled = true;
                    btnFlipOnly.Enabled = true;
                    btnContinuousJob.Enabled = true;
                    btnStartFlow.Enabled = true;
                    txtSequence.Enabled = true;
                }
            }
        }

        private async void btnContinuousJob_Click(object sender, EventArgs e)
        {
            await RunContinuousJobAsync();
        }

        private async Task RunContinuousJobAsync()
        {
            // 前提檢查：必須在綠燈待機 (Y17 on) 且有進料 (X10 on) 狀態下才能執行
            bool isY17On = pnlGL.Tag != null && (bool)pnlGL.Tag;
            bool isX10On = _xLeds[8].Tag != null && (bool)_xLeds[8].Tag;

            if (!isY17On || !isX10On)
            {
                MessageBox.Show("無法執行連續作業！\n前提條件：必須在綠燈待機 (Y17 為 ON) 且有進料 (X10 為 ON) 狀態下才能執行！", "前提條件不符", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_comm == null || !_comm.IsOpen)
            {
                MessageBox.Show("請先連線 PLC 才能執行連續作業！", "未連線", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 停用 UI 控制項
            btnRunSequence.Enabled = false;
            btnStandby.Enabled = false;
            btnSingleJob.Enabled = false;
            btnFlipOnly.Enabled = false;
            btnContinuousJob.Enabled = false;
            btnStartFlow.Enabled = false;
            txtSequence.Enabled = false;

            AppendLog("INFO", "=== 開始執行連續作業流程 ===");
            CancellationTokenSource blinkCts = null;

            try
            {
                while (true)
                {
                    // 1. Y17 off (綠燈熄), Y15 on (紅燈亮)
                    AppendLog("INFO", "連續作業迴圈啟動: 點亮運轉紅燈 (Y15)，熄滅待機綠燈 (Y17)...");
                    await Task.Run(() => {
                        _comm.ForceCoil(15, false); // Y17 GL off
                        _comm.ForceCoil(13, true);  // Y15 RL on
                    });

                    // 2. 判斷 X10 (s0進料，十進位 index 8)
                    bool isX10 = _xLeds[8].Tag != null && (bool)_xLeds[8].Tag;
                    if (isX10)
                    {
                        // 判斷 X12 (s2重量下/顏色紅黑，十進位 index 10)
                        bool isX12 = _xLeds[10].Tag != null && (bool)_xLeds[10].Tag;
                        if (!isX12)
                        {
                            AppendLog("INFO", "偵測到 X12 (顏色紅黑) 為 OFF (黑色料)，啟用 Y15 閃爍 (每秒一次 on/off)。");
                            blinkCts = new CancellationTokenSource();
                            CancellationToken token = blinkCts.Token;
                            _ = Task.Run(async () => {
                                try
                                {
                                    bool y15State = true;
                                    while (!token.IsCancellationRequested)
                                    {
                                        _comm.ForceCoil(13, y15State);
                                        y15State = !y15State;
                                        await Task.Delay(500, token);
                                    }
                                }
                                catch (TaskCanceledException) { }
                                catch (Exception ex)
                                {
                                    AppendLog("ERROR", $"Y15 閃爍背景服務出錯: {ex.Message}");
                                }
                            }, token);
                        }
                        else
                        {
                            AppendLog("INFO", "偵測到 X12 (顏色紅黑) 為 ON (紅色料)，Y15 保持恆亮。");
                        }
                    }

                    // 3. 判斷 X11 (重量上，十進位 index 9)
                    bool isX11 = _xLeds[9].Tag != null && (bool)_xLeds[9].Tag;
                    if (isX11)
                    {
                        AppendLog("INFO", "條件分支：X11 (重量上) 為 ON，直接進行移料步驟。");
                    }
                    else
                    {
                        AppendLog("INFO", "條件分支：X11 (重量上) 為 OFF，開始執行翻轉步驟。");
                        
                        // 執行翻轉步驟
                        // 3-1. Y2 off (B-)
                        AppendLog("INFO", "翻轉 [1/5]: Y2 off...");
                        await Task.Run(() => _comm.ForceCoil(2, false));
                        AppendLog("INFO", "等待 B缸退回到 X3(b0) 為 true...");
                        if (!await WaitForInputStateAsync(3, true, 15))
                        {
                            throw new Exception("翻轉步驟中，等待退回 X3 逾時！");
                        }

                        // 3-2. Y3 on (C+) 直到 X4 off
                        AppendLog("INFO", "翻轉 [2/5]: Y3 on (C+)...");
                        await Task.Run(() => {
                            _comm.ForceCoil(3, true);
                            _comm.ForceCoil(4, false);
                        });
                        AppendLog("INFO", "等待 X4 off (c0離去)...");
                        if (!await WaitForInputStateAsync(4, false, 15))
                        {
                            throw new Exception("翻轉步驟中，等待 X4 off 逾時！");
                        }
                        await Task.Run(() => _comm.ForceCoil(3, false));

                        // 3-3. T1 (暫停 1 秒)
                        AppendLog("INFO", "翻轉 [3/5]: 暫停 1 秒...");
                        await Task.Delay(1000);

                        // 3-4. Y0 on (A+) 直到 X0 on
                        AppendLog("INFO", "翻轉 [4/5]: Y0 on (A+)...");
                        await Task.Run(() => {
                            _comm.ForceCoil(0, true);
                            _comm.ForceCoil(1, false);
                        });
                        AppendLog("INFO", "等待 X0(a1) 為 true...");
                        if (!await WaitForInputStateAsync(0, true, 15))
                        {
                            throw new Exception("翻轉步驟中，等待 X0(a1) 逾時！");
                        }
                        await Task.Run(() => _comm.ForceCoil(0, false));

                        // 3-5. 判斷 X6/X7 進行旋轉
                        bool isX6 = _xLeds[6].Tag != null && (bool)_xLeds[6].Tag;
                        bool isX7 = _xLeds[7].Tag != null && (bool)_xLeds[7].Tag;

                        if (isX6)
                        {
                            AppendLog("INFO", "翻轉分支 [X6 on]: 執行 Y7 on 直到 X7 on...");
                            await Task.Run(() => {
                                _comm.ForceCoil(7, true);
                                _comm.ForceCoil(6, false);
                            });
                            if (!await WaitForInputStateAsync(7, true, 15))
                            {
                                throw new Exception("翻轉分支 [X6] 等待 X7 逾時！");
                            }
                            await Task.Run(() => _comm.ForceCoil(7, false));
                        }
                        else if (isX7)
                        {
                            AppendLog("INFO", "翻轉分支 [X7 on]: 執行 Y6 on 直到 X6 on...");
                            await Task.Run(() => {
                                _comm.ForceCoil(6, true);
                                _comm.ForceCoil(7, false);
                            });
                            if (!await WaitForInputStateAsync(6, true, 15))
                            {
                                throw new Exception("翻轉分支 [X7] 等待 X6 逾時！");
                            }
                            await Task.Run(() => _comm.ForceCoil(6, false));
                        }

                        // 3-6. 暫停 1 秒 (T1) 
                        AppendLog("INFO", "翻轉後等待: 暫停 1 秒...");
                        await Task.Delay(1000);

                        // 3-7. Y1 on (A-) 直到 X1 on
                        AppendLog("INFO", "翻轉 [5/5]: Y1 on (A-)...");
                        await Task.Run(() => {
                            _comm.ForceCoil(1, true);
                            _comm.ForceCoil(0, false);
                        });
                        AppendLog("INFO", "等待 X1(a0) 為 true...");
                        if (!await WaitForInputStateAsync(1, true, 15))
                        {
                            throw new Exception("翻轉步驟中，等待 X1(a0) 逾時！");
                        }
                        await Task.Run(() => _comm.ForceCoil(1, false));
                    }

                    // 4. 移料步驟
                    // 4-1. Y3 on (C+) 直到 X4 off
                    AppendLog("INFO", "移料步驟 [1/6]: Y3 on (C+)...");
                    await Task.Run(() => {
                        _comm.ForceCoil(3, true);
                        _comm.ForceCoil(4, false);
                    });
                    AppendLog("INFO", "等待 X4 off (c0離去)...");
                    if (!await WaitForInputStateAsync(4, false, 15))
                    {
                        throw new Exception("移料步驟中，等待 X4 off 逾時！");
                    }
                    await Task.Run(() => _comm.ForceCoil(3, false));

                    // 4-2. Y2 持續 ON 
                    AppendLog("INFO", "移料步驟 [2/6]: Y2 (B+) 持續 ON...");
                    await Task.Run(() => _comm.ForceCoil(2, true));

                    // 4-3. 等待 X2 on
                    AppendLog("INFO", "移料步驟 [3/6]: 等待 X2 為 true...");
                    if (!await WaitForInputStateAsync(2, true, 15))
                    {
                        throw new Exception("移料步驟中，等待 X2 逾時！");
                    }

                    // 4-4. 當 X2 on，Y5 on (D+) 直到 X5 on (ps1)
                    AppendLog("INFO", "移料步驟 [4/6]: Y5 on (D+)...");
                    await Task.Run(() => {
                        _comm.ForceCoil(5, true);
                        _comm.ForceCoil(8, false); // Y10 off
                    });
                    AppendLog("INFO", "等待極限開關 X5(ps1) 為 true...");
                    if (!await WaitForInputStateAsync(5, true, 15))
                    {
                        throw new Exception("移料步驟中，等待 X5(ps1) 逾時！");
                    }
                    await Task.Run(() => _comm.ForceCoil(5, false));

                    // 4-5. 當 X5 on，Y4 on (C-) 直到 X4 off
                    AppendLog("INFO", "移料步驟 [5/6]: Y4 on (C-)...");
                    await Task.Run(() => {
                        _comm.ForceCoil(4, true);
                        _comm.ForceCoil(3, false);
                    });
                    AppendLog("INFO", "等待 X4 off (c0離去)...");
                    if (!await WaitForInputStateAsync(4, false, 15))
                    {
                        throw new Exception("移料步驟中，等待 X4 off 逾時！");
                    }
                    await Task.Run(() => _comm.ForceCoil(4, false));

                    // 4-6. 當 X4 off 時，Y2 off (B-)
                    AppendLog("INFO", "移料步驟 [6/6]: Y2 (B+) off...");
                    await Task.Run(() => _comm.ForceCoil(2, false));

                    // 4-7. 等待 X3 on，Y10 on (D-) 直到 X5 off
                    AppendLog("INFO", "等待 X3(b0) 為 true...");
                    if (!await WaitForInputStateAsync(3, true, 15))
                    {
                        throw new Exception("移料步驟中，等待 X3(b0) 逾時！");
                    }
                    AppendLog("INFO", "移料完成釋放：Y10 on (D-)...");
                    await Task.Run(() => {
                        _comm.ForceCoil(8, true); // Y10 (放)
                        _comm.ForceCoil(5, false); // Y5 off
                    });
                    AppendLog("INFO", "等待 X5(ps1) 復歸變為 false...");
                    if (!await WaitForInputStateAsync(5, false, 15))
                    {
                        throw new Exception("移料步驟中，等待 X5(ps1) 復歸逾時！");
                    }
                    await Task.Run(() => _comm.ForceCoil(8, false));

                    // 5. 移料完成後，先關閉閃爍，恢復 Y15 恆亮
                    if (blinkCts != null)
                    {
                        blinkCts.Cancel();
                        blinkCts.Dispose();
                        blinkCts = null;
                    }
                    await Task.Run(() => _comm.ForceCoil(13, true)); // Y15 on 恆亮

                    // 6. 監控 X10 狀態
                    AppendLog("INFO", "本輪連續作業移料完成。開始監控 X10 進料狀態...");
                    bool nextRound = false;
                    bool isX10Now = _xLeds[8].Tag != null && (bool)_xLeds[8].Tag;

                    if (isX10Now)
                    {
                        AppendLog("INFO", "X10 已為 ON，暫停 3 秒 (T3) 後重複連續作業動作。");
                        await Task.Delay(3000);
                        nextRound = true;
                    }
                    else
                    {
                        AppendLog("INFO", "X10 為 OFF，啟動 10 秒進料等待。");
                        for (int i = 0; i < 100; i++) // 100 * 100ms = 10秒
                        {
                            await Task.Delay(100);
                            if (_xLeds[8].Tag != null && (bool)_xLeds[8].Tag)
                            {
                                AppendLog("INFO", "在 10 秒內偵測到 X10 變為 ON！暫停 3 秒 (T3) 後重複連續作業動作。");
                                await Task.Delay(3000);
                                nextRound = true;
                                break;
                            }
                        }
                    }

                    if (!nextRound)
                    {
                        AppendLog("INFO", "連續作業結束：超過 10 秒 X10 仍為 OFF。熄滅運轉燈，點亮待機綠燈...");
                        await Task.Run(() => {
                            _comm.ForceCoil(13, false); // Y15 off
                            _comm.ForceCoil(15, true);  // Y17 on
                        });
                        MessageBox.Show("連續作業因超過 10 秒無進料 (X10 off) 而自動結束並復歸待機狀態。", "連續作業結束", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break; // 跳出 while (true) 迴圈，結束任務
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

                // 安全機制：發生錯誤時，強制將所有相關的輸出關閉
                try
                {
                    await Task.Run(() =>
                    {
                        _comm.ForceCoil(0, false);
                        _comm.ForceCoil(1, false);
                        _comm.ForceCoil(2, false);
                        _comm.ForceCoil(3, false);
                        _comm.ForceCoil(4, false);
                        _comm.ForceCoil(5, false);
                        _comm.ForceCoil(6, false);
                        _comm.ForceCoil(7, false); // Y7
                        _comm.ForceCoil(8, false); // Y10
                        
                        _comm.ForceCoil(13, false); // RL off
                        _comm.ForceCoil(15, true);  // GL on (待機)
                    });
                }
                catch { }
            }
            finally
            {
                if (blinkCts != null)
                {
                    blinkCts.Cancel();
                    blinkCts.Dispose();
                }

                // 恢復 UI 按鈕狀態
                if (_comm != null && _comm.IsOpen)
                {
                    btnRunSequence.Enabled = true;
                    btnStandby.Enabled = true;
                    btnSingleJob.Enabled = true;
                    btnFlipOnly.Enabled = true;
                    btnContinuousJob.Enabled = true;
                    btnStartFlow.Enabled = true;
                    txtSequence.Enabled = true;
                }
            }
        }
    }
}
