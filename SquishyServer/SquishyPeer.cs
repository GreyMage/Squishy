using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Squishy
{

    // EVENT PROTOCOL
    /*

    Packets
        NAME(BYTES)
    Event:
        EVENT(1) NAMELENGTH(2) NAME(NAMELENGTH) NUMARGS(1) ARG1LENGTH(4) ARG1(ARG1LENGTH) ARG2LENGTH(4) ARG2(ARG2LENGTH) 

    */

    public class SquishyPeer
    {

        public enum SquishyCodes : byte 
        {
            EVENT
        }

        private SquishyState state = new SquishyState();

        public SquishyPeer(Socket socket)
        {
            state.workSocket = socket;
            state.workSocket.BeginReceive(state.buffer, 0, SquishyState.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            ProcessThread();
        }

        private void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Read data from the client socket. 
            int bytesRead = state.workSocket.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.data.Enqueue(state.buffer, bytesRead);
                state.workSocket.BeginReceive(state.buffer, 0, SquishyState.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }

        public void SendEvent(string name, List<object> args)
        {
            List<byte> payload = new List<byte>();
            // Add event flag
            payload.Add((byte)SquishyCodes.EVENT);

            // This is _extremely fast_ compared to making it a method or something. I dont know why.
            byte[] namelen = new byte[4];
            namelen[0] = (byte)(name.Length >> 24);
            namelen[1] = (byte)(name.Length >> 16);
            namelen[2] = (byte)(name.Length >> 8);
            namelen[3] = (byte)name.Length;
            // Then add it to the payload.
            payload.AddRange(namelen);

            // Copy Event name
            byte[] evname = Encoding.ASCII.GetBytes(name);
            //System.Buffer.BlockCopy(name.ToCharArray(), 0, evname, 0, evname.Length);
            payload.AddRange(evname);

            byte[] finalpayload = payload.ToArray();
            for (int i = 0; i < finalpayload.Length; i++)
            {
                Console.WriteLine("sending {0}", finalpayload[i]);
            }
            Console.WriteLine("Sent {0} bytes to client.", finalpayload.Length);
            state.workSocket.BeginSend(finalpayload, 0, finalpayload.Length, 0, new AsyncCallback(SendCallback), state.workSocket);

        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = state.workSocket.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                
                //state.workSocket.Shutdown(SocketShutdown.Both);
                //state.workSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async Task ProcessThread()
        {
            while (true)
            {

                while (state.data.Count > 0)
                {
                    // Pull off byte, show to screen
                    byte b = state.data.Dequeue();
                    Console.WriteLine("read in {0}", b);
                    if (b.Equals((byte)SquishyCodes.EVENT))
                    {
                        // This is an RPC/Event

                        // Pull in event name length
                        byte[] _namelen = await ensureRead(4);
                        int namelen = (_namelen[3] << 0) | (_namelen[2] << 8) | (_namelen[1] << 16) | (_namelen[0] << 16);

                        // Pull in event name
                        byte[] _evtname = await ensureRead(namelen);
                        string evtname = ASCIIEncoding.ASCII.GetString(_evtname);

                        //and we have a name!
                        Console.WriteLine("Hey this is an event! The name is \"{0}\"", evtname);
                    }
                    Console.Write((char)b);
                }
                await Task.Delay(1);
            }
        }

        // Streams fucking SUCK.
        private async Task<byte[]> ensureRead(int len)
        {
            byte[] ret = new byte[len]; int i = 0;
            while (true)
            {
                //
                while (state.data.Count > 0)
                {
                    ret[i] = state.data.Dequeue();
                    Console.WriteLine("read in {0}", ret[i]);
                    if (++i == ret.Length) break;
                }
                if (i == ret.Length) break;
                await Task.Delay(100);
            }
            return ret;
        }

        private byte[] int2bytes(int x)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(x >> 24);
            bytes[1] = (byte)(x >> 16);
            bytes[2] = (byte)(x >> 8);
            bytes[3] = (byte)x;
            return bytes;
        }
        private int bytes2int(byte[] x)
        {
            return (x[3] << 0) | (x[2] << 8) | (x[1] << 16) | (x[0] << 16);
        }
    }

    

    
}
