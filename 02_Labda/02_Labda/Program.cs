using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


//2) Írjunk egy programot, amely N<5 labdát (*) tesz ki a képernyõre, majd ezeket elindítja átlós (valamelyik) irányba,
//és a képernyõ szélére érve ezek visszapattannak. A konzol 80*25-ös. Az osztály neve labda legyen, a mozgatást a 
//mozog metódus végezze!

namespace _02_Labda
{
    class Labda
    {
        static object konzolZar = new object();
        int x, y, dx, dy;

        public Labda(int x, int y, int dx, int dy)
        {
            this.x = x;
            this.y = y;
            this.dx = dx;
            this.dy = dy;
        }

        public void Mozog()
        {
            while (true)
            {
                lock (konzolZar)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(' ');
                }

                int kovX = x + dx;
                int kovY = y + dy;

                if (kovX < 0 || kovX > 79)
                {
                    dx = -dx;
                    kovX = x + dx;
                }

                if (kovY < 0 || kovY > 24)
                {
                    dy = -dy;
                    kovY = y + dy;
                }

                x = kovX;
                y = kovY;

                lock (konzolZar)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write('*');
                }

                Thread.Sleep(50);
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Clear();

            //Console.Write("Labdák száma (1-4): ");
            int n=20;
            //if (!int.TryParse(Console.ReadLine(), out n) || n < 1 || n > 4)
            //    n = 4;

            Random rnd = new Random();
            Thread[] szalak = new Thread[n];

            for (int i = 0; i < n; i++)
            {
                int x = rnd.Next(0, 80);
                int y = rnd.Next(0, 25);
                int dx = rnd.Next(2) == 0 ? -1 : 1;
                int dy = rnd.Next(2) == 0 ? -1 : 1;

                Labda l = new Labda(x, y, dx, dy);

                szalak[i] = new Thread(l.Mozog);
                szalak[i].IsBackground = true;
                szalak[i].Start();
            }

            Console.ReadKey();
        }
    }
}
