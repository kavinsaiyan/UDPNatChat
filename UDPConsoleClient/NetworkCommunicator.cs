using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
public class NetworkCommunicator : INetworkCommunicator
{
    private ByteArrayBuffer byteArrayBuffer;
    public ByteArrayBuffer Buffer => byteArrayBuffer; 
    private Socket _socket;

    public NetworkCommunicator(Socket socket)
    {
        byteArrayBuffer = new ByteArrayBuffer();
        _socket = socket;
    }

    public bool CanSend()
    {
        return _socket.Connected && _socket.Poll(-1, SelectMode.SelectWrite);
    }

    public async Task SendToAsync(IPEndPoint[] endPoints)
    {
        var data = new ArraySegment<byte>(byteArrayBuffer.Buffer,0, byteArrayBuffer.Pos);
        for(int i =0; i< endPoints.Length; i++)
        {
            await _socket.SendToAsync(data, endPoints[i]);
        }
    }
}