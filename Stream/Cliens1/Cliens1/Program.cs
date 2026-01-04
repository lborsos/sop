using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cliens1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("127.0.0.1", 2456);
            StreamWriter w = new StreamWriter(client.GetStream(), Encoding.UTF8);
            StreamReader r = new StreamReader(client.GetStream(), Encoding.UTF8);
            string answer = r.ReadLine();
            Console.WriteLine(answer);
            bool end = false;
            while (!end)
            {
                Console.WriteLine("The command?");
                string command = Console.ReadLine();
                w.WriteLine(command);
                w.Flush();
                answer = r.ReadLine();
                Console.WriteLine(answer);
                if (answer == "OK!")
                {
                    Console.WriteLine("More lines message");
                    while (answer != "OK*")
                    {
                        answer = r.ReadLine();
                        Console.WriteLine(answer);
                    }
                }
                if (answer.ToUpper() == "BYE")
                    end = true;
            }
            Console.ReadKey();
        }
    }
}
