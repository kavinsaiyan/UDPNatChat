using System.Net;

namespace UDPServerConsole
{
    public class ClientData
    {
        public IPEndPoint remoteEndPoint = null;
        public IPEndPoint localEndPoint = null;

        public int clientID = -1;

        public int heartBeatMissCount = 0;
        public const int MAX_HEART_BEAT_MISS_COUNT = 8;
    }
}