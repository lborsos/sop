using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_Fogyaszto_Monitor
{
    internal class Program
    {
        static Queue<int> puffer = new Queue<int>();
        // static object locker = new object();
        static object ConsoleLocker = new object();
        static bool finished = false;

        static void Producer() {
            for (int i = 0; i < 10; i++) { 
                Monitor.Enter(puffer);
                try {
                    puffer.Enqueue(i);
                    Monitor.Pulse(puffer);
                } finally { Monitor.Exit(puffer); }
                Monitor.Enter(ConsoleLocker);
                try { Console.WriteLine($"Termelt: {i}");
                } finally { Monitor.Exit(ConsoleLocker); }
                Thread.Sleep(500);
            }
            Monitor.Enter(puffer);
            try { 
                finished = true;
                Monitor.PulseAll(puffer);
            } finally { Monitor.Exit(puffer); }
        }
        static void Consumer() {
            while (true) {
                int item;
                Monitor.Enter(puffer);
                try {
                    while (puffer.Count == 0 && !finished)
                    { Monitor.Wait(puffer); }
                    if (puffer.Count == 0 && finished) break;
                    item = puffer.Dequeue();
                } finally { Monitor.Exit(puffer); }
                Monitor.Enter(ConsoleLocker);
                try { Console.WriteLine($"Fogyasztott: {item}");
                } finally { Monitor.Exit(ConsoleLocker); }
            }
            Console.WriteLine("Fogyaszto vege!");
        }
        static void Main(string[] args)
        {
            Thread producer = new Thread(Producer);
            Thread consumer = new Thread(Consumer);

            producer.Start(); consumer.Start(); 
            producer.Join(); consumer.Join();

            Console.WriteLine("Vege!"); Console.ReadKey();
        }
    }
}
