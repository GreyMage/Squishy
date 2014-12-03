using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squishy
{
    public class ByteQueue
    {
        private const int szinc = 5;
        private int front = 0;
        private int back = 0;
        public int Count = 0;
        private byte[] array = new byte[szinc];

        public void Enqueue(byte b)
        {
            array[back++] = b;
            checkResize();
        }

        public void Enqueue(byte[] b)
        {
            while (back + b.Length > array.Length)
            {
                flush(array.Length + b.Length);
            }
            Array.Copy(b, 0, array, back, b.Length);
            back = back + b.Length;
            checkResize();
        }

        public byte Dequeue()
        {
            byte b = array[front++];
            checkResize();
            return b;
        }

        public byte[] Dequeue(int count)
        {
            byte[] ret = new byte[count];
            Array.Copy(array, front, ret, 0, count);
            front += count;
            checkResize();
            return ret;
        }

        private void checkResize()
        {
            if (front >= back)
            {
                flush(array.Length);
            }
            if (back >= array.Length)
            {
                flush(array.Length + szinc);
            }
            this.Count = Math.Max(back - front, 0);
        }

        private void flush(int newsize)
        {
            byte[] na = new byte[newsize];
            Array.Copy(array, front, na, 0, Math.Max(back - front, 0));
            this.array = na;
            back = Math.Max(back - front, 0);
            front = 0;
        }
    }
}
