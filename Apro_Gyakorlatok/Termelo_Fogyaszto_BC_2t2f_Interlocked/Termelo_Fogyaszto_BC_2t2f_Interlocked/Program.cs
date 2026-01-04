using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_Fogyaszto_BC_2t2f_Interlocked
{
    internal class Program
    {
        static object consoleLOcker = new object();
        static int nrProducers = 2;
        static int nrConsumers = 2;
        static int prodLeft = nrProducers;
        static BlockingCollection<int> verem = new BlockingCollection<int>(3);
        static CountdownEvent consDone = new CountdownEvent(nrConsumers);

        static void Producer(object id) { 
            for (int i = 0; i<10; i++)
            {
                verem.Add(i);
                Console.WriteLine($"Add {i}");
                Thread.Sleep(500);
            }
            if (Interlocked.Decrement(ref prodLeft) == 0) verem.CompleteAdding();
        }
        static void Consumer(object id) {
            foreach (int item in verem.GetConsumingEnumerable()) { 
                lock (consoleLOcker) Console.WriteLine($"Get: {item}");
            }
            consDone.Signal();
        }

        static void Main(string[] args)
        {
            for (int i = 0; i < nrProducers; i++) ThreadPool.QueueUserWorkItem(Producer,i+1);
            for (int i = 0; i < nrConsumers; i++) ThreadPool.QueueUserWorkItem(Consumer,i+1);
            consDone.Wait();
            lock (consoleLOcker) Console.WriteLine("END");
        }
    }
}
