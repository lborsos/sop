using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gpt4
{
    using System;
    using System.Collections.Generic;
    // using System.Linq;   // T├ûR├ûLVE ΓÇô LINQ nem kell
    using System.Threading;
    // ThreadPool T├ûR├ûLVE ΓÇô feladat szerint nem haszn├ílhat├│
    using System.Threading.Tasks;   // ├ÜJ ΓÇô Task haszn├ílata

    class Program
    {
        static List<int> numbersList = new List<int>();
        // static bool stopRemoveThread = false;   // T├ûR├ûLVE ΓÇô rossz logika
        static object lockObj = new object();

        static bool producerFinished = false;   // ├ÜJ ΓÇô jelzi, hogy az els┼æ sz├íl v├⌐gzett

        static void Main(string[] args)
        {
            // ThreadPool.QueueUserWorkItem(AddNumbers);   // T├ûR├ûLVE
            // ThreadPool.QueueUserWorkItem(RemoveNumbers); // T├ûR├ûLVE

            // ├ÜJ ΓÇô Task ind├¡t├ís
            Task t1 = Task.Run(new Action(AddNumbers));
            Task t2 = Task.Run(new Action(RemoveNumbers));

            // Thread.Sleep(5000);   // T├ûR├ûLVE ΓÇô nem korrekt v├írakoz├ís

            // ├ÜJ ΓÇô megv├írjuk a sz├ílakat
            Task.WaitAll(t1, t2);

            int singleDigitCount = 0;   // ├ÜJ
            int totalCount = 0;         // ├ÜJ

            lock (lockObj)
            {
                totalCount = numbersList.Count;

                int i;
                for (i = 0; i < numbersList.Count; i++)   // ├ÜJ ΓÇô LINQ helyett
                {
                    int x = numbersList[i];
                    if (x >= 0 && x < 10)
                    {
                        singleDigitCount++;
                    }
                }

                // int singleDigitCount = numbersList.Count(num => num >= 0 && num < 10);  // T├ûR├ûLVE ΓÇô LINQ
            }

            Console.WriteLine("Total numbers left in the list: " + totalCount);
            Console.WriteLine("Total single-digit numbers left: " + singleDigitCount);
        }

        static void AddNumbers()
        {
            Random rand = new Random();
            int i;

            for (i = 0; i < 100000; i++)
            {
                int x = rand.Next(1, 10000);   // 1..9999, max 4 jegy

                lock (lockObj)
                {
                    numbersList.Add(x);
                }
            }

            // ├ÜJ ΓÇô jelezz├╝k, hogy k├⌐sz a termel├⌐s
            lock (lockObj)
            {
                producerFinished = true;
            }
        }

        static void RemoveNumbers()
        {
            /*
            while (!stopRemoveThread)   // T├ûR├ûLVE
            {
                lock (lockObj)
                {
                    bool found = false;
                    for (int i = 0; i < numbersList.Count; i++)
                    {
                        if (numbersList[i] >= 100)
                        {
                            numbersList[i] = -1;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        stopRemoveThread = true;
                    }
                }

                Thread.Sleep(10);
            }
            */

            // ├ÜJ ΓÇô specifik├íci├│ szerint
            while (true)
            {
                bool replaced = false;
                bool finished;

                lock (lockObj)
                {
                    int i;
                    for (i = 0; i < numbersList.Count; i++)
                    {
                        if (numbersList[i] >= 100)   // legal├íbb 3 jegy┼▒
                        {
                            numbersList[i] = -1;
                            replaced = true;
                            break;
                        }
                    }

                    finished = producerFinished;
                }

                if (!replaced)
                {
                    if (finished)
                    {
                        break;   // ha nincs t├╢bb 3 jegy┼▒ ├⌐s a termel┼æ k├⌐sz ΓåÆ le├íll
                    }

                    Thread.Sleep(1);   // ├ÜJ ΓÇô kis v├írakoz├ís
                }
            }
        }
    }
}
