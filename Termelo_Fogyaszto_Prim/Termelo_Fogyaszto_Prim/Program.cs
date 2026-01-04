using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 3. 
A feladat: fejlesszünk olyan alkalmazást, amely 4 szálon keres prímszámokat; minden szál más-más
számintervallumon dolgozzon! Az előállított prímszámokat egy 50 elemet tárolni képes Supervisor (külön osztály)
kezelje! Indítsunk 2 feldolgozó szálat, melyek a prímszámokat a képernyőre írják, az egyik feldolgozó
szál kékkel, a másik sárgával írjon! A végén jelenítsük meg, hogy melyik feldolgozó szál hány prímet
írt ki összesen a képernyőre!

A Supervisor osztály a listán kívül tartalmazza a leállást, indítást, és figyelje a termelők
és a fogyasztók számát, valamint a berak és kivesz metódusokat is tartalmazza!

A fogyasztó osztály elkezd metódusa hívja meg a Supervisor.kivesz metódusát.
A termelő osztály elkezd metódusa hívja meg a Supervisor.berak metódust.

A két osztály elkezd metódusaira mutassanak a szálmutatók az indításkor.

 */


namespace ProducerConsumerClean
{
    class Supervisor
    {
        readonly object _lock = new object();
        readonly Queue<int> _lista = new Queue<int>();

        readonly int _maxMeret;
        int _aktivTermelok;
        int _aktivFogyasztok;
        bool _termelesLeallt;

        public int AktivTermelok { get { lock (_lock) return _aktivTermelok; } }
        public int AktivFogyasztok { get { lock (_lock) return _aktivFogyasztok; } }

        public Supervisor(int maxMeret = 50)
        {
            _maxMeret = maxMeret;
        }

        public void TermeloIndit()
        {
            lock (_lock) { _aktivTermelok++; }
        }

        public void TermeloLeallit()
        {
            lock (_lock)
            {
                _aktivTermelok--;
                if (_aktivTermelok <= 0)
                {
                    _termelesLeallt = true;
                    Monitor.PulseAll(_lock);
                }
            }
        }

        public void FogyasztoIndit()
        {
            lock (_lock) { _aktivFogyasztok++; }
        }

        public void FogyasztoLeallit()
        {
            lock (_lock) { _aktivFogyasztok--; }
        }

        public void Berak(int szam)
        {
            lock (_lock)
            {
                while (_lista.Count >= _maxMeret)
                    Monitor.Wait(_lock);

                _lista.Enqueue(szam);
                Monitor.PulseAll(_lock);
            }
        }

        public bool Kivesz(out int szam)
        {
            lock (_lock)
            {
                while (_lista.Count == 0)
                {
                    if (_termelesLeallt)
                    {
                        szam = default;
                        return false;
                    }
                    Monitor.Wait(_lock);
                }

                szam = _lista.Dequeue();
                Monitor.PulseAll(_lock);
                return true;
            }
        }
    }

    class Termelo
    {
        readonly int _tol, _ig;
        readonly Supervisor _supervisor;

        public Termelo(Supervisor supervisor, int tol, int ig)
        {
            _supervisor = supervisor;
            _tol = tol;
            _ig = ig;
        }

        public void Elkezd()
        {
            _supervisor.TermeloIndit();
            try
            {
                for (int i = _tol; i <= _ig; i++)
                    if (Prim(i))
                        _supervisor.Berak(i);
            }
            finally
            {
                _supervisor.TermeloLeallit();
            }
        }

        static bool Prim(int n)
        {
            if (n < 2) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            int limit = (int)Math.Sqrt(n);
            for (int i = 3; i <= limit; i += 2)
                if (n % i == 0) return false;

            return true;
        }
    }

    class Fogyaszto
    {
        readonly Supervisor _supervisor;
        readonly ConsoleColor _szin;
        static readonly object _consoleLock = new object();
        int _db;

        public int Darab => _db;

        public Fogyaszto(Supervisor supervisor, ConsoleColor szin)
        {
            _supervisor = supervisor;
            _szin = szin;
        }

        public void Elkezd()
        {
            _supervisor.FogyasztoIndit();
            try
            {
                while (_supervisor.Kivesz(out int szam))
                {
                    _db++;
                    lock (_consoleLock)
                    {
                        Console.ForegroundColor = _szin;
                        Console.WriteLine(szam);
                    }
                }
            }
            finally
            {
                _supervisor.FogyasztoLeallit();
            }

            lock (_consoleLock)
            {
                Console.ResetColor();
                Console.WriteLine("Nincs több termelő -> fogyasztó leállt");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var supervisor = new Supervisor(50);

            var termelo1 = new Termelo(supervisor, 1000, 2000);
            var termelo2 = new Termelo(supervisor, 2001, 3000);
            var termelo3 = new Termelo(supervisor, 3001, 4000);
            var termelo4 = new Termelo(supervisor, 4001, 5000);

            var fogyaszto1 = new Fogyaszto(supervisor, ConsoleColor.Blue);
            var fogyaszto2 = new Fogyaszto(supervisor, ConsoleColor.Yellow);

            var t1 = new Thread(termelo1.Elkezd);
            var t2 = new Thread(termelo2.Elkezd);
            var t3 = new Thread(termelo3.Elkezd);
            var t4 = new Thread(termelo4.Elkezd);

            var t5 = new Thread(fogyaszto1.Elkezd);
            var t6 = new Thread(fogyaszto2.Elkezd);

            t1.Start(); t2.Start(); t3.Start(); t4.Start();
            t5.Start(); t6.Start();

            t1.Join(); t2.Join(); t3.Join(); t4.Join();
            t5.Join(); t6.Join();

            Console.ResetColor();
            Console.WriteLine($"Kék fogyasztó összesen: {fogyaszto1.Darab} prímet írt ki.");
            Console.WriteLine($"Sárga fogyasztó összesen: {fogyaszto2.Darab} prímet írt ki.");
            Console.WriteLine("Minden szál befejezte.");

            Console.ReadKey();
        }
    }
}






