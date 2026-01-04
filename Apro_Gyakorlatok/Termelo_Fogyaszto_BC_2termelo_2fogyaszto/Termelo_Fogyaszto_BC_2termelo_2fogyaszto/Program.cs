using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Termelo_Fogyaszto_BC_2termelo_2fogyaszto
{
    internal class Program
    {
        static int nrProducer = 2;
        static int nrConsumer = 2;
        static BlockingCollection<int> verem = new BlockingCollection<int>(3);
        static CountdownEvent prodDone = new CountdownEvent(nrProducer);
        static CountdownEvent consDone = new CountdownEvent(nrConsumer);
        static object consoleLocker = new object();

        static void Producer(object id)
        {
            for (int i = 0; i < 10; i++)
            {
                int value = (int)id * 100 + i;
                verem.Add(value);
                lock (consoleLocker) Console.WriteLine($"Added: {value}, cid= {id}");
            }
            prodDone.Signal();
        }

        static void Consumer(object id) {
            foreach (int item in verem.GetConsumingEnumerable())
            {
                lock (consoleLocker) Console.WriteLine($"Get: {item}, pid: {id}");
                Thread.Sleep(500);
            }
            lock (consoleLocker) Console.WriteLine("Finished Getting");
            consDone.Signal();
        }

        static void Main(string[] args)
        {
            for (int i = 0; i < nrProducer; i++) ThreadPool.QueueUserWorkItem(Producer, i + 1);
            for (int i = 0; i < nrConsumer; i++) ThreadPool.QueueUserWorkItem(Consumer, i + 1);

            prodDone.Wait();
            verem.CompleteAdding();
            consDone.Wait();
            Console.WriteLine("End");
        }
    }
}
