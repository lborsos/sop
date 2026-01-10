using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsProdN
{
    class Supervisor
    {
        static readonly List<int> puffer = new List<int>();
        static readonly object locker = puffer;

        static int BuffSize = 50;
        static int NrOfProd = 0;
        static int NrOfCons = 0;
        static bool ProdStopped = false;
        static bool ConsStopped = false;

        public static void ProducersStarts()
        {
            lock (locker)
            {
                NrOfProd++;
            }
        }

        public static void ConsumerStarts()
        {
            lock (locker)
            {
                NrOfCons++;
            }
        }

        public static void ProducerStops()
        {
            lock (locker)
            {
                NrOfProd--;
                if (NrOfProd <= 0)
                    ProdStopped = true;

                Monitor.PulseAll(locker);
            }
        }

        public static void ConsumerStops()
        {
            lock (locker)
            {
                NrOfCons--;
                if (NrOfCons <= 0)
                    ConsStopped = true;

                Monitor.PulseAll(locker);
            }
        }

        public static void Produce(int number)
        {
            lock (locker)
            {
                while (puffer.Count >= BuffSize)
                    Monitor.Wait(locker);

                puffer.Add(number);
                Monitor.PulseAll(locker);
            }
        }

        public static int Consume()
        {
            lock (locker)
            {
                while (puffer.Count == 0)
                {
                    if (ProdStopped)
                        throw new Exception("No more producer");
                    Monitor.Wait(locker);
                }

                int num = puffer[0];
                puffer.RemoveAt(0);
                Monitor.PulseAll(locker);
                return num;
            }
        }
    }

    class Producer
    {
        int start;
        int ende;

        public Producer(int from, int until)
        {
            start = from;
            ende = until;
        }

        public void Make()
        {
            Supervisor.ProducersStarts();
            for (int i = start; i <= ende; i++)
                if (Prime(i))
                    Supervisor.Produce(i);
            Supervisor.ProducerStops();
        }

        bool Prime(int num)
        {
            if (num < 2) return false;
            for (int i = 2; i <= Math.Sqrt(num); i++)
                if (num % i == 0)
                    return false;
            return true;
        }
    }

    class Consumer
    {
        static readonly object consoleLock = new object();
        ConsoleColor c;

        public Consumer(ConsoleColor pc)
        {
            c = pc;
        }

        public void Consume()
        {
            Supervisor.ConsumerStarts();
            while (true)
            {
                try
                {
                    int temp = Supervisor.Consume();
                    lock (consoleLock)
                    {
                        Console.ForegroundColor = c;
                        Console.WriteLine(temp);
                    }
                }
                catch
                {
                    break;
                }
            }

            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("There's no more producer, 1 consumer stopped");
            }

            Supervisor.ConsumerStops();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Producer p1 = new Producer(1000, 2000);
            Producer p2 = new Producer(2001, 3000);
            Producer p3 = new Producer(3001, 4000);
            Producer p4 = new Producer(4001, 5000);

            Thread t1 = new Thread(p1.Make);
            Thread t2 = new Thread(p2.Make);
            Thread t3 = new Thread(p3.Make);
            Thread t4 = new Thread(p4.Make);

            Consumer c1 = new Consumer(ConsoleColor.Blue);
            Consumer c2 = new Consumer(ConsoleColor.Yellow);

            Thread t5 = new Thread(c1.Consume);
            Thread t6 = new Thread(c2.Consume);

            t1.Start(); t2.Start(); t3.Start(); t4.Start();
            t5.Start(); t6.Start();

            t1.Join(); t2.Join(); t3.Join(); t4.Join();
            t5.Join(); t6.Join();

            Console.ResetColor();
            Console.WriteLine("All done.");
            Console.ReadKey();
        }
    }
}