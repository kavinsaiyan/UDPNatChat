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
        // return _socket.Connected && _socket.Poll(-1, SelectMode.SelectWrite);
        return true; // since the concept of polling is not applicable to UDP
    }

    public async Task SendToAsync(IPEndPoint[] endPoints)
    {
        var data = byteArrayBuffer.GetArraySlice();
        for(int i =0; i< endPoints.Length; i++)
        {
            await _socket.SendToAsync(data, endPoints[i]);
        }
    }
}