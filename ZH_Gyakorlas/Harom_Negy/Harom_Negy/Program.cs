using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
Három és négy (ThreadPool, Task és dedikált szál)

Deklaráljon egy N elemű vektort, töltse fel a vektort (1-N közötti) véletlenszámokkal a Main-ben! 
(100-9999 közé eső számokkal.) 
Két szál segítségével számolja meg, hogy a vektorban hány hárommal osztható, 
de néggyel nem osztható, 4 jegyű szám van. Ehhez egy változót használjon, 
mindkét szál ezt a változót használja. Az egyik a vektor elejéről, 
a másik a vektor végétől kezdje el a keresést. Nem használhat lock-ot vagy Monitor-t! 
A végén a Main írja ki, hogy a megadott számokból mennyi volt a vektorban! 
A feladatot 3 eszközzel oldja meg: ThreadPool, Task és dedikált szálak. 
A StopWatch segítségével mérje meg, és írassa ki, hogy melyik eszközzel milyen gyorsak az implementált algoritmusok!
 */

namespace Harom_Negy
{
    internal class Program
    {
        static int[] vektor;
        static int count;
        static int mid;
        static CountdownEvent done;

        static int taskCount;
        static int threadCount;
        static int taskMid;
        static int threadMid;

        static bool HaromNegy4Jegyu(int szam)
        {
            if (szam < 1000 || szam > 9999) return false;
            if (szam % 3 != 0) return false;
            if (szam % 4 == 0) return false;
            return true;
        }

        // ---------- ThreadPool ----------
        static void ElsoFelTP(object _)
        {
            for (int i = 0; i < mid; i++)
                if (HaromNegy4Jegyu(vektor[i]))
                    Interlocked.Increment(ref count);
            done.Signal();
        }

        static void MasodikFelTP(object _)
        {
            for (int i = vektor.Length - 1; i >= mid; i--)
                if (HaromNegy4Jegyu(vektor[i]))
                    Interlocked.Increment(ref count);
            done.Signal();
        }

        static int ThreadPoolMegoldas(int[] v)
        {
            vektor = v;
            count = 0;
            mid = v.Length / 2;
            done = new CountdownEvent(2);

            ThreadPool.QueueUserWorkItem(ElsoFelTP);
            ThreadPool.QueueUserWorkItem(MasodikFelTP);

            done.Wait();
            return count;
        }

        // ---------- Task ----------
        static void ElsoFelTask()
        {
            for (int i = 0; i < taskMid; i++)
                if (HaromNegy4Jegyu(vektor[i]))
                    Interlocked.Increment(ref taskCount);
        }

        static void MasodikFelTask()
        {
            for (int i = vektor.Length - 1; i >= taskMid; i--)
                if (HaromNegy4Jegyu(vektor[i]))
                    Interlocked.Increment(ref taskCount);
        }

        static int TaskMegoldas(int[] v)
        {
            vektor = v;
            taskCount = 0;
            taskMid = v.Length / 2;

            Task t1 = Task.Factory.StartNew(ElsoFelTask);
            Task t2 = Task.Factory.StartNew(MasodikFelTask);

            Task.WaitAll(t1, t2);
            return taskCount;
        }

        // ---------- Dedikált szál ----------
        static void ElsoFelThread()
        {
            for (int i = 0; i < threadMid; i++)
                if (HaromNegy4Jegyu(vektor[i]))
                    Interlocked.Increment(ref threadCount);
        }

        static void MasodikFelThread()
        {
            for (int i = vektor.Length - 1; i >= threadMid; i--)
                if (HaromNegy4Jegyu(vektor[i]))
                    Interlocked.Increment(ref threadCount);
        }

        static int SzalasMegoldas(int[] v)
        {
            vektor = v;
            threadCount = 0;
            threadMid = v.Length / 2;

            Thread t1 = new Thread(ElsoFelThread);
            Thread t2 = new Thread(MasodikFelThread);

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            return threadCount;
        }

        // ---------- mérés ----------
        static long MeresMs(Func<int[], int> f, int[] v, out int eredmeny)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            eredmeny = f(v);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        static void Main()
        {
            int n = 2000000;
            vektor = new int[n];
            Random rnd = new Random();

            for (int i = 0; i < n; i++)
                vektor[i] = rnd.Next(100, 10000);

            int r1, r2, r3;
            long t1 = MeresMs(ThreadPoolMegoldas, vektor, out r1);
            long t2 = MeresMs(TaskMegoldas, vektor, out r2);
            long t3 = MeresMs(SzalasMegoldas, vektor, out r3);

            Console.WriteLine("Talalatok (mindnek egyeznie kell):");
            Console.WriteLine("ThreadPool: " + r1);
            Console.WriteLine("Task:       " + r2);
            Console.WriteLine("Szál:       " + r3);
            Console.WriteLine();

            Console.WriteLine("Idők (ms):");
            Console.WriteLine("ThreadPool: " + t1);
            Console.WriteLine("Task:       " + t2);
            Console.WriteLine("Szál:       " + t3);
        }
    }
}
