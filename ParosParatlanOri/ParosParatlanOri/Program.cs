using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParosParatlanOri
{
    internal class Program
    {
        static int[] vektor;

        static bool JoSzam(int x)
        {
            if (x < 1000 || x > 9999) return false;
            if (x % 8 != 0) return false;
            if (x % 12 == 0) return false;
            return true;
        }

        static long MeresMsThreadPool(int[] v, out int eredmeny)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            eredmeny = ThreadPoolMegoldas(v);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        static long MeresMsParallel(int[] v, out int eredmeny)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            eredmeny = ParallelMegoldas(v);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        static long MeresMsThread(int[] v, out int eredmeny)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            eredmeny = SzalasMegoldas(v);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        static int tpCount;
        static int tpMid;
        static CountdownEvent tpDone;

        static void ElsoFelTP(object _)
        {
            int i;
            for (i = 0; i < tpMid; i++)
            {
                if (JoSzam(vektor[i]))
                {
                    Interlocked.Increment(ref tpCount);
                }
            }
            tpDone.Signal();
        }

        static void MasodikFelTP(object _)
        {
            int i;
            for (i = vektor.Length - 1; i >= tpMid; i--)
            {
                if (JoSzam(vektor[i]))
                {
                    Interlocked.Increment(ref tpCount);
                }
            }
            tpDone.Signal();
        }

        static int ThreadPoolMegoldas(int[] v)
        {
            vektor = v;
            tpCount = 0;
            tpMid = v.Length / 2;
            tpDone = new CountdownEvent(2);

            ThreadPool.QueueUserWorkItem(ElsoFelTP);
            ThreadPool.QueueUserWorkItem(MasodikFelTP);

            tpDone.Wait();
            return tpCount;
        }

        static int parallelCount;

        static int ParallelMegoldas(int[] v)
        {
            vektor = v;
            parallelCount = 0;

            Parallel.For(0, v.Length,
                delegate (int i)
                {
                    if (JoSzam(v[i]))
                    {
                        Interlocked.Increment(ref parallelCount);
                    }
                });

            return parallelCount;
        }

        static int threadCount;
        static int threadMid;

        static void ElsoFelThread()
        {
            int i;
            for (i = 0; i < threadMid; i++)
            {
                if (JoSzam(vektor[i]))
                {
                    Interlocked.Increment(ref threadCount);
                }
            }
        }

        static void MasodikFelThread()
        {
            int i;
            for (i = vektor.Length - 1; i >= threadMid; i--)
            {
                if (JoSzam(vektor[i]))
                {
                    Interlocked.Increment(ref threadCount);
                }
            }
        }

        static int SzalasMegoldas(int[] v)
        {
            vektor = v;
            threadCount = 0;
            threadMid = v.Length / 2;

            Thread t1 = new Thread(new ThreadStart(ElsoFelThread));
            Thread t2 = new Thread(new ThreadStart(MasodikFelThread));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            return threadCount;
        }

        static void Main()
        {
            int n = 2000000;
            vektor = new int[n];

            Random rnd = new Random();
            int i;
            for (i = 0; i < n; i++)
            {
                vektor[i] = rnd.Next(100, 10000);
            }

            int r1, r2, r3;
            long t1 = MeresMsThreadPool(vektor, out r1);
            long t2 = MeresMsParallel(vektor, out r2);
            long t3 = MeresMsThread(vektor, out r3);

            Console.WriteLine("Talalatok (mindnek egyeznie kell):");
            Console.WriteLine("ThreadPool:  " + r1);
            Console.WriteLine("Parallel.for:" + r2);
            Console.WriteLine("Szal:        " + r3);
            Console.WriteLine();

            Console.WriteLine("Idok (ms):");
            Console.WriteLine("ThreadPool:  " + t1);
            Console.WriteLine("Parallel.for:" + t2);
            Console.WriteLine("Szal:        " + t3);
        }
    }
}
