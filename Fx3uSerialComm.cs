using System;
using System.IO.Ports;
using System.Text;

namespace fx3u
{
    public class Fx3uSerialComm : IDisposable
    {
        private SerialPort _serialPort;
        private readonly object _lockObject = new object();

        // 供 UI 顯示 Log 使用。參數: (方向 "TX"/"RX"/"INFO"/"ERROR", 訊息內容)
        public event Action<string, string> LogMessage;

        public string PortName => _serialPort?.PortName;

        public bool IsOpen => _serialPort != null && _serialPort.IsOpen;

        public Fx3uSerialComm(string portName)
        {
            // 三菱 FX3U 編程埠預設通訊設定：9600 bps, 偶同位 Even, 7 Data Bits, 1 Stop Bit
            _serialPort = new SerialPort(portName, 9600, Parity.Even, 7, StopBits.One)
            {
                ReadTimeout = 500,
                WriteTimeout = 500
            };
        }

        public void Open()
        {
            lock (_lockObject)
            {
                if (_serialPort != null && !_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    // 丟棄快取資料
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    RaiseLog("INFO", $"開啟 Serial Port {PortName} 成功。設定: 9600, 7, E, 1");
                }
            }
        }

        public void Close()
        {
            lock (_lockObject)
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                    RaiseLog("INFO", $"關閉 Serial Port {PortName}");
                }
            }
        }

        /// <summary>
        /// 讀取 16 個接點的狀態 (X0~X17 或 Y0~Y17)
        /// X 接點首地址為 0x0080，Y 接點首地址為 0x00A0
        /// </summary>
        /// <param name="isX">true 代表讀取 X 接點，false 代表讀取 Y 接點</param>
        /// <returns>長度為 16 的布林陣列，索引 0~7 代表 X0~X7/Y0~Y7，索引 8~15 代表 X10~X17/Y10~Y17</returns>
        public bool[] ReadDeviceStates(bool isX)
        {
            string addr = isX ? "0080" : "00A0";
            string len = "02"; // 讀取 2 位元組 (16 bits)
            
            // 建立讀取命令: STX (0x02) + CMD '0' (0x30) + ADDR + LEN + ETX (0x03)
            string body = "0" + addr + len;
            string checksum = CalculateChecksum(body + (char)0x03);
            
            byte[] txBuffer = BuildPacket(body, checksum);

            lock (_lockObject)
            {
                if (!IsOpen)
                {
                    throw new InvalidOperationException("Serial Port 尚未連線。");
                }

                int retry = 3;
                while (retry > 0)
                {
                    try
                    {
                        // 丟棄快取
                        _serialPort.DiscardInBuffer();
                        
                        // 發送
                        RaiseLog("TX", FormatBytesToHex(txBuffer) + " (讀取 " + (isX ? "X0~X17" : "Y0~Y17") + ")");
                        _serialPort.Write(txBuffer, 0, txBuffer.Length);

                        // 回應格式為: STX (1 byte) + DATA (4 bytes ASCII Hex) + ETX (1 byte) + Checksum (2 bytes) = 共 8 bytes
                        byte[] rxBuffer = new byte[8];
                        int bytesRead = 0;
                        while (bytesRead < 8)
                        {
                            int read = _serialPort.Read(rxBuffer, bytesRead, 8 - bytesRead);
                            if (read == 0) break;
                            bytesRead += read;
                        }

                        if (bytesRead < 8)
                        {
                            throw new TimeoutException("接收回應超時，長度不足 8 位元組。");
                        }

                        RaiseLog("RX", FormatBytesToHex(rxBuffer));

                        // 驗證標頭與結尾
                        if (rxBuffer[0] != 0x02 || rxBuffer[5] != 0x03)
                        {
                            throw new FormatException("回傳封包格式錯誤：STX 或 ETX 標記不符。");
                        }

                        // 驗證 Checksum
                        string rxData = Encoding.ASCII.GetString(rxBuffer, 1, 4); // 4位元組的資料
                        string calcSum = CalculateChecksum(rxData + (char)0x03);
                        string recSum = Encoding.ASCII.GetString(rxBuffer, 6, 2);
                        if (calcSum != recSum)
                        {
                            throw new FormatException($"和校驗 (Checksum) 錯誤。計算值: {calcSum}, 接收值: {recSum}");
                        }

                        // 解析資料
                        return ParseDeviceStates(rxData);
                    }
                    catch (Exception ex)
                    {
                        retry--;
                        RaiseLog("ERROR", $"讀取失敗: {ex.Message}。剩餘重試次數: {retry}");
                        if (retry == 0)
                        {
                            throw;
                        }
                        System.Threading.Thread.Sleep(50); // 稍作等待後重試
                    }
                }
            }

            throw new Exception("讀取時發生未知錯誤。");
        }

        /// <summary>
        /// 強制單點控制 (Force ON/OFF) 
        /// </summary>
        /// <param name="index">接點編號。0~7 代表 Y0~Y7，8~15 代表 Y10~Y17 (內部會處理八進位轉換)</param>
        /// <param name="state">true 代表強制的 ON，false 代表強制的 OFF</param>
        public void ForceCoil(int index, bool state)
        {
            // 將十進位 index 轉換為 PLC 八進位偏移量 N
            // index 0~7   => N = 0~7
            // index 8~15  => N = 8~15 (對應八進位 10~17)
            // 單點強制位址 = 0x0500 + N。
            // 例如 Y17 (index 15) -> N = 15 = 0x0F -> Address = 0x050F
            int addressOffset = 0x0500 + index;
            
            // 地址在傳輸時順序為「低位在前，高位在後」
            byte lowByte = (byte)(addressOffset & 0xFF);
            byte highByte = (byte)((addressOffset >> 8) & 0xFF);
            
            string addrStr = lowByte.ToString("X2") + highByte.ToString("X2");

            // CMD: '7' 代表 Force ON，'8' 代表 Force OFF
            char cmd = state ? '7' : '8';
            string body = cmd + addrStr;
            string checksum = CalculateChecksum(body + (char)0x03);

            byte[] txBuffer = BuildPacket(body, checksum);

            lock (_lockObject)
            {
                if (!IsOpen)
                {
                    throw new InvalidOperationException("Serial Port 尚未連線。");
                }

                int retry = 3;
                while (retry > 0)
                {
                    try
                    {
                        // 丟棄快取
                        _serialPort.DiscardInBuffer();

                        // 發送
                        string stateText = state ? "ON" : "OFF";
                        RaiseLog("TX", FormatBytesToHex(txBuffer) + $" (強制 Y{GetOctalString(index)} {stateText})");
                        _serialPort.Write(txBuffer, 0, txBuffer.Length);

                        // PLC 強制指令成功時通常回傳 1 個 byte 的 ACK (0x06)；失敗回傳 NAK (0x15)
                        int rxByte = _serialPort.ReadByte();
                        
                        RaiseLog("RX", rxByte.ToString("X2"));

                        if (rxByte == 0x06)
                        {
                            RaiseLog("INFO", $"強制 Y{GetOctalString(index)} {stateText} 成功。");
                            return; // 成功
                        }
                        else if (rxByte == 0x15)
                        {
                            throw new Exception("PLC 回傳 NAK (拒絕執行)。");
                        }
                        else
                        {
                            throw new FormatException($"PLC 回傳未知回應碼: 0x{rxByte:X2}");
                        }
                    }
                    catch (Exception ex)
                    {
                        retry--;
                        RaiseLog("ERROR", $"強制控制控制失敗: {ex.Message}。剩餘重試次數: {retry}");
                        if (retry == 0)
                        {
                            throw;
                        }
                        System.Threading.Thread.Sleep(50);
                    }
                }
            }
        }

        /// <summary>
        /// 計算三菱 FX 編程口校驗和
        /// 將字串中所有字元的值相加，取低 8 位元，並轉為 2 字元 Hex 大寫字串
        /// </summary>
        public static string CalculateChecksum(string input)
        {
            int sum = 0;
            foreach (char c in input)
            {
                sum += c;
            }
            return (sum & 0xFF).ToString("X2");
        }

        private byte[] BuildPacket(string body, string checksum)
        {
            // STX (1 byte) + BODY + ETX (1 byte) + CHECKSUM (2 bytes)
            byte[] packet = new byte[1 + body.Length + 1 + 2];
            packet[0] = 0x02; // STX
            
            byte[] bodyBytes = Encoding.ASCII.GetBytes(body);
            Array.Copy(bodyBytes, 0, packet, 1, bodyBytes.Length);
            
            packet[1 + body.Length] = 0x03; // ETX
            
            byte[] checksumBytes = Encoding.ASCII.GetBytes(checksum);
            Array.Copy(checksumBytes, 0, packet, 1 + body.Length + 1, 2);

            return packet;
        }

        /// <summary>
        /// 解析讀取到的 4 字元 ASCII 狀態資料
        /// 資料格式：例如 "5584"，對應 4 個 Hex 字元
        /// 字元 0 (即 '5')：對應元件 index 4~7 (低位到高位)
        /// 字元 1 (即 '5')：對應元件 index 0~3
        /// 字元 2 (即 '8')：對應元件 index 12~15 (Y14~Y17)
        /// 字元 3 (即 '4')：對應元件 index 8~11 (Y10~Y13)
        /// </summary>
        private bool[] ParseDeviceStates(string data)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException("解析資料長度必須為 4 個字元。");
            }

            int val0_3 = Convert.ToInt32(data[1].ToString(), 16);  // 低位組的低 4 位 (Y0~Y3)
            int val4_7 = Convert.ToInt32(data[0].ToString(), 16);  // 低位組的高 4 位 (Y4~Y7)
            int val8_11 = Convert.ToInt32(data[3].ToString(), 16); // 高位組的低 4 位 (Y10~Y13)
            int val12_15 = Convert.ToInt32(data[2].ToString(), 16);// 高位組的高 4 位 (Y14~Y17)

            bool[] states = new bool[16];

            // 解析 index 0~3
            states[0] = (val0_3 & 0x01) != 0;
            states[1] = (val0_3 & 0x02) != 0;
            states[2] = (val0_3 & 0x04) != 0;
            states[3] = (val0_3 & 0x08) != 0;

            // 解析 index 4~7
            states[4] = (val4_7 & 0x01) != 0;
            states[5] = (val4_7 & 0x02) != 0;
            states[6] = (val4_7 & 0x04) != 0;
            states[7] = (val4_7 & 0x08) != 0;

            // 解析 index 8~11
            states[8] = (val8_11 & 0x01) != 0;
            states[9] = (val8_11 & 0x02) != 0;
            states[10] = (val8_11 & 0x04) != 0;
            states[11] = (val8_11 & 0x08) != 0;

            // 解析 index 12~15
            states[12] = (val12_15 & 0x01) != 0;
            states[13] = (val12_15 & 0x02) != 0;
            states[14] = (val12_15 & 0x04) != 0;
            states[15] = (val12_15 & 0x08) != 0;

            return states;
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

        private string FormatBytesToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2")).Append(" ");
            }
            return sb.ToString().Trim();
        }

        private void RaiseLog(string dir, string msg)
        {
            LogMessage?.Invoke(dir, msg);
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
        }
    }
}
