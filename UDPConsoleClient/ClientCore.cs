using System.Net;
using System.Threading.Tasks;
using UDPConsoleClient;
using UDPConsoleCommonLib;

public enum EndPointToUse { Both, Remote, Local }
public class ClientCore : INetworkOperator
{
    private bool _connectedToRelay;
    public bool ConnectedToRelay { get => _connectedToRelay; set => _connectedToRelay = value; }
    private ConnectedClientData _otherClient;

    private readonly MessageType[] _acceptedMessages;
    private INetworkCommunicator _networkCommunicator;
    private IClientCoreResolver _clientCoreResolver;

    public ClientCore(INetworkCommunicator networkCommunicator, IClientCoreResolver clientCoreResolver)
    {
        _networkCommunicator = networkCommunicator;
        _clientCoreResolver = clientCoreResolver;
        _acceptedMessages = new MessageType[] { MessageType.ClientListResponse, MessageType.OtherClientIP };
    }

    public void SetOtherClientID(int id)
    {
        _otherClient = new ConnectedClientData();
        _otherClient.id = id;
    }

    public void SetOtherClientIPAddresses(IPAddress remoteEP, IPAddress localEP)
    {
        _otherClient.SetOtherClientIPAddresses(remoteEP, localEP);
    }

    public bool IsMessageReceivedFromOtherClient(in IPEndPoint messageReceivingEndPoint)
    {
        if(_otherClient.localEP.Equals(messageReceivingEndPoint.Address))
            _otherClient.endPointToUse = EndPointToUse.Local;
        if(_otherClient.remoteEP.Equals(messageReceivingEndPoint.Address))
            _otherClient.endPointToUse = EndPointToUse.Remote;
        _otherClient.port = messageReceivingEndPoint.Port;

        if(_otherClient.endPointToUse != EndPointToUse.Both)
        {
            _otherClient.connectedToOtherClient = true;
            return true;
        }
        return false;
    }

    public bool CanProcessMessage(MessageType messageType)
    {
        for(int i=0;i<_acceptedMessages.Length; i++)
        {
            if(_acceptedMessages[i] == messageType)
                return true;
        }
        return false;
    }

    public async Task ProcessMessageAsync(MessageType messageType, ByteArrayBuffer buffer, IPAddress senderAddress)
    {
        Logger.Log("process message " + messageType);
        switch (messageType)
        {
            case MessageType.ClientListResponse:
                int len = buffer.ReadInt();
                int[] clientIds = new int[len];
                for (int i = 0; i < len; i++)
                {
                    int clientID = buffer.ReadInt();
                    clientIds[i] = clientID;
                }
                int randomClientToConnect = CommonFunctioncs.RandomRange(0, len);
                SetOtherClientID(randomClientToConnect);
                await _clientCoreResolver.RequestClientIP(randomClientToConnect);
                break;
            case MessageType.OtherClientIP:
                string otherClientIP = buffer.ReadString();
                int port = buffer.ReadInt();
                Logger.Log($"[ClientCore.cs/ProcessMessage]: Other client ip is {otherClientIP} and port is {port}");
                break;    
            default:
                Logger.LogError("[ClientCore.cs/ProcessMessage]: Unhandled message type "+messageType);
                break;
        }
    }
}

public class ConnectedClientData
{
    public bool connectedToOtherClient = false;
    public int id;
    public IPAddress remoteEP;
    public IPAddress localEP;
    public EndPointToUse endPointToUse;
    public int port = 7777;
    public IPEndPoint[] endPoints;

    public void SetOtherClientIPAddresses(IPAddress remoteEP, IPAddress localEP)
    {
        this.remoteEP = remoteEP;
        this.localEP = localEP;
        endPointToUse = EndPointToUse.Both;
    }
}

public interface IClientCoreResolver
{
    public Task RequestClientIP(int id);
}