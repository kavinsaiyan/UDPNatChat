using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
public static class NetworkExtensions
{
    public static byte ReadByte(ref byte[] data, ref int pos)
    {
        return data[pos++];
    }

    public static int ReadInt(ref byte[] data, ref int pos)
    {
        int readData = BitConverter.ToInt32(data, pos);
        pos += 4;
        return readData;
    }

    public static string ReadString(ref byte[] data, ref int pos)
    {
        int strlen = BitConverter.ToInt32(data, pos);
        pos += 4;
        string readStr = Encoding.ASCII.GetString(data, pos, strlen);
        return readStr;
    }

    public static void WriteByte(ref byte[] data, ref int pos, in byte byteData)
    {
        data[pos++] = byteData;
    }

    public static void WriteInt(ref byte[] data, ref int pos, in int intData)
    {
        byte[] writtenBytes = BitConverter.GetBytes(intData);
        Buffer.BlockCopy(writtenBytes,0,data,pos,writtenBytes.Length);
    }

    public static void WriteString(ref byte[] data, ref int pos, in string stringData)
    {
        byte[] writtenBytes = Encoding.ASCII.GetBytes(stringData);
        byte[] len = BitConverter.GetBytes(writtenBytes.Length);
        Buffer.BlockCopy(len,0,data,pos,len.Length);
        pos+=len.Length;
        Buffer.BlockCopy(writtenBytes,0,data,pos,writtenBytes.Length);
        pos+=writtenBytes.Length;
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