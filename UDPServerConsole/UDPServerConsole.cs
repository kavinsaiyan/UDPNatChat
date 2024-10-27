using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using UDPConsoleCommonLib;
using System.Runtime.InteropServices;

namespace UDPServerConsole;

public static class UDPServerConsole
{
    public static List<ClientData> clientData = new List<ClientData>();

    public static int connectionCounter = 0;
    public static bool stopProgram = false;

    public static Socket server = null;
    public static byte[] buffer = new byte[4096];

    public static async Task Main(string[] args)
    {
        _ = Task.Run(ListenForUserStop);

        server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        try
        {
            //server.Blocking = false;
            server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            server.Bind(new IPEndPoint(IPAddress.Any, 7777));
            //server.Listen();

            _ = Task.Run(ListenForClientsAsync);

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

            if(server.Poll(-1, SelectMode.SelectRead))
            {
                server.ReceiveFrom(buffer, 0, buffer.Length, 0, ref remoteEndPoint);
                if(TryGetClient(remoteEndPoint, out ClientData currentClient))
                {
                    await ProcessRead(currentClient);
                }
                else
                {
                    ClientData newClient = new();
                    clientData.Add(newClient);

                    newClient.remoteEndPoint = (IPEndPoint)remoteEndPoint;
                    newClient.clientID = connectionCounter++;

                    Logger.Log($"Client {newClient.clientID} Connected");
                    await ProcessRead(newClient);
                }
            }
            await Task.Delay(100);
        }
    }

    private static async Task ProcessRead(ClientData currentClient) 
    {
        int readPos = 0,writePos = 0;
        MessageType messageType = (MessageType)buffer[readPos++];

        switch (messageType)
        {
            case MessageType.InitialData:
                string localIp = NetworkExtensions.ReadString(ref buffer, ref readPos);
                int port = NetworkExtensions.ReadInt(ref buffer, ref readPos);
                if(IPAddress.TryParse(localIp, out IPAddress localIPAddress))
                    currentClient.localEndPoint = new IPEndPoint(localIPAddress,port);
                else
                    Logger.LogError("[UPDServerConsole.cs/ProcessRead]: could not parse ip " + localIp);
                break;
            case MessageType.HeartBeat:
                // Logger.Log("[UPDServerConsole.cs/ProcessRead]: Heart beat received from "+ currentClient.clientID);
                currentClient.heartBeatMissCount = 0;
                break;
            case MessageType.RequestClientList:
                NetworkExtensions.WriteByte(ref buffer,ref writePos,(byte)MessageType.ClientListResponse);
                NetworkExtensions.WriteInt(ref buffer, ref writePos, clientData.Count);

                for(int i=0; i< clientData.Count;i++)
                {
                    if(clientData[i] == currentClient)
                        continue;
                    NetworkExtensions.WriteInt(ref buffer,ref writePos, in clientData[i].clientID);
                }
                await server.SendToAsync(new ArraySegment<byte>(buffer,0,writePos),currentClient.remoteEndPoint);
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
                if(server.Poll(-1, SelectMode.SelectWrite))
                {
                    int readPos = 0;
                    ClientData currentClient = clientData[i];
                    NetworkExtensions.WriteByte(ref buffer, ref readPos, (byte)MessageType.HeartBeat);
                    await server.SendToAsync(new ArraySegment<byte>(buffer,0,readPos),currentClient.remoteEndPoint);
                    currentClient.heartBeatMissCount++;
                    if(currentClient.heartBeatMissCount > ClientData.MAX_HEART_BEAT_MISS_COUNT)
                    {
                        clientData.Remove(currentClient);
                        NetworkExtensions.WriteByte(ref buffer, ref readPos, (byte)MessageType.DisconnectFromServer);
                        await server.SendToAsync(new ArraySegment<byte>(buffer,0,readPos), currentClient.remoteEndPoint);
                        Logger.Log("Disconnected client : "+currentClient.clientID);
                    }
                }
                //await Task.Delay(10);
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