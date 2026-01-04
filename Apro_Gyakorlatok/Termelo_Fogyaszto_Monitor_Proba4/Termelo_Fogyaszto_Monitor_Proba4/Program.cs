using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_Fogyaszto_Monitor_Proba4
{
    internal class Program
    {
        static Queue<int> verem = new Queue<int>();
        static bool kesz = false;
        static object consoleLocker = new object();
        static ConsoleColor colProd = ConsoleColor.Green;
        static ConsoleColor colCons = ConsoleColor.Red;

        static void Producer()
        {
            for (int i = 0; i < 10; i++)
            {
                Monitor.Enter(verem);
                try
                {
                    verem.Enqueue(i);
                    Monitor.Pulse(verem);
                } finally { Monitor.Exit(verem); }
                Monitor.Enter(consoleLocker);
                try
                {
                    Console.ForegroundColor = colProd;
                    Console.WriteLine($"Added: {i}");
                } finally { Monitor.Exit(consoleLocker); }
                Thread.Sleep(500);
            }
            Monitor.Enter(verem);
            try
            {
                kesz = true;
                Monitor.PulseAll(verem);
            } finally { Monitor.Exit(verem); }
            Monitor.Enter(consoleLocker);
            try
            {
                Console.ForegroundColor = colProd;
                Console.WriteLine("Adding finished!");
            } finally { Monitor.Exit(consoleLocker); }
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
                Monitor.Enter(consoleLocker);
                try
                {
                    Console.ForegroundColor = colCons;
                    Console.WriteLine($"Getting: {item}");
                } finally { Monitor.Exit(consoleLocker); }
            }
            Monitor.Enter(consoleLocker);
            try
            {
                Console.ForegroundColor = colCons;
                Console.WriteLine("Finished Getting!");
            } finally { Monitor.Exit(consoleLocker); }
        }
        static void Main(string[] args)
        {
            Thread producer = new Thread(Producer);
            Thread consumer = new Thread(Consumer);

            producer.Start();
            consumer.Start();

            producer.Join();
            consumer.Join();

            Console.ReadKey();
        }
    }
}
