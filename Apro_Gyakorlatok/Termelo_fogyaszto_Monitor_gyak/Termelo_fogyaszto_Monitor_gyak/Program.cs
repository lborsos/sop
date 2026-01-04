using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_fogyaszto_Monitor_gyak
{
    internal class Program
    {
        static Queue<int> verem = new Queue<int>();
        static bool kesz = false;
        static object locker = new object();
        static object consoleLocker = new object();

        static void Producer() {
            for (int i = 0; i < 10; i++) {
                Monitor.Enter(locker);
                try
                {
                    verem.Enqueue(i);
                    Monitor.Pulse(locker);
                }
                finally
                {
                    Monitor.Exit(locker);
                }
                Monitor.Enter(consoleLocker);
                try
                {
                    Console.WriteLine($"Termelt: {i}");
                }
                finally
                {
                    Monitor.Exit(consoleLocker);
                }
                Thread.Sleep(500);
            }
            Monitor.Enter(locker);
            try
            {
                kesz = true;
                Monitor.PulseAll(verem);
            }
            finally 
            {
                Monitor.Exit(locker);
            }
            Monitor.Enter(consoleLocker);
            try { Console.WriteLine("Finished Adding!"); }
            finally { Monitor.Exit(consoleLocker); }    
        }
        static void Consumer()
        {
            int item;
            while (true)
            {
                Monitor.Enter(verem);
                try
                {
                    while (verem.Count == 0 && !kesz) {
                        Monitor.Wait(verem);
                    }
                    if (verem.Count == 0 && kesz) break;
                    item = verem.Dequeue();
                }
                finally { Monitor.Exit(locker); }
                Monitor.Enter(consoleLocker);
                try
                {
                    Console.WriteLine($"KIvesz: {item}");
                }
                finally { Monitor.Exit(locker);}
            }
            Monitor.Enter(consoleLocker); 
            try {
                Console.WriteLine("Finished consum!");
                }
            finally { Monitor.Exit(consoleLocker);}
        }
        static void Main(string[] args)
        {
            Thread producer = new Thread(Producer);
            Thread consumer = new Thread(Consumer);

            producer.Start();
            consumer.Start();

            producer.Join();
            consumer.Join();


        }
    }
}
