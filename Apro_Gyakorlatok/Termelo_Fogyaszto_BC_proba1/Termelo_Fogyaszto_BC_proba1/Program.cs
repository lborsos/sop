using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_Fogyaszto_BC_proba1
{
    internal class Program
    {
        static BlockingCollection<int> verem = new BlockingCollection<int>(3);
        static CountdownEvent done = new CountdownEvent(1);
        static object consoleLocker = new object();

        static void Producer(object _)
        {
            for (int i = 0; i < 10; i++)
            {
                verem.Add(i);
                lock (consoleLocker) Console.WriteLine($"Add: {i}");
                Thread.Sleep(500);
            }
            verem.CompleteAdding();
            lock (consoleLocker) Console.WriteLine("Adding complete!");
        }

        static void Consumer(object _)
        {
            foreach(int item in verem.GetConsumingEnumerable())
            {
                lock (consoleLocker) Console.WriteLine($"Getting {item}");
            }
            lock (consoleLocker) Console.WriteLine("Getting finished!");
            done.Signal();
        }
        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem( Producer);
            ThreadPool.QueueUserWorkItem(Consumer);

            done.Wait();


        }
    }
}
