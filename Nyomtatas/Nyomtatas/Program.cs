using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nyomtatas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("Termelok (kliensek) szama: ");
            string s = Console.ReadLine();
            int producerCount = 0;

            if (!int.TryParse(s, out producerCount) || producerCount < 1)
            {
                producerCount = 3;
            }

            Supervisor supervisor = new Supervisor(producerCount);
            supervisor.Start();

            Console.ReadLine();
        }
    }

    public class Document
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

    public class Producer
    {
        public int ProducerId;
        public int Amount;

        private Supervisor supervisor;
        private Random random;

        public Producer(int producerId, int amount, Supervisor sup)
        {
            ProducerId = producerId;
            Amount = amount;
            supervisor = sup;
            random = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Run()
        {
            int i;
            for (i = 0; i < Amount; i++)
            {
                string name = GenerateName();
                string queueType = random.Next(0, 2) == 0 ? "tinta" : "lezer";

                int docId = supervisor.NextDocumentId();
                Document doc = new Document(docId, name, queueType);

                supervisor.Submit(doc, ProducerId);

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

    public class Printer
    {
        public int PrinterId;
        public string Type;
        public string QueueType;
        public int WorkTimeMs;

        private Supervisor supervisor;

        public Printer(int printerId, string type, string queueType, int workTimeMs, Supervisor sup)
        {
            PrinterId = printerId;
            Type = type;
            QueueType = queueType;
            WorkTimeMs = workTimeMs;
            supervisor = sup;
        }

        public void Run()
        {
            while (true)
            {
                Document doc;
                bool ok = supervisor.TryGetNext(QueueType, out doc);
                if (!ok)
                {
                    break;
                }

                supervisor.PrintTaken(doc, PrinterId, Type);

                Thread.Sleep(WorkTimeMs);

                supervisor.PrintDone(doc, PrinterId, Type);
            }

            supervisor.PrinterFinished(PrinterId, Type);
        }
    }

    public class Supervisor
    {
        private BlockingCollection<Document> tintaQueue;
        private BlockingCollection<Document> lezerQueue;

        private List<Producer> producers;
        private List<Printer> printers;

        private int docIdCounter;
        private int remainingProducers;
        private int remainingPrinters;

        public Supervisor(int producerCount)
        {
            tintaQueue = new BlockingCollection<Document>(30);
            lezerQueue = new BlockingCollection<Document>(30);

            producers = new List<Producer>();
            printers = new List<Printer>();

            docIdCounter = 0;
            remainingProducers = producerCount;
            remainingPrinters = 4;

            int i;
            for (i = 1; i <= producerCount; i++)
            {
                int amount = 5 + i * 2;
                Producer p = new Producer(i, amount, this);
                producers.Add(p);
            }

            Printer pr1 = new Printer(1, "Tintasugaras-1", "tinta", 160, this);
            Printer pr2 = new Printer(2, "Tintasugaras-2", "tinta", 160, this);
            Printer pr3 = new Printer(3, "Lezer-1", "lezer", 80, this);
            Printer pr4 = new Printer(4, "Lezer-2", "lezer", 80, this);

            printers.Add(pr1);
            printers.Add(pr2);
            printers.Add(pr3);
            printers.Add(pr4);
        }

        public void Start()
        {
            int i;

            for (i = 0; i < printers.Count; i++)
            {
                Printer pr = printers[i];
                ThreadPool.QueueUserWorkItem(new WaitCallback(PrinterWork), pr);
            }

            for (i = 0; i < producers.Count; i++)
            {
                Producer p = producers[i];
                ThreadPool.QueueUserWorkItem(new WaitCallback(ProducerWork), p);
            }

            SafeWriteLine(ConsoleColor.Gray, "Indult. Enter a kilepeshez...");
        }

        private void ProducerWork(object state)
        {
            Producer p = (Producer)state;
            p.Run();
        }

        private void PrinterWork(object state)
        {
            Printer pr = (Printer)state;
            pr.Run();
        }

        public int NextDocumentId()
        {
            return Interlocked.Increment(ref docIdCounter);
        }

        public void Submit(Document doc, int producerId)
        {
            if (doc.QueueType == "tinta")
            {
                tintaQueue.Add(doc);
                SafeWriteLine(ConsoleColor.Yellow,
                    "BERAK (tinta)  P" + producerId + " -> Doc#" + doc.Id + " " + doc.Name +
                    "   [tinta sor: " + tintaQueue.Count + "]");
            }
            else
            {
                lezerQueue.Add(doc);
                SafeWriteLine(ConsoleColor.Cyan,
                    "BERAK (lezer)  P" + producerId + " -> Doc#" + doc.Id + " " + doc.Name +
                    "   [lezer sor: " + lezerQueue.Count + "]");
            }
        }

        public bool TryGetNext(string queueType, out Document doc)
        {
            doc = null;

            try
            {
                if (queueType == "tinta")
                {
                    doc = tintaQueue.Take();
                    return true;
                }
                doc = lezerQueue.Take();
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public void ProducerFinished()
        {
            bool closeNow = false;

            lock (this)
            {
                remainingProducers--;
                if (remainingProducers == 0)
                {
                    closeNow = true;
                }
            }

            if (closeNow)
            {
                tintaQueue.CompleteAdding();
                lezerQueue.CompleteAdding();
                SafeWriteLine(ConsoleColor.Gray, "Minden termelo kesz. Sorok lezarva.");
            }
        }

        public void PrintTaken(Document doc, int printerId, string printerName)
        {
            if (doc.QueueType == "tinta")
            {
                SafeWriteLine(ConsoleColor.DarkYellow,
                    "KIVESZ (tinta)  " + printerName + " -> Doc#" + doc.Id + " " + doc.Name +
                    "   [tinta sor: " + tintaQueue.Count + "]");
            }
            else
            {
                SafeWriteLine(ConsoleColor.Blue,
                    "KIVESZ (lezer)  " + printerName + " -> Doc#" + doc.Id + " " + doc.Name +
                    "   [lezer sor: " + lezerQueue.Count + "]");
            }
        }

        public void PrintDone(Document doc, int printerId, string printerName)
        {
            SafeWriteLine(ConsoleColor.Green,
                "NYOMTATVA       " + printerName + " -> Doc#" + doc.Id + " " + doc.Name);
        }

        public void PrinterFinished(int printerId, string printerName)
        {
            bool allDone = false;

            lock (this)
            {
                remainingPrinters--;
                if (remainingPrinters == 0)
                {
                    allDone = true;
                }
            }

            SafeWriteLine(ConsoleColor.Gray, printerName + " vege.");

            if (allDone)
            {
                SafeWriteLine(ConsoleColor.Gray, "Kesz. Enter a kilepeshez...");
            }
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
}
