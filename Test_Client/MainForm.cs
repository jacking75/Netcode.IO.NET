using NetcodeIO.NET.Utils.IO;
using NetcodeIO.NET;
using Org.BouncyCastle.Bcpg;
using System.Net;
using System.Text;

namespace Test_Client;

public partial class MainForm : Form
{
    static readonly byte[] _privateKey = new byte[]
    {
        0x60, 0x6a, 0xbe, 0x6e, 0xc9, 0x19, 0x10, 0xea,
        0x9a, 0x65, 0x62, 0xf6, 0x6f, 0x2b, 0x30, 0xe4,
        0x43, 0x71, 0xd6, 0x2c, 0xd1, 0x99, 0x27, 0x26,
        0x6b, 0x3c, 0x60, 0xf4, 0xb7, 0x15, 0xab, 0xa1,
    };

    const Int64 _protocolId = 0x1122334455667788L;

    private Client _client = new Client();

    private bool _isRunningClient = false;


    public MainForm()
    {
        InitializeComponent();
    }

    private void startClient()
    {
        var server_address = new IPEndPoint[] { new IPEndPoint(IPAddress.Parse(textBox1.Text), Int32.Parse(textBox2.Text)) };

        TokenFactory factory = new TokenFactory(_protocolId, _privateKey);
        byte[] connectToken = factory.GenerateConnectToken(server_address,
        30,
        5,
        1UL,
        1UL,
        new byte[256]);

        _client.OnStateChanged += Client_OnStateChanged;
        _client.OnMessageReceived += Client_OnMessageReceived;

        PrintLog("Connecting...");

        _client.Connect(connectToken);


    }
        
    private void Client_OnStateChanged(ClientState state)
    {
        PrintLog($"Client state changed: {state.ToString()}");

        if (state == ClientState.Connected)
        {
            // connected! start sending stuff.
            _isRunningClient = true;
        }
        else
        {
            if (_isRunningClient)
            {
                _isRunningClient = false;
            }
        }
    }

    private void Client_OnMessageReceived(byte[] payload, int payloadSize)
    {
        // payload 를 스트링으로 변환
        string payloadString = Encoding.UTF8.GetString(payload, 0, payloadSize);
        PrintLog($"Got packet: {payloadSize} byte, {payloadString}");
    }

    private void PrintLog(string log)
    {
        listBox1.Items.Add(log);
    }

    // 접속 하기
    private void button1_Click(object sender, EventArgs e)
    {
        startClient();
    }

    // 끊기
    private void button2_Click(object sender, EventArgs e)
    {
        if (_isRunningClient)
        {
            _client.Disconnect();
            _isRunningClient = false;
        }
    }

    // 더미 텍스트 메시지 보내기
    private void button3_Click(object sender, EventArgs e)
    {
        if (_isRunningClient == false)
        {
            return;
        }

        byte[] byteArray = Encoding.UTF8.GetBytes(textBox3.Text);

        var testPacket = new byte[256];
        using (var testPacketWriter = ByteArrayReaderWriter.Get(testPacket))
        {
            
            testPacketWriter.Write(byteArray);
        }

        PrintLog($"Sent packet: {byteArray.Length} byte, {textBox3.Text}");

        _client.Send(testPacket, byteArray.Length);
    }
}
