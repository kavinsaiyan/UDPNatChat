using System.Net;
using UDPConsoleCommonLib;
public interface INetworkOperator
{
    public bool CanProcessMessage(MessageType messageType);
    public void ProcessMessage(in byte[] data, in int pos, in int len, in IPAddress senderAddress);
}