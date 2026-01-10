using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerFogyBorok_BC_Task
{
    class Wine
    {
        public string Type;
        public ConsoleColor Color;
        public int Amount;

        public Wine(string type, ConsoleColor color, int amount)
        {
            Type = type;
            Color = color;
            Amount = amount;
        }
    }

    static class Supervisor
    {
        static BlockingCollection<Wine> redBarrel;
        static BlockingCollection<Wine> whiteBarrel;

        static readonly object locker = new object();
        static readonly object consoleLock = new object();

        static int redProducers;
        static int whiteProducers;

        public static void Init(int barrelSize)
        {
            redBarrel = new BlockingCollection<Wine>(barrelSize);
            whiteBarrel = new BlockingCollection<Wine>(barrelSize);
        }

        public static void ProducerStarts(string type)
        {
            lock (locker)
            {
                if (type == "voros") redProducers++;
                else whiteProducers++;
            }
        }

        public static void ProducerStops(string type)
        {
            lock (locker)
            {
                if (type == "voros")
                {
                    redProducers--;
                    if (redProducers <= 0) redBarrel.CompleteAdding();
                }
                else
                {
                    whiteProducers--;
                    if (whiteProducers <= 0) whiteBarrel.CompleteAdding();
                }
            }
        }

        public static void Produce(Wine w)
        {
            if (w.Type == "voros") redBarrel.Add(w);
            else whiteBarrel.Add(w);

            lock (consoleLock)
            {
                Console.ForegroundColor = w.Color;
                Console.WriteLine("Berakas: " + w.Type + " (1L)");
            }
        }

        public static IEnumerable<Wine> ConsumeAll(string type)
        {
            if (type == "voros") return redBarrel.GetConsumingEnumerable();
            return whiteBarrel.GetConsumingEnumerable();
        }

        public static void Print(string msg, ConsoleColor c)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = c;
                Console.WriteLine(msg);
            }
        }
    }

    class Producer
    {
        string type;
        ConsoleColor color;
        int quantity;
        int workTime;

        public Producer(string type, ConsoleColor color, int quantity, int workTime)
        {
            this.type = type;
            this.color = color;
            this.quantity = quantity;
            this.workTime = workTime;
        }

        public void Make()
        {
            Supervisor.ProducerStarts(type);

            for (int i = 0; i < quantity; i++)
            {
                Thread.Sleep(workTime);
                Supervisor.Produce(new Wine(type, color, 1));
            }

            Supervisor.ProducerStops(type);
            Supervisor.Print("Termelo leallt: " + type, ConsoleColor.Gray);
        }
    }

    class Consumer
    {
        string type;
        ConsoleColor color;

        public Consumer(string type, ConsoleColor color)
        {
            this.type = type;
            this.color = color;
        }

        public void Drink()
        {
            foreach (Wine w in Supervisor.ConsumeAll(type))
            {
                Thread.Sleep(30);
                Supervisor.Print("Kivetel: " + w.Type + " (1L)", color);
            }

            Supervisor.Print("Fogyaszto leallt: " + type, ConsoleColor.Gray);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int barrelSize = 10;
            Supervisor.Init(barrelSize);

            Producer p1 = new Producer("voros", ConsoleColor.Red, 12, 140);
            Producer p2 = new Producer("voros", ConsoleColor.Red, 10, 160);
            Producer p3 = new Producer("feher", ConsoleColor.Yellow, 18, 70);
            Producer p4 = new Producer("feher", ConsoleColor.Yellow, 14, 90);

            Consumer c1 = new Consumer("voros", ConsoleColor.Red);
            Consumer c2 = new Consumer("voros", ConsoleColor.Red);
            Consumer c3 = new Consumer("feher", ConsoleColor.Yellow);
            Consumer c4 = new Consumer("feher", ConsoleColor.Yellow);

            Task t1 = Task.Factory.StartNew(p1.Make, TaskCreationOptions.LongRunning);
            Task t2 = Task.Factory.StartNew(p2.Make, TaskCreationOptions.LongRunning);
            Task t3 = Task.Factory.StartNew(p3.Make, TaskCreationOptions.LongRunning);
            Task t4 = Task.Factory.StartNew(p4.Make, TaskCreationOptions.LongRunning);

            Task t5 = Task.Factory.StartNew(c1.Drink, TaskCreationOptions.LongRunning);
            Task t6 = Task.Factory.StartNew(c2.Drink, TaskCreationOptions.LongRunning);
            Task t7 = Task.Factory.StartNew(c3.Drink, TaskCreationOptions.LongRunning);
            Task t8 = Task.Factory.StartNew(c4.Drink, TaskCreationOptions.LongRunning);

            Task.WaitAll(t1, t2, t3, t4);
            Task.WaitAll(t5, t6, t7, t8);

            Console.ResetColor();
            Console.WriteLine("All done.");
            Console.ReadKey();
        }
    }
}
