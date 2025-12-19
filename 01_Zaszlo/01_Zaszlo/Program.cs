using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



//1) 
// Készítsünk olyan programot, amely a magyar zászlót rajzolja ki a karakteres képernyõre az alábbi módon: készítsünk elõször egy külön osztályt az alábbi adatokkal:

//    y1, y2 egész szám adatok,
//    szín
//    kirajzolandó csillagok darabszáma
//    egy kirajzol() paraméter nélküli void-os függvény, mely a képernyõn az adott y1..y2 koordináták közé esõ sávban adott szín színnel random koordinátákra
//    csillag karaktert rajzol.

//Példányosítsuk meg az osztályt 3 példányra, az elsõ a y1=0, y2 = 7, szín = piros beállítások mellett egy piros sávot rajzol hasonlóan a második fehér,
//a harmadik zöld sávot rajzol ki. A képernyõ 25 soros (0..24), és 80 oszlopos (0..79).

//Hívjuk meg a három kirajzol() függvényt egymás után, hogy megkapjuk a három sávot. 


namespace _01_Zaszlo
{
    class Sav
    {
        public int y1, y2, darab;
        public ConsoleColor szin;
        Random rnd = new Random();
        public Sav(int y1, int y2, ConsoleColor szin, int darab)
        {
            this.y1 = y1;
            this.y2 = y2;
            this.szin = szin;
            this.darab = darab;
        }
        public void Kirajzol()
        {
            Console.ForegroundColor = szin;
            for (int i = 0; i < darab; i++)
            {
//                lock (typeof(Program)) // "typeoff(Program)" helyett lehetett volna "Console.Out"
                lock (Console.Out) // "typeoff(Program)" helyett lehetett volna "Console.Out"
                    {
                        Console.ForegroundColor = szin;
                    Console.SetCursorPosition(rnd.Next(0, 80), rnd.Next(y1, y2 + 1));
                    Console.Write("*");
                }
                //Thread.Sleep(5);
            }
        }

    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            Sav piros = new Sav(0, 7, ConsoleColor.Red, 3000);
            Sav feher = new Sav(8, 15, ConsoleColor.White, 3000);
            Sav zold = new Sav(16, 24, ConsoleColor.Green, 3000);

            Thread t1 = new Thread(piros.Kirajzol);
            Thread t2 = new Thread(feher.Kirajzol);
            Thread t3 = new Thread(zold.Kirajzol);

            t1.Start(); t2.Start(); t3.Start();

            t1.Join(); t2.Join(); t3.Join();

            Console.SetCursorPosition(0, 25);
        }
    }
}
