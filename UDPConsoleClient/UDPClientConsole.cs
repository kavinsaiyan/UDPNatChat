using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UDPConsoleCommonLib;

namespace UDPConsoleClient;
public class UDPClientConsole 
{
    private static bool _stopProgram = false;
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
        _ = Task.Run(ListenForUserInput).ContinueWith(t => { if (t.Exception != null) Logger.LogError(t.Exception.ToString()); });

        try
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Connect(IPAddress.Loopback, 7777);

            networkCommunicator = new NetworkCommunicator(_client);

            relayCommunicator = new RelayCommunicator(networkCommunicator);
            clientCore = new ClientCore(networkCommunicator, relayCommunicator);
            networkOperators = new INetworkOperator[] { relayCommunicator, clientCore };

            _ = Task.Run(ReadAsync).ContinueWith(t => { if (t.Exception != null) Logger.LogError(t.Exception.ToString()); });

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
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any,0);
        while (!_stopProgram)
        {
            await _client.ReceiveFromAsync(networkCommunicator.Buffer.Buffer, endPoint);

            networkCommunicator.Buffer.ResetPointer();
            MessageType messageType = (MessageType)networkCommunicator.Buffer.ReadByte();
            // Logger.Log("received message "+messageType);
            for (int i = 0; i < networkOperators.Length; i++)
            {
                if (networkOperators[i].CanProcessMessage(messageType))
                {
                    await networkOperators[i].ProcessMessageAsync(messageType, networkCommunicator.Buffer, endPoint.Address);
                    continue;
                }
            }
            await Task.Delay(10);
        }
    }

    public static async Task WriteAsync()
    {
        while (!_stopProgram)
        {
            if (_initialDataSent == false)
            {
                _initialDataSent = true;
                await relayCommunicator.SendInitialDataAsync();
            }
            else if (_requestClientList)
            {
                _requestClientList = false;
                await relayCommunicator.RequestClientListAsync();
            }
            await Task.Delay(100);
        }
    }
}