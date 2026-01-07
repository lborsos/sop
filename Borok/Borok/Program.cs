using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Borok
{
    class Program
    {
        static void Main(string[] args)
        {
            Supervisor sup = new Supervisor();
            sup.Start();

            Console.ReadLine();
        }
    }

    class Supervisor
    {
        private BlockingCollection<string> redBuffer;
        private BlockingCollection<string> whiteBuffer;

        private List<Producer> producers;
        private List<Consumer> consumers;

        private int remainingProducers;
        private int remainingConsumers;

        public Supervisor()
        {
            redBuffer = new BlockingCollection<string>(10);
            whiteBuffer = new BlockingCollection<string>(10);

            producers = new List<Producer>();
            consumers = new List<Consumer>();

            Producer p1 = new Producer("voros", ConsoleColor.Red, 10, 150, this);
            Producer p2 = new Producer("voros", ConsoleColor.Red, 8, 150, this);
            Producer p3 = new Producer("feher", ConsoleColor.Yellow, 12, 80, this);
            Producer p4 = new Producer("feher", ConsoleColor.Yellow, 9, 80, this);

            producers.Add(p1);
            producers.Add(p2);
            producers.Add(p3);
            producers.Add(p4);

            Consumer c1 = new Consumer("voros", ConsoleColor.Red, this);
            Consumer c2 = new Consumer("voros", ConsoleColor.Red, this);
            Consumer c3 = new Consumer("feher", ConsoleColor.Yellow, this);
            Consumer c4 = new Consumer("feher", ConsoleColor.Yellow, this);

            consumers.Add(c1);
            consumers.Add(c2);
            consumers.Add(c3);
            consumers.Add(c4);

            remainingProducers = producers.Count;
            remainingConsumers = consumers.Count;
        }

        public void Start()
        {
            int i;

            for (i = 0; i < producers.Count; i++)
            {
                Producer p = producers[i];
                Task.Run(new Action(p.Run));
            }

            for (i = 0; i < consumers.Count; i++)
            {
                Consumer c = consumers[i];
                Task.Run(new Action(c.Run));
            }
        }

        public void AddWine(string type, string text)
        {
            if (type == "voros")
            {
                redBuffer.Add(text);
                SafeWrite(ConsoleColor.Red, "BERAK (voros): " + text + "   [db: " + redBuffer.Count + "]");
            }
            else
            {
                whiteBuffer.Add(text);
                SafeWrite(ConsoleColor.Yellow, "BERAK (feher): " + text + "   [db: " + whiteBuffer.Count + "]");
            }
        }

        public string TakeWine(string type)
        {
            if (type == "voros")
            {
                return redBuffer.Take();
            }
            return whiteBuffer.Take();
        }

        public void ProducerFinished()
        {
            lock (this)
            {
                remainingProducers--;
                if (remainingProducers == 0)
                {
                    redBuffer.CompleteAdding();
                    whiteBuffer.CompleteAdding();
                }
            }
        }

        public void ConsumerFinished()
        {
            lock (this)
            {
                remainingConsumers--;
                if (remainingConsumers == 0)
                {
                    SafeWrite(ConsoleColor.Gray, "Kesz. Enter a kilepeshez...");
                }
            }
        }

        public void SafeWrite(ConsoleColor color, string text)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ResetColor();
            }
        }
    }

    class Producer
    {
        public string WineType;
        public ConsoleColor Colour;
        public int Amount;
        public int WorkTime;

        private Supervisor supervisor;

        public Producer(string type, ConsoleColor colour, int amount, int workTime, Supervisor sup)
        {
            WineType = type;
            Colour = colour;
            Amount = amount;
            WorkTime = workTime;
            supervisor = sup;
        }

        public void Run()
        {
            int i;
            for (i = 1; i <= Amount; i++)
            {
                string text = WineType + " bor #" + i;
                supervisor.AddWine(WineType, text);
                Thread.Sleep(WorkTime);
            }

            supervisor.ProducerFinished();
        }
    }

    class Consumer
    {
        public string WineType;
        public ConsoleColor Colour;

        private Supervisor supervisor;

        public Consumer(string type, ConsoleColor colour, Supervisor sup)
        {
            WineType = type;
            Colour = colour;
            supervisor = sup;
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    string wine = supervisor.TakeWine(WineType);
                    supervisor.SafeWrite(Colour, "KIVESZ (" + WineType + "): " + wine);
                    Thread.Sleep(100);
                }
            }
            catch (InvalidOperationException)
            {
                supervisor.ConsumerFinished();
            }
        }
    }
}
