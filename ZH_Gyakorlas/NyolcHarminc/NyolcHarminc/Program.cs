using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
/*
 
Nyolc és harminc (ThreadPool, Task és dedikált szál) – 20 pont

Deklaráljon egy N elemű vektort, töltse fel a vektort (1-N közötti) véletlenszámokkal a Main-ben! 
(100-999999 közé eső számokkal.) Két szál segítségével számolja meg, hogy a vektorban hány nyolccal osztható, 
de harminccal nem osztható, 4 jegyű szám van. Ehhez egy változót használjon, mindkét szál ezt a változót használja. 
Az egyik a vektor elejéről, a másik a vektor végétől kezdje el a keresést. Nem használhat lock-ot vagy Monitor-t! 
A végén a Main írja ki, hogy a megadott számokból mennyi volt a vektorban! A feladatot 3 eszközzel oldja meg: 
ThreadPool, Task és dedikált szálak. 
A StopWatch segítségével mérje meg, és írassa ki, hogy melyik eszközzel milyen gyorsak az implementált algoritmusok!
 
 */
namespace NyolcHarminc
{

    internal class Program
    {
        static int[] vektor;
        static int count;
        static int mid;
        static CountdownEvent done;

        static int count2;
        static int mid2;

        static int count3;
        static int mid3;



        static bool NyolcHarminc4jegyu(int szam)
        {
            if (szam < 1000 || szam > 9999) return false;
            if (szam % 8 != 0) return false;
            if (szam % 30 == 0) return false;
            return true;
        }

        // ThreadPool
        static void ElsoThreadPool(object id)
        {
            for (int i = 0; i < mid; i++)
                if (NyolcHarminc4jegyu(vektor[i]))
                    Interlocked.Increment(ref count);
            done.Signal();
        }
        static void MasodikThreadPool(object id)
        {
            for (int i = vektor.Length - 1; i >= mid; i--)
                if (NyolcHarminc4jegyu(vektor[i]))
                    Interlocked.Increment(ref count);
            done.Signal();
        }
        static int ThreadPoolMegoldas(int[] v)
        {
            vektor = v;
            count = 0;
            mid = v.Length / 2;
            done = new CountdownEvent(2);

            ThreadPool.QueueUserWorkItem(ElsoThreadPool);
            ThreadPool.QueueUserWorkItem(MasodikThreadPool);

            done.Wait();
            return count;
        }

        // Task

        static void ElsoTask()
        {
            for (int i = 0; i < mid2; i++)
                if (NyolcHarminc4jegyu(vektor[i]))
                    Interlocked.Increment(ref count2);
        }
        static void MasodikTask()
        {
            for (int i = vektor.Length - 1; i >= mid2; i--)
                if (NyolcHarminc4jegyu(vektor[i]))
                    Interlocked.Increment(ref count2);
        }
        static int TaskMegoldas(int[] v)
        {
            vektor = v;
            count2 = 0;
            mid2 = v.Length / 2;

            Task t1 = Task.Factory.StartNew(ElsoTask);
            Task t2 = Task.Factory.StartNew(MasodikTask);

            Task.WaitAll(t1,t2);

            return count2;
        }

        // Dedikalt szal
        static void ElsoDSZ()
        {
            for (int i = 0; i < mid3; i++)
                if (NyolcHarminc4jegyu(vektor[i]))
                    Interlocked.Increment(ref count3);
        }
        static void MasodikDSZ()
        {
            for (int i = vektor.Length - 1; i >= mid3; i--)
                if (NyolcHarminc4jegyu(vektor[i]))
                    Interlocked.Increment(ref count3);
        }
        static int DSZMegoldas(int[] v)
        {
            vektor = v;
            count3 = 0;
            mid3 = v.Length / 2;

            Thread t1 = new Thread(ElsoDSZ);
            Thread t2 = new Thread(MasodikDSZ);

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            return count3;
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
            int[] vektor = new int[n];
            Random rnd = new Random();
            for (int i = 0; i < n; i++)
                vektor[i] = rnd.Next(100,1000000);
            int r1, r2, r3;
            long t1 = MeresMS(ThreadPoolMegoldas, vektor, out r1);
            long t2 = MeresMS(TaskMegoldas, vektor, out r2);
            long t3 = MeresMS(DSZMegoldas, vektor, out r3);

            Console.WriteLine("ThreadPool " + r1);
            Console.WriteLine("Task       " + r2);
            Console.WriteLine("Szal       " + r3);
            Console.WriteLine();

            Console.WriteLine("Idok");
            Console.WriteLine("ThreadPool " + t1);
            Console.WriteLine("Task       " + t2);
            Console.WriteLine("Szal       " + t3);
        }
    }
}
