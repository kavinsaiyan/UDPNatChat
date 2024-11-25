using System.Net;
using System.Threading.Tasks;
public interface INetworkCommunicator
{
    public ByteArrayBuffer Buffer { get; }
    public Task SendToAsync(IPAddress[] addresses);
    public bool CanSend();
}