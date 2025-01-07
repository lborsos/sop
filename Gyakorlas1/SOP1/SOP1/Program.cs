using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SOP1
{
    internal class Program
    {
        const int N = 1000000;
        static int[] data = new int[N];
        static int sum = 0;
        static void proc1()
        {
            for (int i = 0; i < N/2; i++)
            {
                // lock (typeof(Program) {; }
                lock (data)
                {
                    sum += data[i];
                }
                // Console.WriteLine(i);
            }
        }
        static void proc2()
        {
            for (int i = N/2; i < N; i++)
            {
                lock (data)
                {
                    sum += data[i];
                }
                // Console.WriteLine(i);
            }
        }
        static void Main(string[] args)
        {
            for (int i = 0; i < N; i++) {
                data[i] = i;
            }
            Thread t1 = new Thread(proc1); t1.Priority = ThreadPriority.Highest;
            Thread t2 = new Thread(proc2); t2.Priority = ThreadPriority.Normal;
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Console.WriteLine(sum);
            //proc1();
            //proc2();
        }
    }
}
