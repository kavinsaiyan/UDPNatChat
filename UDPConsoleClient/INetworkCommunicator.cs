using System.Net;
using System.Threading.Tasks;
public interface INetworkCommunicator
{
    public ByteArrayBuffer Buffer { get; }
    public Task SendToAsync(IPEndPoint[] addresses);
    public bool CanSend();
}