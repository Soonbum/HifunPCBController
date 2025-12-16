using System.IO.Ports;
using System.Text;

namespace HifunPCBController;

public partial class HifunPCBController : Form
{
    private HifunPCB HifunBoard;

    public HifunPCBController()
    {
        InitializeComponent();

        HifunBoard = new HifunPCB();

        // 이벤트 연결
        HifunBoard.LogMessage += ShowLog;
        HifunBoard.DataReceived += ProcessReceivedData;
    }

    // 로그 출력 (UI 스레드 처리)
    private void ShowLog(string msg)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action<string>(ShowLog), msg);
            return;
        }

        rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
        rtbLog.ScrollToCaret();
    }

    // 데이터 수신 처리 (UI 스레드 처리 및 HEX 보기 옵션)
    private void ProcessReceivedData(string data)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action<string>(ProcessReceivedData), data);
            return;
        }

        // 1. 일반 텍스트로 표시 (깨진 문자일 경우 대비)
        rtbLog.AppendText($"[RX-ASCII] {data}\r\n");

        // 2. HEX 코드로 변환하여 표시 (디버깅에 훨씬 유리함)
        string hexOutput = "";
        foreach (char c in data)
        {
            hexOutput += ((int)c).ToString("X2") + " ";
        }

        rtbLog.SelectionColor = Color.Blue;
        rtbLog.AppendText($"[RX-HEX] {hexOutput.Trim()}\r\n");
        rtbLog.SelectionColor = Color.Black;

        // 만약 '7E'로 시작하는 패턴이 보이면 성공!
    }

    private void HifunPCBController_Load(object sender, EventArgs e)
    {
        // 사용 가능한 COM 포트 검색
        string[] ports = SerialPort.GetPortNames();
        cboPorts.Items.AddRange(ports);
        if (ports.Length > 0) cboPorts.SelectedIndex = 0;
    }

    private void btnConnect_Click(object sender, EventArgs e)
    {
        // 콤보박스에서 선택된 게 있으면 그걸로 연결
        if (cboPorts.SelectedItem != null)
        {
            HifunBoard.Connect(cboPorts.SelectedItem.ToString());
        }
        else
        {
            // 선택된 게 없으면 자동 연결 시도
            bool success = HifunBoard.AutoConnect();
            if (!success)
            {
                MessageBox.Show("자동 연결 실패. 포트를 확인하세요.");
            }
        }
    }

    private void btnDisconnect_Click(object sender, EventArgs e)
    {
        HifunBoard.Disconnect();
    }

    private void btnCheckPos_Click(object sender, EventArgs e)
    {
        HifunBoard.ReadPosition();
    }

    private void btnBuzzerOn_Click(object sender, EventArgs e)
    {
        HifunBoard.BeepBuzzer();
    }

    private void btnBuzzerOff_Click(object sender, EventArgs e)
    {
        HifunBoard.StopBuzzer();
    }

    private void btnLedOn_Click(object sender, EventArgs e)
    {
        HifunBoard.SetPWM(255);
    }

    private void btnLedOff_Click(object sender, EventArgs e)
    {
        HifunBoard.SetPWM(0);
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        rtbLog.Clear();
    }
}

public class HifunPCB
{
    private SerialPort serialPort;

    // 데이터가 들어오면 Form으로 전달하기 위한 이벤트
    public event Action<string> DataReceived;
    public event Action<string> LogMessage;

    // 수신 버퍼
    private StringBuilder rxBuffer = new();

    // --- [상태 관리 변수] (C++ pGVariable 대응) ---
    public string TargetMotorPosition { get; set; } = "0"; // 7E01용
    public string TargetMotorSpeed { get; set; } = "100";  // 7E04용
    public int TargetPWM { get; set; } = 0;                // 7E06용

    // 7E05 핀 커맨드용 상태 플래그
    public bool IsFanOn { get; set; } = false;
    public bool IsPTCOn { get; set; } = false;
    public bool IsButtonLEDOn { get; set; } = false;
    public int SignalTowerColorCode { get; set; } = 0;     // 0:Off, 1:Red, 2:Green, etc.
    public int TargetBladePosition { get; set; } = 0;      // 비트 시프트용

    // 버저 상태
    public bool UseShutdownBuzzer { get; set; } = false;
    public bool UsePrintFinishBuzzer { get; set; } = false;

    public HifunPCB()
    {
        serialPort = new();
    }

    public bool IsConnected => serialPort.IsOpen;
    public string CurrentPort => (serialPort != null && serialPort.IsOpen) ? serialPort.PortName : "NoSerialPort";

