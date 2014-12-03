using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squishy
{
    class Program
    {
        static void Main(string[] args)
        {
            SquishyServer server = new SquishyServer();
            server.startListen();

        }
    }
}
