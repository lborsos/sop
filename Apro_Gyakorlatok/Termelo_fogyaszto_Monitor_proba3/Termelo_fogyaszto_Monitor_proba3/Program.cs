using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_fogyaszto_Monitor_proba3
{
    internal class Program
    {
        static Queue<int> verem = new Queue<int>();
        static bool kesz = false;

        static void Producer()
        {
            for (int i = 0; i < 10; i++)
            {
                Monitor.Enter(verem);
                try
                {
                    verem.Enqueue(i);
                    Monitor.Pulse(verem);
                }
                finally { Monitor.Exit(verem); }
                Monitor.Enter(typeof(Program));
                try
                {
                    Console.WriteLine($"Added: {i}");
                }
                finally { Monitor.Exit(typeof(Program)); }
                Thread.Sleep(500);
            }
            Monitor.Enter(verem);
            try
            {
                kesz = true;
                Monitor.PulseAll(verem);
            } finally { Monitor.Exit(verem); }
            Monitor.Enter(typeof (Program));
            try { Console.WriteLine("Finished adding..."); }
            finally { Monitor.Exit(typeof(Program)); }
        }
        static void Consumer()
        {
            int item;
            while (true)
            {
                Monitor.Enter(verem);
                try
                {
                    while (verem.Count == 0 && !kesz) Monitor.Wait(verem);
                    if (verem.Count == 0 && kesz) break;
                    item = verem.Dequeue();
                }
                finally { Monitor.Exit(verem); }
                Monitor.Enter(typeof (Program));
                try
                {
                    Console.WriteLine($"Getting: {item}");
                }
                finally { Monitor.Exit(typeof(Program)); }
            }
            Monitor.Enter(typeof (Program));
            try { Console.WriteLine("Getting finished!"); }
            finally { Monitor.Exit(typeof(Program)); };
        }
        static void Main(string[] args)
        {
            Thread prducer = new Thread(Producer);
            Thread consumer = new Thread(Consumer);

            prducer.Start();
            consumer.Start();

            prducer.Join();
            consumer.Join();

            Console.ReadKey();
        }
    }
}
