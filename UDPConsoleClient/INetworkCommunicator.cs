using System.Net;
using UDPConsoleCommonLib;
public interface INetworkCommunicator
{
    public ByteArrayBuffer Buffer { get; }
    public void SendTo(IPAddress[] addresses);
    public bool CanSend();
}