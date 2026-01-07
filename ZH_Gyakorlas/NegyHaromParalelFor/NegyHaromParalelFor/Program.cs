using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 
Páros – páratlan (ThreadPool, Task és Parallel.for) – 20 pont

Deklaráljon egy N elemű vektort, töltse fel a vektort (1–N közötti) véletlenszámokkal a Main-ben!
(100–9999 közé eső számokkal.)

Két szál segítségével számolja meg, hogy a vektorban hány néggyel osztható, 
de hárommal nem osztható, 4 jegyű szám van.
Ehhez egy változót használjon, mindkét szál ezt a változót használja.
Az egyik a vektor elejéről, a másik a vektor végétől kezdje el a keresést.
Nem használhat lock-ot vagy Monitor-t!

A végén a Main írja ki, hogy a megadott számokból mennyi volt a vektorban!

A feladatot 3 eszközzel oldja meg: ThreadPool, Task és Parallel.for.
A StopWatch segítségével mérje meg, és írassa ki, hogy melyik eszközzel milyen gyorsak az 
implementált algoritmusok!

 */
namespace NegyHaromParalelFor
{
    internal class Program
    {
        static int[] vektor;

        static int countTP;
        static int midTP;
        static CountdownEvent doneTP;

        static int countTask;
        static int midTask;

        static int countPar;

        static bool NeggyelOszthatoDeHarommalNem_4Jegyu(int szam)
        {
            if (szam < 1000 || szam > 9999) return false;
            if (szam % 4 != 0) return false;
            if (szam % 3 == 0) return false;
            return true;
        }

        // ThreadPool
        static void ElsoTP(object _)
        {
            for (int i = 0; i < midTP; i++)
                if (NeggyelOszthatoDeHarommalNem_4Jegyu(vektor[i]))
                    Interlocked.Increment(ref countTP);

            doneTP.Signal();
        }

        static void MasodikTP(object _)
        {
            for (int i = vektor.Length - 1; i >= midTP; i--)
                if (NeggyelOszthatoDeHarommalNem_4Jegyu(vektor[i]))
                    Interlocked.Increment(ref countTP);

            doneTP.Signal();
        }

        static int ThreadPoolMegoldas(int[] v)
        {
            vektor = v;
            countTP = 0;
            midTP = v.Length / 2;
            doneTP = new CountdownEvent(2);

            ThreadPool.QueueUserWorkItem(ElsoTP);
            ThreadPool.QueueUserWorkItem(MasodikTP);

            doneTP.Wait();
            return countTP;
        }

        // Task
        static void ElsoTask()
        {
            for (int i = 0; i < midTask; i++)
                if (NeggyelOszthatoDeHarommalNem_4Jegyu(vektor[i]))
                    Interlocked.Increment(ref countTask);
        }

        static void MasodikTask()
        {
            for (int i = vektor.Length - 1; i >= midTask; i--)
                if (NeggyelOszthatoDeHarommalNem_4Jegyu(vektor[i]))
                    Interlocked.Increment(ref countTask);
        }

        static int TaskMegoldas(int[] v)
        {
            vektor = v;
            countTask = 0;
            midTask = v.Length / 2;

            Task t1 = Task.Factory.StartNew(ElsoTask);
            Task t2 = Task.Factory.StartNew(MasodikTask);

            Task.WaitAll(t1, t2);
            return countTask;
        }

        //Parallel.For
        static void ParallelBody(int i)
        {
            if (NeggyelOszthatoDeHarommalNem_4Jegyu(vektor[i]))
                Interlocked.Increment(ref countPar);
        }

        static int ParallelForMegoldas(int[] v)
        {
            vektor = v;
            countPar = 0;

            ParallelOptions opt = new ParallelOptions();
            opt.MaxDegreeOfParallelism = 2;

            Parallel.For(0, v.Length, opt, ParallelBody);
            return countPar;
        }

        static long MeresMS(Func<int[], int> f, int[] v, out int eredmeny)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            eredmeny = f(v);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        static void Main(string[] args)
        {
            int n = 200000;
            int[] v = new int[n];
            Random rnd = new Random();

            for (int i = 0; i < n; i++)
                v[i] = rnd.Next(100, 10000);

            int r1, r2, r3;

            long t1 = MeresMS(ThreadPoolMegoldas, v, out r1);
            long t2 = MeresMS(TaskMegoldas, v, out r2);
            long t3 = MeresMS(ParallelForMegoldas, v, out r3);

            Console.WriteLine("Talalatok (egyezni kell):");
            Console.WriteLine("ThreadPool   " + r1);
            Console.WriteLine("Task         " + r2);
            Console.WriteLine("Parallel.For " + r3);
            Console.WriteLine();

            Console.WriteLine("Idok (ms):");
            Console.WriteLine("ThreadPool   " + t1);`
            Console.WriteLine("Task         " + t2);
            Console.WriteLine("Parallel.For " + t3);
        }
    }
}
