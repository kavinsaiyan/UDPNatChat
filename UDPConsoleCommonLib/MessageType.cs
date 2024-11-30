namespace UDPConsoleCommonLib;

public enum MessageType : byte
{
    None, InitialData, HeartBeat, RequestClientList, ConnectToClient, 
    ClientListResponse, DisconnectFromServer, TryConnectToClient, RequestingClientIP, StartConnectingToOtherClient,
    OtherClientIP
}