using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1gpt2
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    class Program
    {
        static List<int> numbersList = new List<int>();
        static int largest = int.MinValue;
        static int smallest = int.MaxValue;
        static int largestCount = 0;
        static int smallestCount = 0;

        static object lockObj = new object();

        // static int midPoint;  // T├ûR├ûLVE ΓÇô nem felez┼æpont kell

        // ├ÜJ: ├╢ssze├⌐r├⌐shez k├╢z├╢s indexek
        static int leftIndex;
        static int rightIndex;

        // ├ÜJ: sz├ílank├⌐nt feldolgozott elemsz├ím
        static int processedStart;
        static int processedEnd;

        static void Main(string[] args)
        {
            Random rand = new Random();
            for (int i = 0; i < 10000; i++)
            {
                numbersList.Add(rand.Next(1, 10001));
            }

            // midPoint = numbersList.Count / 2;  // T├ûR├ûLVE

            // ├ÜJ: indexek init
            leftIndex = 0;
            rightIndex = numbersList.Count - 1;

            // ├ÜJ: feldolgozott elemsz├ím null├íz├ís
            processedStart = 0;
            processedEnd = 0;

            Thread t1 = new Thread(SearchFromStart);
            Thread t2 = new Thread(SearchFromEnd);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Console.WriteLine("Largest number: " + largest + ", occurred " + largestCount + " times.");
            Console.WriteLine("Smallest number: " + smallest + ", occurred " + smallestCount + " times.");

            // ├ÜJ: ki├¡rjuk, mennyit dolgozott a k├⌐t sz├íl
            Console.WriteLine("Start thread processed: " + processedStart);
            Console.WriteLine("End thread processed:   " + processedEnd);
            Console.WriteLine("Total processed:        " + (processedStart + processedEnd));
        }

        static void SearchFromStart()
        {
            /*
            for (int i = 0; i < midPoint; i++)   // T├ûR├ûLVE
            {
                lock (lockObj)
                {
                    CheckNumber(numbersList[i]);
                }
            }
            */

            // ├ÜJ ΓÇô ├╢ssze├⌐r├⌐sig megy
            while (true)
            {
                int i = Interlocked.Increment(ref leftIndex) - 1; // ├ÜJ

                if (i > Volatile.Read(ref rightIndex))            // ├ÜJ
                    break;

                int value = numbersList[i];

                lock (lockObj)
                {
                    CheckNumber(value);
                }

                Interlocked.Increment(ref processedStart);        // ├ÜJ
            }
        }

        static void SearchFromEnd()
        {
            /*
            for (int i = numbersList.Count - 1; i >= midPoint; i--)  // T├ûR├ûLVE
            {
                lock (lockObj)
                {
                    CheckNumber(numbersList[i]);
                }
            }
            */

            // ├ÜJ ΓÇô ├╢ssze├⌐r├⌐sig megy
            while (true)
            {
                int i = Interlocked.Decrement(ref rightIndex) + 1; // ├ÜJ

                if (i < Volatile.Read(ref leftIndex))              // ├ÜJ
                    break;

                int value = numbersList[i];

                lock (lockObj)
                {
                    CheckNumber(value);
                }

                Interlocked.Increment(ref processedEnd);           // ├ÜJ
            }
        }

        static void CheckNumber(int number)
        {
            if (number > largest)
            {
                largest = number;
                largestCount = 1;
            }
            else if (number == largest)
            {
                largestCount++;
            }

            if (number < smallest)
            {
                smallest = number;
                smallestCount = 1;
            }
            else if (number == smallest)
            {
                smallestCount++;
            }
        }
    }
}
