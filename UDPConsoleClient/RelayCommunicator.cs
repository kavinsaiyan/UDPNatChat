using System.Net;
using System.Threading.Tasks;
using UDPConsoleClient;
using UDPConsoleCommonLib;

public class RelayCommunicator : INetworkOperator
{
    private const int HEART_BEAT_INTERVAL = 100;
    private INetworkCommunicator _networkCommunicator;
    private IPEndPoint[] _relayIP;
    private bool _sendHeartBeat = false;
    public RelayCommunicator(INetworkCommunicator networkCommunicator)    
    {
        _networkCommunicator = networkCommunicator;
        _relayIP = new IPEndPoint[] { 
            new IPEndPoint(IPAddress.Loopback, 7777)
        };
    }

    public bool CanProcessMessage(MessageType messageType)
    {
        return messageType == MessageType.HeartBeat;
    }

    public void ProcessMessage(MessageType messageType, ref byte[] data, ref int pos, IPAddress senderAddress)
    {
        Logger.Log("Received Heartbeat from "+senderAddress.ToString());
    }

    public async Task SendInitialDataAsync()
    {
        byte messageType = (byte)MessageType.InitialData;
        _networkCommunicator.Buffer.ResetPointer();

        _networkCommunicator.Buffer.WriteByte(in messageType);

        string localIp = NetworkExtensions.GetLocalIPAddress().ToString();
        _networkCommunicator.Buffer.WriteString(in localIp);

        _networkCommunicator.Buffer.WriteInt(7777);

        await _networkCommunicator.SendToAsync(_relayIP);

        Logger.Log("sent initial data");
        StartSendingHeartbeat();
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
        Logger.Log("Starting to send heart beat");
        while (_sendHeartBeat)
        {
            Logger.Log("Inside loop to send heart beat");
            await Task.Delay(HEART_BEAT_INTERVAL);
            if (_networkCommunicator.CanSend())
            {
                Logger.Log("sending heart beat");
                _networkCommunicator.Buffer.ResetPointer();
                byte messageType = (byte)MessageType.HeartBeat;
                _networkCommunicator.Buffer.WriteByte(in messageType);
                await _networkCommunicator.SendToAsync(_relayIP);
            }
        }
    }
}