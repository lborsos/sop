using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientNv1
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("127.0.0.1", 2456);
            StreamReader r = new StreamReader(client.GetStream(), Encoding.UTF8);
            StreamWriter w = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };
            string comm = r.ReadLine();
            Console.WriteLine(comm);
            string task = Console.ReadLine();
            w.WriteLine(task);
            //w.Flush();
            while (task.ToUpper() != "EXIT")
            {
                string answer = r.ReadLine();
                Console.WriteLine("RAW FROM SERVER: " + answer);
                if (answer == null) break; //Ha a aserever bontja a kapcsolatot
                if (answer == "OK*") //multi line answer
                    while ((answer = r.ReadLine()) != "OK!")
                    {
                        Console.WriteLine(answer);
                    }
                else
                {
                    string[] line = answer.Split('|');
                    if (line[0] == "ERR")
                        Console.WriteLine(answer);
                    else
                        Console.WriteLine($"Az eredmeny: {line[1]}");
                }
                Console.WriteLine("A kovetkezo parancs?");
                task = Console.ReadLine();
                w.WriteLine(task);
                //w.Flush();
            }
        }
    }
}
