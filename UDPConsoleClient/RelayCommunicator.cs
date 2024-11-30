using System.Net;
using System.Threading.Tasks;
using UDPConsoleClient;
using UDPConsoleCommonLib;

public class RelayCommunicator : INetworkOperator, IClientCoreResolver
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

    public Task ProcessMessageAsync(MessageType messageType, ByteArrayBuffer buffer, IPAddress senderAddress)
    {
        // Logger.Log("Received Heartbeat from "+senderAddress.ToString());
        return Task.CompletedTask;
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
        while (_sendHeartBeat)
        {
            await Task.Delay(HEART_BEAT_INTERVAL);
            // if (_networkCommunicator.CanSend())
            {
                _networkCommunicator.Buffer.ResetPointer();
                byte messageType = (byte)MessageType.HeartBeat;
                _networkCommunicator.Buffer.WriteByte(in messageType);
                await _networkCommunicator.SendToAsync(_relayIP);
            }
        }
    }

    public async Task RequestClientListAsync()
    {
        Logger.Log("requesting clients");
        _networkCommunicator.Buffer.ResetPointer();
        byte messageType = (byte)MessageType.RequestClientList;
        _networkCommunicator.Buffer.WriteByte(in messageType);
        await _networkCommunicator.SendToAsync(_relayIP);
    }
    
    public async Task RequestClientIP(int id)
    {
        _networkCommunicator.Buffer.ResetPointer();

        byte messageType = (byte)MessageType.RequestingClientIP;
        _networkCommunicator.Buffer.WriteByte(messageType);

        _networkCommunicator.Buffer.WriteInt(id);

        await _networkCommunicator.SendToAsync(_relayIP);
    }
}