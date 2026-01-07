using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParosParatlan
{
    internal class Program
    {
        static int[] vektor;

        static bool NeggyelOszthatoDeHarommalNemEs4Jegyu(int szam)
        {
            if (szam < 1000 || szam > 9999) return false;
            if (szam % 4 != 0) return false;
            if (szam % 3 == 0) return false;
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

        static long MeresMsTask(int[] v, out int eredmeny)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            eredmeny = TaskMegoldas(v);
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

        static int ThreadPoolCount;
        static int ThreadPoolMid;
        static CountdownEvent ThreadPoolDone;

        static void ElsoFelTP(object _)
        {
            int i;
            for (i = 0; i < ThreadPoolMid; i++)
            {
                if (NeggyelOszthatoDeHarommalNemEs4Jegyu(vektor[i]))
                {
                    Interlocked.Increment(ref ThreadPoolCount);
                }
            }
            ThreadPoolDone.Signal();
        }

        static void MasodikFelTP(object _)
        {
            int i;
            for (i = vektor.Length - 1; i >= ThreadPoolMid; i--)
            {
                if (NeggyelOszthatoDeHarommalNemEs4Jegyu(vektor[i]))
                {
                    Interlocked.Increment(ref ThreadPoolCount);
                }
            }
            ThreadPoolDone.Signal();
        }

        static int ThreadPoolMegoldas(int[] v)
        {
            vektor = v;
            ThreadPoolCount = 0;
            ThreadPoolMid = v.Length / 2;
            ThreadPoolDone = new CountdownEvent(2);

            ThreadPool.QueueUserWorkItem(ElsoFelTP);
            ThreadPool.QueueUserWorkItem(MasodikFelTP);

            ThreadPoolDone.Wait();
            return ThreadPoolCount;
        }

        static int TaskCount;
        static int TaskMid;

        static void ElsoFelTask()
        {
            int i;
            for (i = 0; i < TaskMid; i++)
            {
                if (NeggyelOszthatoDeHarommalNemEs4Jegyu(vektor[i]))
                {
                    Interlocked.Increment(ref TaskCount);
                }
            }
        }

        static void MasodikFelTask()
        {
            int i;
            for (i = vektor.Length - 1; i >= TaskMid; i--)
            {
                if (NeggyelOszthatoDeHarommalNemEs4Jegyu(vektor[i]))
                {
                    Interlocked.Increment(ref TaskCount);
                }
            }
        }

        static int TaskMegoldas(int[] v)
        {
            vektor = v;
            TaskCount = 0;
            TaskMid = v.Length / 2;

            Task t1 = Task.Run(new Action(ElsoFelTask));
            Task t2 = Task.Run(new Action(MasodikFelTask));

            Task.WaitAll(t1, t2);
            return TaskCount;
        }

        static int ParallelCount;

        static int ParallelMegoldas(int[] v)
        {
            vektor = v;
            ParallelCount = 0;

            Parallel.For(0, v.Length,
                delegate (int i)
                {
                    if (NeggyelOszthatoDeHarommalNemEs4Jegyu(v[i]))
                    {
                        Interlocked.Increment(ref ParallelCount);
                    }
                });

            return ParallelCount;
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
            long t2 = MeresMsTask(vektor, out r2);
            long t3 = MeresMsParallel(vektor, out r3);

            Console.WriteLine("ThreadPool:  " + r1);
            Console.WriteLine("Task:        " + r2);
            Console.WriteLine("Parallel.for:" + r3);
            Console.WriteLine();

            Console.WriteLine("Idok (ms):");
            Console.WriteLine("ThreadPool:  " + t1);
            Console.WriteLine("Task:        " + t2);
            Console.WriteLine("Parallel.for:" + t3);
        }
    }
}
