using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_fogyaszto_Monitor_Laci
{
    internal class Program
    {
        static Queue<int> puffer = new Queue<int>();
        static object locker = new object();
        static object consoleLocker = new object();
        static bool kesz = false;

        static void Producer() {
            for (int i = 0; i < 10; i++) {
                Monitor.Enter(locker);
                try
                {
                    puffer.Enqueue(i);
                    Monitor.Pulse(locker);
                } finally {  Monitor.Exit(locker); }
                Monitor.Enter(consoleLocker);
                try
                {
                    Console.WriteLine($"Termelt: {i}");
                } finally { Monitor.Exit(consoleLocker); }
                Thread.Sleep(500);
            }
            Monitor.Enter(locker);
            try { 
                kesz = true;
                Monitor.PulseAll(locker);
            } finally { Monitor.Exit(locker); }
            Monitor.Enter(consoleLocker);
            try { Console.WriteLine("Termelo kesz!");  }
            finally { Monitor.Exit(consoleLocker); }
        }

        static void Consumer() {
            int item;
            while (true) { 
                Monitor.Enter(locker);
                try
                {
                    while (!kesz && puffer.Count == 0) Monitor.Wait(locker);
                    if (kesz && puffer.Count == 0) break;
                    item = puffer.Dequeue();
                } finally { Monitor.Exit(locker); }
                Monitor.Enter(consoleLocker);
                try {
                    Console.WriteLine($"Fogyasztot: {item}");
                } finally{ Monitor.Exit(consoleLocker); }
            }
            Monitor.Enter(consoleLocker) ;
            try { Console.WriteLine("Fogyaszto vege!");  }
            finally { Monitor.Exit(consoleLocker); }
        }
        static void Main(string[] args)
        {
            Thread producer = new Thread(Producer);
            Thread consumer = new Thread(Consumer);

            producer.Start();
            consumer.Start();
            producer.Join();
            consumer.Join();

            Console.WriteLine("Vege");
            Console.ReadKey();
        }
    }
}
