using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _03_Lock_Monitor_gyak_01
{
    class Program
    {
        static int counter = 0;
        static object locker = new object();

        static void Worker()
        {
            for (int i = 0; i < 100000; i++)
            {
                Monitor.Enter(locker);
                try
                {
                    counter++;
                } finally 
                { 
                    Monitor.Exit(locker);
                }
            }
        }

        static void Main()
        {
            Thread t1 = new Thread(Worker);
            Thread t2 = new Thread(Worker);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Console.WriteLine(counter);
        }
    }
}
