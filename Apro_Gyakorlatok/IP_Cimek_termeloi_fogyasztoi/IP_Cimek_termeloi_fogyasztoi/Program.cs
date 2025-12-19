using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 IP címek – Blockingcollection és ThreadPool

BlockingCollection és ThreadPool használatával oldja meg a következő termelő-fogyasztó problémát! A termelők IP címeket állítanak elő. 
Négy típusú termelő van: A, B C és D, amelyek csak ilyen osztályú címeket állítanak elő. Mindegyik különböző mennyiségben állítja elő ezeket. 
Az A osztályú cím előállításához kell a legkevesebb idő, a legtöbb a D osztályú címhez (Thread.Sleep). 
A fogyasztóknak is négy típusa van, A, B, C és D, akik csak ilyen osztályú címeket vesznek ki a pufferből. 
A berakásról és a kivételről is üzeneteket írnak ki, minden címtípushoz különböző szín tartozzon. A fogyasztók valamennyi címet feldolgozzák.

A kollekciókhoz (4 darab van) hozzáférést, a termelők és a fogyasztók kontrollálását a Supervisor osztály végezze.

A termelő osztály mezői:

IP_type. értéke A vagy B vagy C vagy D – Char típus. Mit állít elő.
Colour: 3 szín lehet, mindháromhoz eltérő szín. ConsoleColor típusú
Amount: hány címet kell előállítania. int.
WorkTime: a „termelés” után hány millisec-et kell várni. (Lehet előtte is.:-). int.

A fogyasztó osztály mezői

IP_type: értéke A vagy B vagy C vagy D. Char típus. Mit fogyaszt.
Colour: 3 szín lehet, mindháromhoz eltérő szín. ConsoleColor típusú

IP Address classes:
A.   1.0.0.0 – 126.255.255.255.
B. 128.0.0.0 – 191.255.255.255
C. 192.0.0.0 – 223.255.255.255
D. 224.0.0.0 – 239.255.255.255

Ezeket a Random osztály segítségével generálja. Négy termelő és négy fogyasztó legyen! (És persze négy tároló, különböző kapacitásokkal!) 
A fenti korlátokon kívül más megkötés nincs.
*/

namespace IP_Cimek_termeloi_fogyasztoi
{
    internal class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
