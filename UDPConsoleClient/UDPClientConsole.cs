using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UDPConsoleCommonLib;

namespace UDPConsoleClient;
public class UDPClientConsole 
{
    private static bool _stopProgram = false;
    private static byte[] _buffer = new byte[4096];

    private static Socket _client = null;
    private static bool _initialDataSent = false;
    private static bool _requestClientList = false;
    private static NetworkCommunicator networkCommunicator;
    private static INetworkOperator[] networkOperators;
    private static RelayCommunicator relayCommunicator;
    private static ClientCore clientCore;

    public static async Task Main(string[] args)
    {
        // TaskScheduler.UnobservedTaskException += (s,e) => Logger.Log(e.ToString());
        _ = Task.Run(ListenForUserInput);

        try
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Connect(IPAddress.Loopback, 7777);

            networkCommunicator = new NetworkCommunicator(_client);

            relayCommunicator = new RelayCommunicator(networkCommunicator);
            clientCore = new ClientCore(networkCommunicator);
            networkOperators = new INetworkOperator[] { relayCommunicator, clientCore };

            _ = Task.Run(ReadAsync);

            await Task.Run(WriteAsync);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
        }
        finally
        {
            _client?.Dispose();
        }
    }

    public static void ListenForUserInput()
    {
        while (true)
        {
            ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
            if (consoleKeyInfo.Key == ConsoleKey.Z)
            {
                _stopProgram = true;
                break;
            }
            else if (consoleKeyInfo.Key == ConsoleKey.R)
            {
                _requestClientList = true;
            }
        }
    }

    public static async Task ReadAsync()
    {
        IPEndPoint endPoint = null;
        while (!_stopProgram)
        {
            // if(_client.Connected && _client.Poll(-1,SelectMode.SelectRead))
            {
                await _client.ReceiveFromAsync(_buffer, endPoint);

                int readPos = 0;
                MessageType messageType = (MessageType) NetworkExtensions.ReadByte(ref _buffer, ref readPos);
                for(int i=0; i< networkOperators.Length; i++)
                {
                    if(networkOperators[i].CanProcessMessage(messageType))
                    {
                        networkOperators[i].ProcessMessage(messageType, ref _buffer, ref readPos, endPoint.Address);
                        continue;
                    }
                }
                // Logger.Log("message type is "+messageType);
                // switch(messageType)
                // {
                //     case MessageType.ClientListResponse:
                //         int len = NetworkExtensions.ReadInt(ref _buffer, ref readPos);
                //         int[] clientIds = new int[len];
                //         for(int i = 0; i < len; i++)
                //         {
                //             int clientID = NetworkExtensions.ReadInt(ref _buffer, ref readPos);
                //             clientIds[i] = clientID;
                //         }
                //         int randomClientToConnect = CommonFunctioncs.RandomRange(0, len);
                //         Logger.Log("Random client to connect : "+randomClientToConnect);
                //         break;
                //     case MessageType.DisconnectFromServer:
                //         Logger.Log("Received Server Disconnect");
                //         _stopProgram = true;
                //         break;
                // }
            }
            await Task.Delay(10);
        }
    }

    public static async Task WriteAsync()
    {
        while (!_stopProgram)
        {
            // if (_client.Connected && _client.Poll(-1, SelectMode.SelectWrite))
            // if(networkCommunicator.CanSend())
            {
                int pos =0;
                if(_initialDataSent == false)
                {
                    _initialDataSent = true;

                    await relayCommunicator.SendInitialDataAsync();
                }
                else if(_requestClientList)
                {
                    _requestClientList = false;
                    
                    byte messageType = (byte) MessageType.RequestClientList;
                    NetworkExtensions.WriteByte(ref _buffer, ref pos, in messageType);
                    int bytesSent =  await _client.SendAsync(new ArraySegment<byte>(_buffer, 0, pos));
                }
                else
                {
                    // byte messageType = (byte)MessageType.HeartBeat;
                    // NetworkExtensions.WriteByte(ref _buffer, ref pos, in messageType);
                    // int bytesSent =  await _client.SendAsync(new ArraySegment<byte>(_buffer, 0, pos));
                    // Logger.Log("Written type is " + messageType+ " and sent bytes is "+ bytesSent);
                }
                await Task.Delay(100);
            }
            await Task.Delay(100);
        }
    }

    public static IPAddress GetLocalIPAddress()
    {
        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
        for(int i=0; i < hostEntry.AddressList.Length; i++)
        {
            if(hostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            {
                return hostEntry.AddressList[i];
            }
        }
        return IPAddress.Any;
    }
}