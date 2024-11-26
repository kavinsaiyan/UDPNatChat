using System.Net;
using UDPConsoleCommonLib;
public interface INetworkOperator
{
    public bool CanProcessMessage(MessageType messageType);
    public void ProcessMessage(MessageType messageType, ref byte[] data, ref int pos, IPAddress senderAddress);
}