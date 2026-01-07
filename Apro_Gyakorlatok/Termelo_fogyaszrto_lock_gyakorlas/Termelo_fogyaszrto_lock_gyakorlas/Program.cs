using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_fogyaszrto_lock_gyakorlas
{
    internal class Program
    {
        static Queue<int> puffer = new Queue<int>();
        static bool kesz = false;
        static object locker = new object();
        static object consoleLocker = new object();

        static void Producer()
        {
            for (int i = 0; i < 10; i++)
            {
                lock (locker) { 
                    puffer.Enqueue(i);
                    lock (consoleLocker)
                    {
                        Console.WriteLine($"Termel: {i}");
                    }
                    Thread.Sleep(500);
                }
                lock (locker) { 
                    kesz = true;
                }
            }
        }

        static void Consumer()
        {
            int item;
            while (true)
            {
                lock (locker)
                {
                    if (puffer.Count == 0)
                    {
                        if (kesz) break;
                        continue;
                    }
                    item = puffer.Dequeue();
                }
                lock (consoleLocker) {
                    Console.WriteLine($"Fogyaszto: {item}");
                }
            }
            lock (consoleLocker) {
                Console.WriteLine("Fogyaszto vege!");
            }
        }
        static void Main(string[] args)
        {
            Thread producer = new Thread(Producer);
            Thread consumer = new Thread(Consumer);
            producer.Start();
            consumer.Start();
            producer.Join();
            consumer.Join();
            Console.WriteLine("Vege!");
        }
    }
}
