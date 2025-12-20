using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_Fogyaszto_BlockingCollection_ThreadPool
{
    internal class Program
    {
        static BlockingCollection<int> puffer = new BlockingCollection<int>(2);
        static CountdownEvent done = new CountdownEvent(1);

        static void Producer(object _) {
            for (int i = 0; i < 10; i++) { 
                puffer.Add(i);
                Console.WriteLine($"Termelt: {i}");
                Thread.Sleep(300);
            }
            puffer.CompleteAdding();
        }

        static void Consumer(object _) { 
            int first = puffer.Take();
            Console.WriteLine($"Elso elem: {first}");
            foreach (int item in puffer.GetConsumingEnumerable()) { 
                Console.WriteLine($"Fogyasztott: {item}");
            }
            Console.WriteLine("Fogyaszto vege!");
            done.Signal(); // jelzi a Main-nek
        }
        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem( Producer);
            ThreadPool.QueueUserWorkItem( Consumer);

            done.Wait();   // Main itt vár
            Console.ReadKey();
        }
    }
}
