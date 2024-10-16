﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.Encodings;
using System.Text;
using System.Buffers;
using System.Linq;
namespace UDPConsoleClient;
public enum MessageType : byte
{
    None, InitialData, HeartBeat, RequestClientList, ConnectToClient, 
    ClientListResponse, DisconnectFromServer
}

public static class UDPClientConsole
{
    private static bool _stopProgram = false;
    private static byte[] _buffer = new byte[4096];

    private static Socket _client = null;
    private static bool _initialDataSent = false;
    private static bool _requestClientList = false;

    public static async Task Main(string[] args)
    {
        _ = Task.Run(ListenForUserInput);

        _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            _client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Connect(IPAddress.Loopback, 7777);
            
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
        while (!_stopProgram)
        {
            if(_client.Connected && _client.Poll(-1,SelectMode.SelectRead))
            {
                await _client.ReceiveAsync(_buffer);

                int readPos = 0;
                MessageType messageType = (MessageType) _buffer[readPos++];
                // Logger.Log("message type is "+messageType);
                switch(messageType)
                {
                    case MessageType.ClientListResponse:
                        int len = BitConverter.ToInt32(_buffer,readPos);
                        readPos+=4;
                        for(int i = 0; i < len; i++)
                        {
                            int clientID = BitConverter.ToInt32(_buffer, readPos);
                            readPos+=4;
                            Logger.Log("client id : "+clientID);
                        }
                        
                        break;
                    case MessageType.DisconnectFromServer:
                        Logger.Log("Received Server Disconnect");
                        _stopProgram = true;
                        break;
                }
            }
            await Task.Delay(10);
        }
    }

    public static async Task WriteAsync()
    {
        while (!_stopProgram)
        {
            if (_client.Connected && _client.Poll(-1, SelectMode.SelectWrite))
            {
                int pos =0;
                if(_initialDataSent == false)
                {
                    _initialDataSent = true;
                    MessageType messageType = MessageType.InitialData;

                    _buffer[pos++] = (byte)messageType;

                    string localIp = GetLocalIPAddress().ToString();
                    byte[] s = Encoding.ASCII.GetBytes(localIp);
                    byte[] len = BitConverter.GetBytes(s.Length);
                    Buffer.BlockCopy(len, 0, _buffer, pos, len.Length);
                    pos += len.Length;

                    Buffer.BlockCopy(s, 0, _buffer, pos, s.Length);
                    pos += s.Length;
                    
                    int port = 7777;
                    byte[] portData = BitConverter.GetBytes(port);
                    Buffer.BlockCopy(portData, 0, _buffer, pos, portData.Length);
                    pos += portData.Length;

                    int bytesSent =  await _client.SendAsync(new ArraySegment<byte>(_buffer, 0, pos));
                    // Logger.Log("Written type is " + messageType+ " and sent bytes is "+ bytesSent);
                }
                else if(_requestClientList)
                {
                    _requestClientList = false;
                    
                    MessageType messageType = MessageType.RequestClientList;
                    _buffer[pos++] = (byte)messageType;
                    int bytesSent =  await _client.SendAsync(new ArraySegment<byte>(_buffer, 0, pos));
                }
                else
                {
                    MessageType messageType = MessageType.HeartBeat;
                    _buffer[pos++] = (byte)messageType;
                    int bytesSent =  await _client.SendAsync(new ArraySegment<byte>(_buffer, 0, pos));
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