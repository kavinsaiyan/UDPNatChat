using System.Net;

public interface INetworkCommunicator
{
    public byte[] Buffer { get; }
    public void SetDataToSend(IPAddress[] addresses, int pos, int len);
}