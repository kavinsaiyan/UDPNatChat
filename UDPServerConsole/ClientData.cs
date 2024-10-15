using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPServerConsole
{
    public class ClientData
    {
        public IPEndPoint remoteEndPoint = null;
        public IPEndPoint localEndPoint = null;

        public int clientID = -1;
        public Socket socket;
    }
}
