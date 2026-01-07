using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
/*
 IP címek –BlockingCollection és ThreadPool használatával oldja meg a következő 
termelőfogyasztó problémát! A termelők IP címeket állítanak elő. 
Négy típusú termelő van: A, B C és D, amelyek csak ilyen osztályú címeket állítanak elő. 
Mindegyik különböző mennyiségben állítja elő ezeket. 
Az A osztályú cím előállításához kell a legkevesebb idő, 
a legtöbb a D osztályú címhez (Thread.Sleep). 
A fogyasztóknak is négy típusa van, A, B, C és D, akik csak ilyen osztályú címeket vesznek ki
a pufferből. A berakásról és a kivételről is üzeneteket írnak ki, minden címtípushoz 
különböző szín tartozzon. 
A fogyasztók valamennyi címet feldolgozzák. A kollekciókhoz (4 darab van) hozzáférést,
a termelők és a fogyasztók kontrollálását a Supervisor osztály végezze.
 A termelő osztály mezői: 
IP_type. értéke A vagy B vagy C vagy D – Char típus. Mit állít elő. 
Colour: 3 szín lehet, mindháromhoz eltérő szín. ConsoleColor típusú 
Amount: hány címet kell előállítania. int. 
WorkTime: a „termelés” után hány millisec-et kell várni. (Lehet előtte is.:-). int. 
A fogyasztó osztály mezői 
IP_type: értéke A vagy B vagy C vagy D. Char típus. Mit fogyaszt. 
Colour: 3 szín lehet, mindháromhoz eltérő szín. ConsoleColor típusú 
IP Address classes: 
A. 1.0.0.0 – 126.255.255.255. 
B. 128.0.0.0 – 191.255.255.255 
C. 192.0.0.0. – 223.255.255.255 
D. 224.0.0.0 – 239.255.255.255 

Ezeket a Random osztály segítségével generálja. Négy termelő és négy fogyasztó legyen! 
(És persze négy tároló, különböző kapacitásokkal!) 
A fenti korlátokon kívül más megkötés nincs. 

 */

namespace IP_Cimek
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Supervisor supervisor = new Supervisor();
            supervisor.Start();

            Console.ForegroundColor = ConsoleColor.Gray;
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

            producers = new List<Producer>();
            consumers = new List<Consumer>();
            remainingProducers = 0;

            Producer pA = new Producer('A', ConsoleColor.Green, 40, 40);
            Producer pB = new Producer('B', ConsoleColor.Cyan, 30, 80);
            Producer pC = new Producer('C', ConsoleColor.Yellow, 25, 120);
            Producer pD = new Producer('D', ConsoleColor.Magenta, 20, 180);

            producers.Add(pA);
            producers.Add(pB);
            producers.Add(pC);
            producers.Add(pD);

            Consumer cA = new Consumer('A', ConsoleColor.Green);
            Consumer cB = new Consumer('B', ConsoleColor.Cyan);
            Consumer cC = new Consumer('C', ConsoleColor.Yellow);
            Consumer cD = new Consumer('D', ConsoleColor.Magenta);

            consumers.Add(cA);
            consumers.Add(cB);
            consumers.Add(cC);
            consumers.Add(cD);

            remainingProducers = producers.Count;
        }

        public void Start()
        {
            for (int i = 0; i < consumers.Count; i++)
            {
                Consumer consumer = consumers[i];
                consumer.SetSupervisor(this);

                ThreadPool.QueueUserWorkItem(new WaitCallback(ConsumerWork), consumer);
            }

            for (int i = 0; i < producers.Count; i++)
            {
                Producer producer = producers[i];
                producer.SetSupervisor(this);

                ThreadPool.QueueUserWorkItem(new WaitCallback(ProducerWork), producer);
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
        private Random random;

        public Producer(char ipType, ConsoleColor colour, int amount, int workTime)
        {
            IP_type = ipType;
            Colour = colour;
            Amount = amount;
            WorkTime = workTime;

            random = new Random(Guid.NewGuid().GetHashCode());
        }

        public void SetSupervisor(Supervisor sup)
        {
            supervisor = sup;
        }

        public void Run(BlockingCollection<string> buffer)
        {
            for (int i = 0; i < Amount; i++)
            {
                string ip = GenerateRandomIp(IP_type);

                buffer.Add(ip);

                supervisor.SafeWriteLine(Colour, "TERMEL (" + IP_type + "): " + ip + "   [puffer meret: " + buffer.Count + "]");

                Thread.Sleep(WorkTime);
            }

            supervisor.SafeWriteLine(ConsoleColor.Gray, "TERMEL VEGE (" + IP_type + ")");
        }

        private string GenerateRandomIp(char ipType)
        {
            int a = 0;
            int b = 0;
            int c = 0;
            int d = 0;

            if (ipType == 'A')
            {
                a = random.Next(1, 127); 
            }
            else if (ipType == 'B')
            {
                a = random.Next(128, 192); 
            }
            else if (ipType == 'C')
            {
                a = random.Next(192, 224);
            }
            else
            {
                a = random.Next(224, 240);
            }

            b = random.Next(0, 256);
            c = random.Next(0, 256);
            d = random.Next(0, 256);

            return a.ToString() + "." + b.ToString() + "." + c.ToString() + "." + d.ToString();
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
                supervisor.SafeWriteLine(Colour, "FOGYASZT (" + IP_type + "): " + ip + "   [puffer meret: " + buffer.Count + "]");

                //Thread.Sleep(30);
            }

            supervisor.SafeWriteLine(ConsoleColor.Gray, "FOGYASZTO VEGE (" + IP_type + ")");
        }
    }
}
