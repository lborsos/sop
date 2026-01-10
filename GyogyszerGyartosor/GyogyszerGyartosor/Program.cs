using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GyogyszerGyartosor
{
    enum GyogyszerTipus
    {
        Fajdalomcsillapito,
        Vernyomascsokkento,
        Allergia
    }

    class Gyogyszer
    {
        public int Id;
        public GyogyszerTipus Tipus;
        public int Minoseg;

        public Gyogyszer(int id, GyogyszerTipus tipus, int minoseg)
        {
            Id = id;
            Tipus = tipus;
            Minoseg = minoseg;
        }
    }

    static class Felugyelo
    {
        static readonly object locker = new object();
        static readonly object consoleLock = new object();

        static Dictionary<GyogyszerTipus, Queue<Gyogyszer>> tarolo = new Dictionary<GyogyszerTipus, Queue<Gyogyszer>>();

        static int maxBuffer = 95;
        static int aktualisDb = 0;

        static int termelokFutnak = 0;
        static bool termelesVege = false;

        static int globalId = 0;
        static Random rnd = new Random();

        public static void Init(int bufferMax)
        {
            maxBuffer = bufferMax;
            tarolo[GyogyszerTipus.Fajdalomcsillapito] = new Queue<Gyogyszer>();
            tarolo[GyogyszerTipus.Vernyomascsokkento] = new Queue<Gyogyszer>();
            tarolo[GyogyszerTipus.Allergia] = new Queue<Gyogyszer>();
        }

        public static int UjId()
        {
            lock (locker)
            {
                globalId++;
                return globalId;
            }
        }

        public static int RandomKozott(int a, int b)
        {
            lock (locker)
            {
                return rnd.Next(a, b + 1);
            }
        }

        public static void TermeloStart()
        {
            lock (locker)
            {
                termelokFutnak++;
            }
        }

        public static void TermeloStop()
        {
            lock (locker)
            {
                termelokFutnak--;
                if (termelokFutnak <= 0)
                {
                    termelesVege = true;
                    Monitor.PulseAll(locker);
                }
            }
        }

        public static void Betesz(Gyogyszer gy)
        {
            lock (locker)
            {
                while (aktualisDb >= maxBuffer)
                    Monitor.Wait(locker);

                tarolo[gy.Tipus].Enqueue(gy);
                aktualisDb++;

                Print("BETESZ: " + TipusSzoveg(gy.Tipus) + " id=" + gy.Id + " min=" + gy.Minoseg, TipusSzin(gy.Tipus));
                Monitor.PulseAll(locker);
            }
        }

        public static Gyogyszer Kivesz(GyogyszerTipus tipus)
        {
            lock (locker)
            {
                while (tarolo[tipus].Count == 0)
                {
                    if (termelesVege)
                        return null;
                    Monitor.Wait(locker);
                }

                Gyogyszer gy = tarolo[tipus].Dequeue();
                aktualisDb--;

                Print("KIVESZ: " + TipusSzoveg(gy.Tipus) + " id=" + gy.Id + " min=" + gy.Minoseg, TipusSzin(gy.Tipus));
                Monitor.PulseAll(locker);
                return gy;
            }
        }

        public static void Print(string msg, ConsoleColor c)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = c;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static ConsoleColor TipusSzin(GyogyszerTipus t)
        {
            if (t == GyogyszerTipus.Fajdalomcsillapito) return ConsoleColor.Cyan;
            if (t == GyogyszerTipus.Vernyomascsokkento) return ConsoleColor.Yellow;
            return ConsoleColor.Magenta;
        }

        public static string TipusSzoveg(GyogyszerTipus t)
        {
            if (t == GyogyszerTipus.Fajdalomcsillapito) return "Fajdalomcsillapito";
            if (t == GyogyszerTipus.Vernyomascsokkento) return "Vernyomascsokkento";
            return "Allergia";
        }
    }

    class Termelo
    {
        int id;
        int varakozasMs;
        GyogyszerTipus tipus;
        int termelendDb;
        public bool Mukodik;

        public Termelo(int id, int varakozasMs, GyogyszerTipus tipus)
        {
            this.id = id;
            this.varakozasMs = varakozasMs;
            this.tipus = tipus;

            termelendDb = Felugyelo.RandomKozott(70, 95);
            Mukodik = true;
        }

        public void Futtat()
        {
            Felugyelo.TermeloStart();
            Felugyelo.Print("TERMELO START id=" + id + " tipus=" + Felugyelo.TipusSzoveg(tipus) + " db=" + termelendDb, ConsoleColor.Gray);

            for (int i = 0; i < termelendDb; i++)
            {
                Thread.Sleep(varakozasMs);

                int gyId = Felugyelo.UjId();
                int minoseg = Felugyelo.RandomKozott(1, 6);

                Gyogyszer gy = new Gyogyszer(gyId, tipus, minoseg);
                Felugyelo.Betesz(gy);
            }

            Mukodik = false;
            Felugyelo.Print("TERMELO STOP id=" + id, ConsoleColor.Gray);
            Felugyelo.TermeloStop();
        }
    }

    class Fogyaszto
    {
        int id;
        GyogyszerTipus tipus;
        int csomagMeret;

        public int OsszesCsomag;
        public int OsszesBecsomagoltGy;

        public Fogyaszto(int id, GyogyszerTipus tipus, int csomagMeret)
        {
            this.id = id;
            this.tipus = tipus;
            this.csomagMeret = csomagMeret;
        }

        public void Futtat()
        {
            Felugyelo.Print("FOGYASZTO START id=" + id + " tipus=" + Felugyelo.TipusSzoveg(tipus), ConsoleColor.Gray);

            while (true)
            {
                int osszegyujtott = 0;

                while (osszegyujtott < csomagMeret)
                {
                    Gyogyszer gy = Felugyelo.Kivesz(tipus);
                    if (gy == null)
                    {
                        Felugyelo.Print("FOGYASZTO STOP id=" + id + " (nincs eleg a csomaghoz)", ConsoleColor.Gray);
                        Felugyelo.Print("FOGYASZTO id=" + id + " csomag=" + OsszesCsomag + " gyogyszer=" + OsszesBecsomagoltGy, ConsoleColor.Gray);
                        return;
                    }

                    osszegyujtott++;
                }

                OsszesCsomag++;
                OsszesBecsomagoltGy += csomagMeret;

                Felugyelo.Print("CSOMAG KESZ id=" + id + " tipus=" + Felugyelo.TipusSzoveg(tipus), Felugyelo.TipusSzin(tipus));
                Thread.Sleep(40);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Felugyelo.Init(95);

            Termelo t1 = new Termelo(1, 90, GyogyszerTipus.Fajdalomcsillapito);
            Termelo t2 = new Termelo(2, 120, GyogyszerTipus.Vernyomascsokkento);
            Termelo t3 = new Termelo(3, 70, GyogyszerTipus.Allergia);

            Fogyaszto f1 = new Fogyaszto(1, GyogyszerTipus.Fajdalomcsillapito, 20);
            Fogyaszto f2 = new Fogyaszto(2, GyogyszerTipus.Fajdalomcsillapito, 20);
            Fogyaszto f3 = new Fogyaszto(3, GyogyszerTipus.Vernyomascsokkento, 20);
            Fogyaszto f4 = new Fogyaszto(4, GyogyszerTipus.Vernyomascsokkento, 20);
            Fogyaszto f5 = new Fogyaszto(5, GyogyszerTipus.Allergia, 20);
            Fogyaszto f6 = new Fogyaszto(6, GyogyszerTipus.Allergia, 20);

            Thread pt1 = new Thread(new ThreadStart(t1.Futtat));
            Thread pt2 = new Thread(new ThreadStart(t2.Futtat));
            Thread pt3 = new Thread(new ThreadStart(t3.Futtat));

            Thread ct1 = new Thread(new ThreadStart(f1.Futtat));
            Thread ct2 = new Thread(new ThreadStart(f2.Futtat));
            Thread ct3 = new Thread(new ThreadStart(f3.Futtat));
            Thread ct4 = new Thread(new ThreadStart(f4.Futtat));
            Thread ct5 = new Thread(new ThreadStart(f5.Futtat));
            Thread ct6 = new Thread(new ThreadStart(f6.Futtat));

            pt1.Start(); pt2.Start(); pt3.Start();
            ct1.Start(); ct2.Start(); ct3.Start(); ct4.Start(); ct5.Start(); ct6.Start();

            pt1.Join(); pt2.Join(); pt3.Join();
            ct1.Join(); ct2.Join(); ct3.Join(); ct4.Join(); ct5.Join(); ct6.Join();

            Console.ResetColor();
            Console.WriteLine("Kesz.");
            Console.ReadKey();
        }
    }
}