    public bool AutoConnect()
    {
        LogMessage?.Invoke("자동 연결 시도 중...");
        string foundPort = FindPort();

        if (!string.IsNullOrEmpty(foundPort))
        {
            return Connect(foundPort);
        }
        else
        {
            LogMessage?.Invoke("연결 가능한 Hifun 보드를 찾지 못했습니다.");
            return false;
        }
    }

    public string FindPort()
    {
        string[] ports = SerialPort.GetPortNames();

        // 포트가 하나도 없으면 null 반환
        if (ports.Length == 0) return null;

        foreach (var port in ports)
        {
            // 이미 연결된 포트는 건너뜀
            if (serialPort.IsOpen && serialPort.PortName == port) continue;

            SerialPort testPort = null;
            try
            {
                testPort = new(port, 9600)
                {
                    ReadTimeout = 5000,
                    WriteTimeout = 500
                };
                testPort.Open();
                testPort.DiscardInBuffer(); // 기존 잡동사니 데이터 비우기

                // 연결하자마자 Check Build Platform Current Position 명령을 내림
                testPort.Write("7E02,\r\n");

                // 데이터 수신 대기 (약간의 딜레이 필요)
                System.Threading.Thread.Sleep(1000);

                string response = testPort.ReadExisting();

                LogMessage?.Invoke($"연결 성공: {port} ({testPort.BaudRate}bps)");

                // 검증: '7E' 혹은 '~'가 포함되어 있는지 확인
                // HEX값 7E는 ASCII로 '~'입니다.
                if (response.Contains("7E") || response.Contains("~") || response.Length > 2)
                {
                    LogMessage?.Invoke($"장치 발견: {port}");
                    testPort.Close();
                    return port;
                }
            }
            catch
            {
                // 포트 열기 실패 또는 타임아웃 시 무시
            }
            finally
            {
                if (testPort != null && testPort.IsOpen)
                    testPort.Close();
            }
        }
        return null;
    }

    public bool Connect(string portName)
    {
        try
        {
            if (serialPort.IsOpen) serialPort.Close();
            serialPort.PortName = portName;
            serialPort.BaudRate = 9600;
            serialPort.DataBits = 7;
            serialPort.StopBits = StopBits.Two;
            serialPort.Parity = Parity.None;
            serialPort.ReadTimeout = 500;

            // 데이터 수신 이벤트 연결
            serialPort.DataReceived += serialPort_DataReceived;

            serialPort.Open();
            LogMessage?.Invoke($"연결 성공: {portName} ({serialPort.BaudRate}bps)");
            return true;
        }
        catch(Exception ex)
        {
            LogMessage?.Invoke($"연결 오류: {ex.Message}"); 
            return false;
        }
    }

    public void Disconnect()
    {
        if (serialPort.IsOpen)
        {
            serialPort.DataReceived -= serialPort_DataReceived;
            serialPort.Close();
            serialPort.PortName = "NoSerialPort";
            LogMessage?.Invoke("연결 해제됨");
        }
    }

    // 수신 처리
    private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            // 현재 포트에 들어와 있는 데이터를 몽땅 읽어서 버퍼에 추가
            string rawData = serialPort.ReadExisting();
            rxBuffer.Append(rawData);

