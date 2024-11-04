using System.Net;
using System.Threading.Tasks;
using UDPConsoleCommonLib;

public class RelayCommunicator : INetworkOperator
{
    private INetworkCommunicator _networkCommunicator;
    public RelayCommunicator(INetworkCommunicator networkCommunicator)    
    {
        _networkCommunicator = networkCommunicator;
    }

    public bool CanProcessMessage(MessageType messageType)
    {
        return messageType == MessageType.HeartBeat;
    }

    public void ProcessMessage(in byte[] data, in int pos, in int len, in IPAddress senderAddress)
    {

    }


    public void SendInitialData()
    {
        byte messageType = (byte)MessageType.InitialData;
        byte[] _buffer = _networkCommunicator.Buffer;
        int pos = 0;

        NetworkExtensions.WriteByte(ref _buffer, ref pos, in messageType);

        string localIp = NetworkExtensions.GetLocalIPAddress().ToString();
        NetworkExtensions.WriteString(ref _buffer, ref pos, in localIp);

        int port = 7777;
        NetworkExtensions.WriteInt(ref _buffer, ref pos, in port);

        // _networkCommunicator.SetDataToSend(new IPAddress[0],0, pos);
    }

    public async Task SendHeartbeat()
    {

    }
}