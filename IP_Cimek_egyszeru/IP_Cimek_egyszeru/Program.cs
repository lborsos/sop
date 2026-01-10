using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IP_Cimek_egyszeru
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Supervisor supervisor = new Supervisor();
            supervisor.Start();

            Console.ResetColor();
            Console.ReadLine();
        }
    }

    public class Supervisor
    {
        private BlockingCollection<string> bufferA;
        private BlockingCollection<string> bufferB;
        private BlockingCollection<string> bufferC;
        private BlockingCollection<string> bufferD;

        private List<Producer> producers;
        private List<Consumer> consumers;

        private int remainingProducers;

        public Supervisor()
        {
            bufferA = new BlockingCollection<string>(30);
            bufferB = new BlockingCollection<string>(20);
            bufferC = new BlockingCollection<string>(15);
            bufferD = new BlockingCollection<string>(10);

            producers = new List<Producer>
            {
                new Producer('A', ConsoleColor.Green,   40,  40),
                new Producer('B', ConsoleColor.Cyan,    30,  80),
                new Producer('C', ConsoleColor.Yellow,  25, 120),
                new Producer('D', ConsoleColor.Magenta, 20, 180),
            };

            consumers = new List<Consumer>
            {
                new Consumer('A', ConsoleColor.Green),
                new Consumer('B', ConsoleColor.Cyan),
                new Consumer('C', ConsoleColor.Yellow),
                new Consumer('D', ConsoleColor.Magenta),
            };

            remainingProducers = producers.Count;
        }

        public void Start()
        {
            foreach (var consumer in consumers)
            {
                consumer.SetSupervisor(this);
                ThreadPool.QueueUserWorkItem(ConsumerWork, consumer);
            }

            foreach (var producer in producers)
            {
                producer.SetSupervisor(this);
                ThreadPool.QueueUserWorkItem(ProducerWork, producer);
            }
        }

        private void ProducerWork(object state)
        {
            Producer producer = (Producer)state;
            BlockingCollection<string> buffer = GetBufferByType(producer.IP_type);

            producer.Run(buffer);

            lock (this)
            {
                remainingProducers--;
                if (remainingProducers == 0)
                {
                    bufferA.CompleteAdding();
                    bufferB.CompleteAdding();
                    bufferC.CompleteAdding();
                    bufferD.CompleteAdding();
                }
            }
        }

        private void ConsumerWork(object state)
        {
            Consumer consumer = (Consumer)state;
            BlockingCollection<string> buffer = GetBufferByType(consumer.IP_type);
            consumer.Run(buffer);
        }

        private BlockingCollection<string> GetBufferByType(char ipType)
        {
            if (ipType == 'A') return bufferA;
            if (ipType == 'B') return bufferB;
            if (ipType == 'C') return bufferC;
            return bufferD;
        }

        public void SafeWriteLine(ConsoleColor color, string text)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ResetColor();
            }
        }
    }

    public class Producer
    {
        public char IP_type;
        public ConsoleColor Colour;
        public int Amount;
        public int WorkTime;

        private Supervisor supervisor;
        private int counter = 0;

        public Producer(char ipType, ConsoleColor colour, int amount, int workTime)
        {
            IP_type = ipType;
            Colour = colour;
            Amount = amount;
            WorkTime = workTime;
        }

        public void SetSupervisor(Supervisor sup)
        {
            supervisor = sup;
        }

        public void Run(BlockingCollection<string> buffer)
        {
            for (int i = 0; i < Amount; i++)
            {
                string ip = GenerateId();

                buffer.Add(ip);

                supervisor.SafeWriteLine(Colour,
                    "TERMEL (" + IP_type + "): " + ip + "   [puffer meret: " + buffer.Count + "]");

                Thread.Sleep(WorkTime);
            }

            supervisor.SafeWriteLine(ConsoleColor.Gray, "TERMEL VEGE (" + IP_type + ")");
        }

        private string GenerateId()
        {
            counter++;
            return IP_type.ToString() + counter.ToString("D4"); // A0001, A0002, ...
        }
    }

    public class Consumer
    {
        public char IP_type;
        public ConsoleColor Colour;

        private Supervisor supervisor;

        public Consumer(char ipType, ConsoleColor colour)
        {
            IP_type = ipType;
            Colour = colour;
        }

        public void SetSupervisor(Supervisor sup)
        {
            supervisor = sup;
        }

        public void Run(BlockingCollection<string> buffer)
        {
            foreach (string ip in buffer.GetConsumingEnumerable())
            {
                supervisor.SafeWriteLine(Colour,
                    "FOGYASZT (" + IP_type + "): " + ip + "   [puffer meret: " + buffer.Count + "]");
            }

            supervisor.SafeWriteLine(ConsoleColor.Gray, "FOGYASZTO VEGE (" + IP_type + ")");
        }
    }
}
