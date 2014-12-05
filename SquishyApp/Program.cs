using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Squishy
{
    class Program
    {

        static SquishyServer server;

        static void Main(string[] args)
        {
            
            server = new SquishyServer();
            Task.Run(() => server.startListen());

            SquishyClient x = new SquishyClient(IPAddress.Loopback,5123);
            SquishyPeer peer = x.getPeer();
            while (peer == null) 
            {
                Console.WriteLine("\nWaiting");
                peer = x.getPeer();
            }
            peer.SendEvent("welcome", null);


            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

    }
}
