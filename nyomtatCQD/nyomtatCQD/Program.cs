using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nyomtatCQD
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Termelok szama: ");
            string s = Console.ReadLine();
            int producerCount;
            if (!int.TryParse(s, out producerCount) || producerCount < 1)
            {
                producerCount = 3;
            }

            Supervisor sup = new Supervisor(producerCount);
            sup.Start();

            Console.ReadLine();
        }
    }

    class Document
    {
        public int Id;
        public string Name;
        public string QueueType;

        public Document(int id, string name, string queueType)
        {
            Id = id;
            Name = name;
            QueueType = queueType;
        }
    }

    class Producer
    {
        private int producerId;
        private int amount;
        private Supervisor supervisor;
        private Random random;

        public Producer(int producerId, int amount, Supervisor supervisor)
        {
            this.producerId = producerId;
            this.amount = amount;
            this.supervisor = supervisor;
            random = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Run()
        {
            int i;
            for (i = 0; i < amount; i++)
            {
                string name = GenerateName();
                string queueType;
                if (random.Next(0, 2) == 0) queueType = "tinta";
                else queueType = "lezer";

                int id = supervisor.NextDocumentId();
                Document doc = new Document(id, name, queueType);

                supervisor.Submit(doc, producerId);

                Thread.Sleep(random.Next(30, 120));
            }

            supervisor.ProducerFinished();
        }

        private string GenerateName()
        {
            int len = random.Next(5, 11);
            char[] chars = new char[len];
            int i;
            for (i = 0; i < len; i++)
            {
                chars[i] = (char)('A' + random.Next(0, 26));
            }
            return new string(chars) + ".txt";
        }
    }

    class Printer
    {
        private int printerId;
        private string printerName;
        private string queueType;
        private int workTimeMs;
        private Supervisor supervisor;

        public Printer(int printerId, string printerName, string queueType, int workTimeMs, Supervisor supervisor)
        {
            this.printerId = printerId;
            this.printerName = printerName;
            this.queueType = queueType;
            this.workTimeMs = workTimeMs;
            this.supervisor = supervisor;
        }

        public void Run()
        {
            while (true)
            {
                Document doc;
                bool ok = supervisor.TryGetNext(queueType, out doc);
                if (!ok)
                {
                    break;
                }

                supervisor.LogTake(doc, printerName);

                Thread.Sleep(workTimeMs);

                supervisor.LogPrinted(doc, printerName);
            }

            supervisor.LogEnd(printerName);
        }
    }

    class Supervisor
    {
        private Queue<Document> tintaQueue;
        private Queue<Document> lezerQueue;

        private object lockObj;

        private List<Thread> producerThreads;
        private List<Thread> printerThreads;

        private int remainingProducers;
        private int docId;

        public Supervisor(int producerCount)
        {
            tintaQueue = new Queue<Document>();
            lezerQueue = new Queue<Document>();

            lockObj = new object();

            producerThreads = new List<Thread>();
            printerThreads = new List<Thread>();

            remainingProducers = producerCount;
            docId = 0;

            int i;
            for (i = 1; i <= producerCount; i++)
            {
                int amount = 6 + i * 2;
                Producer p = new Producer(i, amount, this);
                Thread t = new Thread(new ThreadStart(p.Run));
                producerThreads.Add(t);
            }

            Printer pr1 = new Printer(1, "Tintasugaras-1", "tinta", 160, this);
            Printer pr2 = new Printer(2, "Tintasugaras-2", "tinta", 160, this);
            Printer pr3 = new Printer(3, "Lezer-1", "lezer", 80, this);
            Printer pr4 = new Printer(4, "Lezer-2", "lezer", 80, this);

            printerThreads.Add(new Thread(new ThreadStart(pr1.Run)));
            printerThreads.Add(new Thread(new ThreadStart(pr2.Run)));
            printerThreads.Add(new Thread(new ThreadStart(pr3.Run)));
            printerThreads.Add(new Thread(new ThreadStart(pr4.Run)));
        }

        public void Start()
        {
            int i;

            for (i = 0; i < printerThreads.Count; i++)
            {
                printerThreads[i].Start();
            }

            for (i = 0; i < producerThreads.Count; i++)
            {
                producerThreads[i].Start();
            }

            SafeWrite(ConsoleColor.Gray, "Indult. Enter a kilepeshez...");
        }

        public int NextDocumentId()
        {
            return Interlocked.Increment(ref docId);
        }

        public void Submit(Document doc, int producerId)
        {
            lock (lockObj)
            {
                if (doc.QueueType == "tinta")
                {
                    tintaQueue.Enqueue(doc);
                    SafeWrite(ConsoleColor.Yellow,
                        "BERAK (tinta) P" + producerId + " -> Doc#" + doc.Id + " " + doc.Name +
                        "   [tinta sor: " + tintaQueue.Count + "]");
                }
                else
                {
                    lezerQueue.Enqueue(doc);
                    SafeWrite(ConsoleColor.Cyan,
                        "BERAK (lezer) P" + producerId + " -> Doc#" + doc.Id + " " + doc.Name +
                        "   [lezer sor: " + lezerQueue.Count + "]");
                }

                Monitor.PulseAll(lockObj);
            }
        }

        public bool TryGetNext(string queueType, out Document doc)
        {
            doc = null;

            lock (lockObj)
            {
                while (true)
                {
                    if (queueType == "tinta")
                    {
                        if (tintaQueue.Count > 0)
                        {
                            doc = tintaQueue.Dequeue();
                            return true;
                        }
                    }
                    else
                    {
                        if (lezerQueue.Count > 0)
                        {
                            doc = lezerQueue.Dequeue();
                            return true;
                        }
                    }

                    if (remainingProducers == 0)
                    {
                        return false;
                    }

                    Monitor.Wait(lockObj);
                }
            }
        }

        public void ProducerFinished()
        {
            lock (lockObj)
            {
                remainingProducers--;
                if (remainingProducers == 0)
                {
                    SafeWrite(ConsoleColor.Gray, "Minden termelo kesz.");
                    Monitor.PulseAll(lockObj);
                }
            }
        }

        public void LogTake(Document doc, string printerName)
        {
            if (doc.QueueType == "tinta")
            {
                SafeWrite(ConsoleColor.DarkYellow,
                    "KIVESZ (tinta) " + printerName + " -> Doc#" + doc.Id + " " + doc.Name);
            }
            else
            {
                SafeWrite(ConsoleColor.Blue,
                    "KIVESZ (lezer) " + printerName + " -> Doc#" + doc.Id + " " + doc.Name);
            }
        }

        public void LogPrinted(Document doc, string printerName)
        {
            SafeWrite(ConsoleColor.Green,
                "NYOMTATVA      " + printerName + " -> Doc#" + doc.Id + " " + doc.Name);
        }

        public void LogEnd(string printerName)
        {
            SafeWrite(ConsoleColor.Gray, printerName + " vege.");
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
}