            // 버퍼 내용을 문자열로 확인
            // (효율을 위해 loop 안에서 계속 ToString() 하는 것보다 처리 방식이 중요)
            while (true)
            {
                string content = rxBuffer.ToString();

                // 종료 문자 찾기 (여기서는 '\r' CR을 기준으로 합니다) - 0D(\r)
                int eolIndex = content.IndexOf('\r');

                // 종료 문자가 없으면? -> 아직 패킷이 다 안 온 것이므로 대기 (루프 종료)
                if (eolIndex == -1)
                {
                    break;
                }

                // 패킷 추출 (처음부터 ~ \r 앞까지)
                string packet = content.Substring(0, eolIndex);

                // 버퍼 정리 (추출한 부분 + \r(1글자) 제거)
                // 데이터가 "... \r\n" 형식이라면 \r 뒤에 \n이 남습니다. 
                // 이는 다음 턴에서 Trim()으로 제거되거나 여기서 +2를 해서 지울 수도 있습니다.
                rxBuffer.Remove(0, eolIndex + 1);

                // 패킷 다듬기 (앞뒤 공백, \n 등 제거)
                packet = packet.Trim();

                // 유효한 내용이 있을 때만 이벤트 발생
                if (!string.IsNullOrEmpty(packet))
                {
                    // UI 스레드 처리는 이벤트를 받는 쪽에서 Invoke 하거나,
                    // 여기서 바로 DataReceived?.Invoke(packet); 호출
                    DataReceived?.Invoke(packet);
                }
            }
        }
        catch (Exception ex)
        {
            // 에러 처리 (로그 등)
            Console.WriteLine($"Serial Error: {ex.Message}");
        }
    }

    public void WriteAllCommand()
    {
        if (!IsConnected) return;

        StringBuilder sb = new StringBuilder();

        // 1. 모터 이동 (7E01)
        // C++: "7E01" + PositionValue + "0,"
        sb.Append($"7E01{TargetMotorPosition}0,");

        // 2. 현재 위치 확인 (7E02)
        sb.Append("7E02,");

        // 3. 속도 설정 (7E04)
        // C++: "7E04" + SpeedValue + ","
        sb.Append($"7E04{TargetMotorSpeed},");

        // 4. 핀 상태 제어 (7E05) - 비트 연산
        string hexPinCommand = CalculatePinCommandHex();
        sb.Append($"7E05{hexPinCommand},");

        // 5. LED PWM 제어 (7E06)
        // C++: "7E06" + 3자리 숫자 + ","
        string pwmStr = Math.Max(0, Math.Min(255, TargetPWM)).ToString("D3");
        sb.Append($"7E06{pwmStr},");

        // 6. 버저 제어 (7E07, 7E08)
        if (UseShutdownBuzzer) sb.Append("7E07,");
        if (UsePrintFinishBuzzer) sb.Append("7E08,");

        // 최종 전송 (C++: HIFUN Board일 경우 \r\n 추가)
        SendRaw(sb.ToString());
    }

    // 7E05 핀 커맨드 계산 로직 (C++ 코드 ulPinCommand 부분 이식)
    private string CalculatePinCommandHex()
    {
        long ulPinCommand = 0;

        // 비트 연산 로직
        // 1 << (iTargetBladePosition + 18)
        ulPinCommand |= (1L << (TargetBladePosition + 18));

        // PTC: On=17, Off=16
        ulPinCommand |= (1L << (IsPTCOn ? 17 : 16));

        // FAN: On=14, Off=13
        ulPinCommand |= (1L << (IsFanOn ? 14 : 13));

        // Temperature (Always bit 15 set in C++)
        ulPinCommand |= (1L << 15);

        // Button LED: On=12, Off=11
        ulPinCommand |= (1L << (IsButtonLEDOn ? 12 : 11));

        // Signal Tower Color (그대로 비트 시프트)
        ulPinCommand |= (1L << SignalTowerColorCode);

        // 추가 로직 (ResinPumpStatusCode 등)은 필요 시 여기에 추가
        // ulPinCommand |= (1L << ResinPumpStatusCode);

        // HEX 변환 및 포맷팅 (대문자, 6자리 0으로 채움)
        // C++: sHexCommand.rightJustified(iWidth, QLatin1Char('0'))
        // 통상적으로 5~6자리 유지
        return ulPinCommand.ToString("X").PadLeft(6, '0');
    }

    // 명령어 전송 (C++ 코드 분석: 명령어 뒤에 \r\n 필수)
    private void SendRaw(string command)
    {
        if (!serialPort.IsOpen) return;

        // Hifun 보드는 명령어 끝에 \r\n(CRLF)을 붙여야 함
        string finalPacket = command + "\r\n";

        try
        {
            serialPort.Write(finalPacket);
            LogMessage?.Invoke($"[TX-ALL] {command}"); // 로그에는 \r\n 제외하고 표시
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"전송 실패: {ex.Message}");
        }
    }

    // --- 기능별 명령어 메서드 (C++ 코드 기반) ---
    // 현재 위치 읽기
    public void ReadPosition()
    {
        SendRaw("7E02,");
    }

    // 버저 울리기
    public void BeepBuzzer()
    {
        SendRaw("7E07,");
    }

    // 버저 중지
    public void StopBuzzer()
    {
        SendRaw("7E08,");
    }

    // PWM(LED) 제어 (7E06 + 3자리 숫자 + ,)
    public void SetPWM(int value)
    {
        // 0~255 사이 값으로 제한
        value = Math.Max(0, Math.Min(255, value));

        // 3자리 문자열로 변환 (예: 50 -> "050")
        string pwmStr = value.ToString("D3");
        SendRaw($"7E06{pwmStr},");
    }

    // 모터 이동 (7E01 + 값 + 0,)
    public void MoveMotor(string positionValue)
    {
        // 주의: 실제 값 포맷은 사용하시던 모터 드라이버에 맞춰야 할 수 있습니다.
        SendRaw($"7E01{positionValue}0,");
    }
}