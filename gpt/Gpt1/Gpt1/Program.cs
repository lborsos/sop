using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gpt1
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    class Program
    {
        static List<int> numbersList = new List<int>();
        static int foundCount = 0;
        static object lockObj = new object();

        // ÚJ: közös indexek
        static int leftIndex;
        static int rightIndex;

        static void Main(string[] args)
        {
            Random rand = new Random();

            for (int i = 0; i < 100; i++)
                numbersList.Add(rand.Next(1000, 10000));

            Console.WriteLine("Generated Numbers:");

            // Lambda törölve – sima for
            for (int i = 0; i < numbersList.Count; i++)
                Console.Write(numbersList[i] + " ");
            Console.WriteLine("\n");

            // ÚJ: indexek inicializálása
            leftIndex = 0;
            rightIndex = numbersList.Count - 1;

            Thread t1 = new Thread(SearchFromStart);
            Thread t2 = new Thread(SearchFromEnd);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nTotal even numbers found: " + foundCount);
            Console.ResetColor();
        }

        static void SearchFromStart()
        {
            /*
            Régi verzió – for ciklus, megszakítással
            for (int i = 0; i < numbersList.Count; i++)
            {
                if (matchFound) break;

                if (numbersList[i] % 2 == 0)
                {
                    lock (lockObj)
                    {
                        if (matchFound) break;
                        foundCount++;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Thread 1 (Start): Found even number " + numbersList[i] + " at index " + i);
                        Thread.Sleep(100);

                        if (foundCount >= 2) matchFound = true;
                    }
                }
            }
            */

            // ÚJ – összeférésig megy
            while (true)
            {
                int i = Interlocked.Increment(ref leftIndex) - 1;

                if (i > Volatile.Read(ref rightIndex))
                    break;

                if (numbersList[i] % 2 == 0)
                {
                    lock (lockObj)
                    {
                        foundCount++;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Thread 1 (Start): Found even number " + numbersList[i] + " at index " + i);
                        Console.ResetColor();
                    }

                    Thread.Sleep(100);
                }
            }
        }

        static void SearchFromEnd()
        {
            /*
            Régi verzió – for ciklus, megszakítással
            for (int i = numbersList.Count - 1; i >= 0; i--)
            {
                if (matchFound) break;

                if (numbersList[i] % 2 == 0)
                {
                    lock (lockObj)
                    {
                        if (matchFound) break;
                        foundCount++;
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Thread 2 (End): Found even number " + numbersList[i] + " at index " + i);
                        Thread.Sleep(100);

                        if (foundCount >= 2) matchFound = true;
                    }
                }
            }
            */

            // ÚJ – összeférésig megy
            while (true)
            {
                int i = Interlocked.Decrement(ref rightIndex) + 1;

                if (i < Volatile.Read(ref leftIndex))
                    break;

                if (numbersList[i] % 2 == 0)
                {
                    lock (lockObj)
                    {
                        foundCount++;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Thread 2 (End): Found even number " + numbersList[i] + " at index " + i);
                        Console.ResetColor();
                    }

                    Thread.Sleep(100);
                }
            }
        }
    }
}
