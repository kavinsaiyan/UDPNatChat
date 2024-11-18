using System.Net;
using System.Threading.Tasks;
using UDPConsoleClient;
using UDPConsoleCommonLib;

public class RelayCommunicator : INetworkOperator
{
    private INetworkCommunicator _networkCommunicator;
    private IPAddress[] _relayIP;
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

    public void ProcessMessage(MessageType messageType, in byte[] data, in int pos, in int len, in IPAddress senderAddress)
    {
        Logger.Log("Received Heartbeat from "+senderAddress.ToString());
    }


    public void SendInitialData()
    {
        byte messageType = (byte)MessageType.InitialData;
        byte[] _buffer = _networkCommunicator.Buffer;
        int pos = 0;

        NetworkExtensions.WriteByte(ref _buffer, ref pos, in messageType);

        string localIp = NetworkExtensions.GetLocalIPAddress().ToString();
        NetworkExtensions.WriteString(ref _buffer, ref pos, in localIp);

        NetworkExtensions.WriteInt(ref _buffer, ref pos, 7777);

        _networkCommunicator.SetDataToSend(_relayIP,0, pos);
    }

    public async Task SendHeartbeat()
    {
        // while (!_)
        // {
        //     if (_client.Connected && _client.Poll(-1, SelectMode.SelectWrite))
        //     {
        //         int pos =0;
        //         byte messageType = (byte)MessageType.HeartBeat;
        //         NetworkExtensions.WriteByte(ref _buffer, ref pos, in messageType);
        //         int bytesSent = await _client.SendAsync(new ArraySegment<byte>(_buffer, 0, pos));
        //     }
        // }
    }
}