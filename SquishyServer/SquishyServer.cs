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
    public class SquishyServer
    {
        // Other Squishies
        private List<SquishyPeer> peers = new List<SquishyPeer>();

        // Self config
        private int port = 5123;
        private IPAddress ipAddress = null;
        private Socket serverSock = null;

        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        // Constructors
        public SquishyServer() { init(IPAddress.Any, this.port); }
        public SquishyServer(int port) { init(IPAddress.Any, port); }
        public SquishyServer(IPAddress ip) { init(ip, this.port); }
        public SquishyServer(IPAddress ip, int port){init(ip, port);}
        private void init(IPAddress ip, int port)
        {
            this.port = port;
            this.ipAddress = ip;
        }

        // Methods
        public async Task startListen()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];
            // Create endpoint
            IPEndPoint localEndPoint = new IPEndPoint(this.ipAddress, this.port);
            // Attach socket
            this.serverSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // bind and listen
                this.serverSock.Bind(localEndPoint);
                this.serverSock.Listen(100);

                while (true)
                {
                    allDone.Reset();
                    
                    Console.WriteLine("Waiting for a connection...");
                    this.serverSock.BeginAccept(new AsyncCallback(AcceptCallback), this.serverSock);

                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the peer.
            SquishyPeer newPeer = new SquishyPeer(handler);
            this.peers.Add(newPeer);
        }

        
    }

    
}
