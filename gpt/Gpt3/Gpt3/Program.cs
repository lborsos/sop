using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gpt3
{
    using System;
    using System.Collections.Generic;
    // using System.Linq;  // T├ûR├ûLVE ΓÇô LINQ nem kell
    using System.Threading;

    class Program
    {
        static List<int> numbersList = new List<int>();
        // static bool stopRemoveThread = false;  // T├ûR├ûLVE ΓÇô rossz vez├⌐rl├⌐s
        static object lockObj = new object();

        static bool producerFinished = false;  // ├ÜJ ΓÇô termel┼æ k├⌐sz jelz├⌐s

        static void Main(string[] args)
        {
            Thread addThread = new Thread(AddNumbers);
            Thread removeThread = new Thread(RemoveNumbers);

            addThread.Priority = ThreadPriority.Highest;
            removeThread.Priority = ThreadPriority.Lowest;

            addThread.Start();
            removeThread.Start();

            addThread.Join();
            removeThread.Join();

            // int singleDigitCount = numbersList.Count(num => num >= 0 && num < 10); // T├ûR├ûLVE ΓÇô LINQ

            int singleDigitCount = 0;  // ├ÜJ
            int totalCount = 0;        // ├ÜJ

            lock (lockObj)
            {
                totalCount = numbersList.Count;

                int i;
                for (i = 0; i < numbersList.Count; i++)  // ├ÜJ ΓÇô LINQ helyett
                {
                    int x = numbersList[i];
                    if (x >= 0 && x < 10)
                    {
                        singleDigitCount++;
                    }
                }
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
                int x = rand.Next(1, 10000);

                lock (lockObj)
                {
                    numbersList.Add(x);
                }
            }

            lock (lockObj)              // ├ÜJ
            {
                producerFinished = true; // ├ÜJ
            }
        }

        static void RemoveNumbers()
        {
            /*
            while (!stopRemoveThread)   // T├ûR├ûLVE
            {
                lock (lockObj)
                {
                    for (int i = 0; i < numbersList.Count; i++)
                    {
                        if (numbersList[i] >= 100)
                        {
                            numbersList[i] = -1;
                            Console.WriteLine($"Replaced element at index {i} with -1.");
                            break;
                        }
                    }

                    if (!numbersList.Any(num => num >= 100))
                    {
                        stopRemoveThread = true;
                    }
                }
                Thread.Sleep(10);
            }
            */

            // ├ÜJ ΓÇô specifik├íci├│ szerint: ha NEM tal├íl legal├íbb 3 jegy┼▒t, akkor azonnal le├íll
            while (true)
            {
                bool replaced = false;
                bool finished;

                lock (lockObj)
                {
                    int i;
                    for (i = 0; i < numbersList.Count; i++)
                    {
                        if (numbersList[i] >= 100)
                        {
                            numbersList[i] = -1;
                            replaced = true;
                            break;
                        }
                    }

                    finished = producerFinished; // ├ÜJ
                }

                if (!replaced)
                {
                    if (finished)
                    {
                        break; // ├ÜJ ΓÇô nincs 3 jegy┼▒ ├⌐s termel┼æ k├⌐sz -> azonnal le├íll
                    }

                    Thread.Sleep(1); // ├ÜJ ΓÇô r├╢vid v├írakoz├ís, am├¡g j├╢nnek ├║j elemek
                }
            }
        }
    }
}
