using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsProd4NTPBC
{
    class Program
    {
        static BlockingCollection<int> buffer = new BlockingCollection<int>(50);
        static int producersLeft = 4;

        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(_ => Producer(1000, 2000));
            ThreadPool.QueueUserWorkItem(_ => Producer(2001, 3000));
            ThreadPool.QueueUserWorkItem(_ => Producer(3001, 4000));
            ThreadPool.QueueUserWorkItem(_ => Producer(4001, 5000));

            ThreadPool.QueueUserWorkItem(_ => Consumer(ConsoleColor.Blue));
            ThreadPool.QueueUserWorkItem(_ => Consumer(ConsoleColor.Yellow));

            while (!buffer.IsCompleted)
                Thread.Sleep(100);

            Console.ResetColor();
            Console.WriteLine("All done.");
            Console.ReadKey();
        }

        static void Producer(int from, int to)
        {
            for (int i = from; i <= to; i++)
                if (Prime(i))
                    buffer.Add(i);

            if (Interlocked.Decrement(ref producersLeft) == 0)
                buffer.CompleteAdding();
        }

        static void Consumer(ConsoleColor color)
        {
            foreach (int num in buffer.GetConsumingEnumerable())
            {
                Console.ForegroundColor = color;
                Console.WriteLine(num);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("There's no more producer, 1 consumer stopped");
        }

        static bool Prime(int num)
        {
            if (num < 2) return false;
            for (int i = 2; i * i <= num; i++)
                if (num % i == 0)
                    return false;
            return true;
        }
    }
}
