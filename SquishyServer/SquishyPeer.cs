using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Squishy
{

    class SquishyPeer
    {
        private SquishyState state = new SquishyState();

        public SquishyPeer(Socket socket)
        {
            state.workSocket = socket;
            state.workSocket.BeginReceive(state.buffer, 0, SquishyState.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Read data from the client socket. 
            int bytesRead = state.workSocket.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.data.Enqueue(state.buffer, bytesRead);
                if (Encoding.ASCII.GetString(state.buffer, 0, bytesRead).IndexOf("FLUSH") > -1)
                {
                    byte[] flushed = state.data.Dequeue(state.data.Count);
                    Console.Write(Encoding.ASCII.GetString(flushed, 0, flushed.Length));
                }

                Console.WriteLine(state.data.Count);

                // Listen for More
                state.workSocket.BeginReceive(state.buffer, 0, SquishyState.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }

        public void Send(String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            state.workSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), state.workSocket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = state.workSocket.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                state.workSocket.Shutdown(SocketShutdown.Both);
                state.workSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    // State object for reading client data asynchronously
    public class SquishyState
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public ByteQueue data = new ByteQueue();
    }

    
}
