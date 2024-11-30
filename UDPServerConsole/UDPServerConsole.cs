using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UDPConsoleCommonLib;

namespace UDPServerConsole;

public static class UDPServerConsole
{
    public static List<ClientData> clientData = new List<ClientData>();
    public delegate Task TaskActionDelegate();

    public static int connectionCounter = 0;
    public static bool stopProgram = false;

    public static Socket server = null;
    public static ByteArrayBuffer byteArrayBuffer = new ByteArrayBuffer();

    public static async Task Main(string[] args)
    {
        // TaskScheduler.UnobservedTaskException += (s,e) => Logger.Log(e.ToString());
        _ = Task.Run(ListenForUserStop).ContinueWith(t => { if(t.Exception!=null) Logger.LogError(t.Exception.ToString()); });
        // _ = Task.Run(ListenForUserStop);

        server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        try
        {
            //server.Blocking = false;
            server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            server.Bind(new IPEndPoint(IPAddress.Any, 7777));
            //server.Listen();

            _ = Task.Run(ListenForClientsAsync).ContinueWith(t => { if(t.Exception!=null) Logger.LogError(t.Exception.ToString()); });
            // _ = Task.Run(ListenForClientsAsync);
        
            Logger.Log("Server started listening..");
            await Task.Run(SendHeartBeatAsync);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
        }
        finally
        {
            stopProgram = true;
            server?.Close();
            server?.Dispose();
        }
    }

    public static void ListenForUserStop() 
    {
        while (true)
        {
            ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
            if (consoleKeyInfo.Key == ConsoleKey.Z)
            {
                stopProgram = true;
                break;
            }
        }
    }

    public static async Task ListenForClientsAsync() 
    {
        while (!stopProgram)
        {
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any,0);
            server.ReceiveFrom(byteArrayBuffer.Buffer, 0, byteArrayBuffer.Buffer.Length, 0, ref remoteEndPoint);

            if (TryGetClient(remoteEndPoint, out ClientData currentClient))
            {
                await ProcessRead(currentClient);
            }
            else
            {
                ClientData newClient = new();
                clientData.Add(newClient);

                newClient.remoteEndPoint = (IPEndPoint)remoteEndPoint;
                newClient.clientID = connectionCounter++;

                await ProcessRead(newClient);
            }
            await Task.Delay(100);
        }
    }

    private static async Task ProcessRead(ClientData currentClient) 
    {
        byteArrayBuffer.ResetPointer();
        MessageType messageType = (MessageType)byteArrayBuffer.ReadByte();
        // Logger.Log("message type is "+messageType);

        switch (messageType)
        {
            case MessageType.InitialData:
                string localIp = byteArrayBuffer.ReadString();
                int port = byteArrayBuffer.ReadInt();
                if(IPAddress.TryParse(localIp, out IPAddress localIPAddress))
                {
                    currentClient.localEndPoint = new IPEndPoint(localIPAddress,port);
                    Logger.Log("[UDPServerConsole.cs/ProcessRead]: Connected a client with ID : " + currentClient.clientID);
                }
                else
                    Logger.LogError("[UPDServerConsole.cs/ProcessRead]: could not parse ip " + localIp);
                break;
            case MessageType.HeartBeat:
                // Logger.Log("[UPDServerConsole.cs/ProcessRead]: Heart beat received from "+ currentClient.clientID);
                currentClient.heartBeatMissCount = 0;
                break;
            case MessageType.RequestClientList:
                byteArrayBuffer.ResetPointer();
                byteArrayBuffer.WriteByte((byte)MessageType.ClientListResponse);
                byteArrayBuffer.WriteInt(clientData.Count);

                for(int i=0; i< clientData.Count;i++)
                {
                    if(clientData[i] == currentClient)
                        continue;
                    byteArrayBuffer.WriteInt(in clientData[i].clientID);
                }
                await server.SendToAsync(byteArrayBuffer.GetArraySlice(),currentClient.remoteEndPoint);
                break;    
            case MessageType.RequestingClientIP:
                byteArrayBuffer.ResetPointer();
                byteArrayBuffer.WriteByte((byte)MessageType.OtherClientIP);
                byteArrayBuffer.WriteString("127.0.0.1"); //replying with dummy client ID
                byteArrayBuffer.WriteInt(5644); 

                await server.SendToAsync(byteArrayBuffer.GetArraySlice(),currentClient.remoteEndPoint);
                break;
            default:
                Logger.LogError("[UPDServerConsole.cs/ProcessRead]: Unhandled for " + messageType);
                break;
        }
    }

    private static async Task SendHeartBeatAsync()
    {
        while(!stopProgram)
        {
            for(int i=clientData.Count - 1; i >= 0; i--)
            {
                byteArrayBuffer.ResetPointer();
                ClientData currentClient = clientData[i];
                byteArrayBuffer.WriteByte((byte)MessageType.HeartBeat);
                await server.SendToAsync(byteArrayBuffer.GetArraySlice(), currentClient.remoteEndPoint);
                currentClient.heartBeatMissCount++;
                if (currentClient.heartBeatMissCount > ClientData.MAX_HEART_BEAT_MISS_COUNT)
                {
                    clientData.Remove(currentClient);
                    byteArrayBuffer.ResetPointer();
                    byteArrayBuffer.WriteByte((byte)MessageType.DisconnectFromServer);
                    await server.SendToAsync(byteArrayBuffer.GetArraySlice(), currentClient.remoteEndPoint);
                    Logger.Log("Disconnected client : " + currentClient.clientID);
                }
            }
            await Task.Delay(100);
        }
    }

    public static bool TryGetClient(EndPoint remoteEndPoint, out ClientData currentClient) 
    {
        currentClient = null;
        for (int i = 0; i < clientData.Count;i++)
        {
            if (clientData[i].remoteEndPoint.Equals(remoteEndPoint))
            {
                currentClient = clientData[i];
                return true;
            }
        }
        return false;
    }
}