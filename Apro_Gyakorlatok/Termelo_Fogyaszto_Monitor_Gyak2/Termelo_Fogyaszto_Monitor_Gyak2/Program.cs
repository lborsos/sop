using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_Fogyaszto_Monitor_Gyak2
{
    internal class Program
    {
        static Queue<int> verem = new Queue<int>();
        static object locker = new object();
        static object consoleLocker = new object();
        static bool kesz = false;

        static void Producer() {
            for (int i = 0; i < 10; i++) { 
                Monitor.Enter(locker);
                try
                {
                    verem.Enqueue(i);
                    Monitor.Pulse(locker);
                }
                finally {
                    Monitor.Exit(locker);
                }
                Monitor.Enter(consoleLocker);
                try {
                    Console.WriteLine($"Added: {i}");
                } finally { Monitor.Exit(consoleLocker); }
                Thread.Sleep(500);
            }
            Monitor.Enter(locker);
            try
            {
                kesz = true;
                Monitor.PulseAll(locker);
            } finally { Monitor.Exit(locker); }
            
            Monitor.Enter(consoleLocker);
            try { Console.WriteLine("Finished adding"); }
            finally { Monitor.Exit(consoleLocker); }
        }
        static void Consumer()
        {
            int item;
            while (true) {
                Monitor.Enter(locker);
                try { 
                    while (verem.Count == 0 && !kesz) { Monitor.Wait(locker); }
                    if (verem.Count == 0 && kesz) break;
                    item = verem.Dequeue();
                } finally { Monitor.Exit(locker); }
                Monitor.Enter(consoleLocker);
                try
                {
                    Console.WriteLine($"Get: {item}");
                } finally { Monitor.Exit(consoleLocker); }
            }
            Monitor.Enter(consoleLocker);
            try { Console.WriteLine("Finished getting!"); }
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
        }
    }
}
