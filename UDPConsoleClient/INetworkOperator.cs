using System.Net;
using UDPConsoleCommonLib;
public interface INetworkOperator
{
    public bool CanProcessMessage(MessageType messageType);
    public void ProcessMessage(MessageType messageType, in byte[] data, in int pos, in int len, IPAddress senderAddress);
}