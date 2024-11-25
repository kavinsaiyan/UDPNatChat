using System;
using System.Net;
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

    public ClientCore(INetworkCommunicator networkCommunicator)
    {
        _networkCommunicator = networkCommunicator;
        _acceptedMessages = new MessageType[] { MessageType.ClientListResponse , MessageType.TryConnectToClient };
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

    public bool IsMessageReceivedFromOtherClient(in IPAddress messageReceivingEndPoint)
    {
        if(_otherClient.localEP.Equals(messageReceivingEndPoint))
            _otherClient.endPointToUse = EndPointToUse.Local;
        if(_otherClient.remoteEP.Equals(messageReceivingEndPoint))
            _otherClient.endPointToUse = EndPointToUse.Remote;

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

    public void ProcessMessage(MessageType messageType,in byte[] data, in int pos, in int len, IPAddress senderAddress)
    {
        switch(messageType)
        {
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

    public void SetOtherClientIPAddresses(IPAddress remoteEP, IPAddress localEP)
    {
        this.remoteEP = remoteEP;
        this.localEP = localEP;
        endPointToUse = EndPointToUse.Both;
    }
}