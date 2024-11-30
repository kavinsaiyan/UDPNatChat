using System.Net;
using System.Threading.Tasks;
using UDPConsoleCommonLib;
public interface INetworkOperator
{
    public bool CanProcessMessage(MessageType messageType);
    public Task ProcessMessageAsync(MessageType messageType, ByteArrayBuffer buffer, IPAddress senderAddress);
}