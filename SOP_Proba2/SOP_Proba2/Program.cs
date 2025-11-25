using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SOP_Proba2
{
    internal class Program
    {
        private static void kiir()
        {
            int x = 0;
            while (x < 10)
            {
                Console.WriteLine("Szal {0}",x);
                Thread.Sleep(1000);
                x++;
            }
            Console.WriteLine("Szal vege!");
        }
        static void Main(string[] args)
        {
            int x = 0;
            Console.WriteLine("Program indul... ENTER - Suspend / Resume... ESC - Abort Szal es a program s leall... X - Program vege de a szal fut mig lejar!");
            Thread t = new Thread(kiir);
            t.Start();
            bool tsuspend = false;
            while (true)
            {
                Console.WriteLine("Program kiir {0}", x);
                if (Console.KeyAvailable)   // csak akkor olvas, ha van billentyű
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("ESC megnyomva -> kilépés.");
                        t.Abort();
                        break;
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        if (tsuspend) {
                            Console.WriteLine("Szal indul!");
                            t.Resume();
                        }
                        else {
                            Console.WriteLine("Szal leall!");
                            t.Suspend();
                        }
                        tsuspend = !tsuspend;
                    } else if (key.KeyChar == 'x' || key.KeyChar == 'X')
                    {
                        Console.WriteLine("X betű lenyomva!");
                        if (t.ThreadState != ThreadState.Running)
                        {
                            t.Join();
                        }
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Lenyomott karakter: '{key.KeyChar}' - Gomb: {key.Key}");
                    }
                }
                x++;
                Thread.Sleep(500);
            }
        }
    }
}
