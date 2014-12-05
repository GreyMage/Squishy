using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Squishy
{
    public class SquishyClient
    {
        // Server
        private SquishyPeer server;

        // Self config
        private int port = 5123;
        private IPAddress ipAddress = null;

        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        public SquishyClient() { init(IPAddress.Any, this.port); }
        public SquishyClient(int port) { init(IPAddress.Any, port); }
        public SquishyClient(IPAddress ip) { init(ip, this.port); }
        public SquishyClient(IPAddress ip, int port){init(ip, port);}
        private void init(IPAddress ip, int port)
        {
            this.port = port;
            this.ipAddress = ip;

            // Try to connect.
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(this.ipAddress, this.port);
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the remote endpoint.
                client.BeginConnect( remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();
                server = new SquishyPeer(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public SquishyPeer getPeer()
        {
            return server;
        }

    }
}
