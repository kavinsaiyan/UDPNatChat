using System.Net;
using System.Threading.Tasks;
using UDPConsoleClient;
using UDPConsoleCommonLib;

public class RelayCommunicator : INetworkOperator
{
    private const int HEART_BEAT_INTERVAL = 10;
    private INetworkCommunicator _networkCommunicator;
    private IPAddress[] _relayIP;
    private bool _sendHeartBeat = false;
    public RelayCommunicator(INetworkCommunicator networkCommunicator)    
    {
        _networkCommunicator = networkCommunicator;
        _relayIP = new IPAddress[] { 
            IPAddress.Parse("127.0.0.1")
        };
    }

    public bool CanProcessMessage(MessageType messageType)
    {
        return messageType == MessageType.HeartBeat;
    }

    public void ProcessMessage(MessageType messageType, in byte[] data, in int pos, in int len, IPAddress senderAddress)
    {
        Logger.Log("Received Heartbeat from "+senderAddress.ToString());
    }

    public void SendInitialData()
    {
        byte messageType = (byte)MessageType.InitialData;
        _networkCommunicator.Buffer.ResetPointer();

        _networkCommunicator.Buffer.WriteByte(in messageType);

        string localIp = NetworkExtensions.GetLocalIPAddress().ToString();
        _networkCommunicator.Buffer.WriteString(in localIp);

        _networkCommunicator.Buffer.WriteInt(7777);

        Task.Run(() => _networkCommunicator.SendToAsync(_relayIP));
    }

    public void StartSendingHeartbeat()
    {
        if(_sendHeartBeat)
            return;
        _sendHeartBeat = true;
        Task.Run(SendHeartbeat);
    }
    public void StopSendingHeartbeat()
    {
        _sendHeartBeat = false;
    }

    private async Task SendHeartbeat()
    {
        while (_sendHeartBeat)
        {
            await Task.Delay(HEART_BEAT_INTERVAL);
            if (_networkCommunicator.CanSend())
            {
                _networkCommunicator.Buffer.ResetPointer();
                byte messageType = (byte)MessageType.HeartBeat;
                _networkCommunicator.Buffer.WriteByte(in messageType);
                await _networkCommunicator.SendToAsync(_relayIP);
            }
        }
    }
}