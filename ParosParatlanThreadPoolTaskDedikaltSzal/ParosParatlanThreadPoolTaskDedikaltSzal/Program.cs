using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParosParatlanThreadPoolTaskDedikaltSzal
{
    internal class Program
    {
        static int[] vektor;

        static bool Paros(int x)
        {
            return (x % 2) == 0;
        }

        static long MeresMsThreadPool(int[] v, out int parosDb, out int paratlanDb)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ThreadPoolMegoldas(v, out parosDb, out paratlanDb);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        static long MeresMsTask(int[] v, out int parosDb, out int paratlanDb)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            TaskMegoldas(v, out parosDb, out paratlanDb);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        static long MeresMsThread(int[] v, out int parosDb, out int paratlanDb)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SzalasMegoldas(v, out parosDb, out paratlanDb);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        static int tpParos;
        static int tpParatlan;
        static int tpMid;
        static CountdownEvent tpDone;

        static void ElsoFelTP(object _)
        {
            int i;
            for (i = 0; i < tpMid; i++)
            {
                if (Paros(vektor[i])) Interlocked.Increment(ref tpParos);
                else Interlocked.Increment(ref tpParatlan);
            }
            tpDone.Signal();
        }

        static void MasodikFelTP(object _)
        {
            int i;
            for (i = tpMid; i < vektor.Length; i++)
            {
                if (Paros(vektor[i])) Interlocked.Increment(ref tpParos);
                else Interlocked.Increment(ref tpParatlan);
            }
            tpDone.Signal();
        }

        static void ThreadPoolMegoldas(int[] v, out int parosDb, out int paratlanDb)
        {
            vektor = v;

            tpParos = 0;
            tpParatlan = 0;
            tpMid = v.Length / 2;
            tpDone = new CountdownEvent(2);

            ThreadPool.QueueUserWorkItem(ElsoFelTP);
            ThreadPool.QueueUserWorkItem(MasodikFelTP);

            tpDone.Wait();

            parosDb = tpParos;
            paratlanDb = tpParatlan;
        }

        static int taskParos;
        static int taskParatlan;
        static int taskMid;

        static void ElsoFelTask()
        {
            int i;
            for (i = 0; i < taskMid; i++)
            {
                if (Paros(vektor[i])) Interlocked.Increment(ref taskParos);
                else Interlocked.Increment(ref taskParatlan);
            }
        }

        static void MasodikFelTask()
        {
            int i;
            for (i = taskMid; i < vektor.Length; i++)
            {
                if (Paros(vektor[i])) Interlocked.Increment(ref taskParos);
                else Interlocked.Increment(ref taskParatlan);
            }
        }

        static void TaskMegoldas(int[] v, out int parosDb, out int paratlanDb)
        {
            vektor = v;

            taskParos = 0;
            taskParatlan = 0;
            taskMid = v.Length / 2;

            Task t1 = Task.Run(new Action(ElsoFelTask));
            Task t2 = Task.Run(new Action(MasodikFelTask));

            Task.WaitAll(t1, t2);

            parosDb = taskParos;
            paratlanDb = taskParatlan;
        }

        static int threadParos;
        static int threadParatlan;
        static int threadMid;

        static void ElsoFelThread()
        {
            int i;
            for (i = 0; i < threadMid; i++)
            {
                if (Paros(vektor[i])) Interlocked.Increment(ref threadParos);
                else Interlocked.Increment(ref threadParatlan);
            }
        }

        static void MasodikFelThread()
        {
            int i;
            for (i = threadMid; i < vektor.Length; i++)
            {
                if (Paros(vektor[i])) Interlocked.Increment(ref threadParos);
                else Interlocked.Increment(ref threadParatlan);
            }
        }

        static void SzalasMegoldas(int[] v, out int parosDb, out int paratlanDb)
        {
            vektor = v;

            threadParos = 0;
            threadParatlan = 0;
            threadMid = v.Length / 2;

            Thread t1 = new Thread(new ThreadStart(ElsoFelThread));
            Thread t2 = new Thread(new ThreadStart(MasodikFelThread));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            parosDb = threadParos;
            paratlanDb = threadParatlan;
        }

        static void Main()
        {
            int n = 2000000;
            vektor = new int[n];

            Random rnd = new Random();
            int i;
            for (i = 0; i < n; i++)
            {
                vektor[i] = rnd.Next(1, n + 1);
            }

            int p1, q1, p2, q2, p3, q3;

            long t1 = MeresMsThreadPool(vektor, out p1, out q1);
            long t2 = MeresMsTask(vektor, out p2, out q2);
            long t3 = MeresMsThread(vektor, out p3, out q3);

            Console.WriteLine("Eredmenyek (mindnek egyeznie kell):");
            Console.WriteLine("ThreadPool: paros=" + p1 + " paratlan=" + q1);
            Console.WriteLine("Task:       paros=" + p2 + " paratlan=" + q2);
            Console.WriteLine("Szal:       paros=" + p3 + " paratlan=" + q3);
            Console.WriteLine();

            Console.WriteLine("Idok (ms):");
            Console.WriteLine("ThreadPool: " + t1);
            Console.WriteLine("Task:       " + t2);
            Console.WriteLine("Szal:       " + t3);
        }
    }
}
