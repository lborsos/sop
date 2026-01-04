using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Stream1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpListener figyelo = null;
            try {
                string ipAddr = ConfigurationManager.AppSettings["IP-addr"];
                string portNr = ConfigurationManager.AppSettings["port"];
                IPAddress ip = IPAddress.Parse(ipAddr);
                int port = int.Parse(portNr);
                figyelo = new TcpListener(ip, port);
                figyelo.Start();
                Console.WriteLine(ipAddr);
            }
            catch  { figyelo = null; }
            TcpClient bejovo = figyelo.AcceptTcpClient();
            StreamWriter w = new StreamWriter(bejovo.GetStream(),Encoding.UTF8);
            StreamReader r = new StreamReader(bejovo.GetStream(), Encoding.UTF8);

            w.WriteLine("Server, 1.0"); w.Flush(); bool end = false;
            while (!end)
            {
                string[] command = r.ReadLine().Split('|');
                switch (command[0].ToUpper())
                {
                    case "DIVIDE":
                        if (int.Parse(command[2]) == 0)
                        {
                            w.WriteLine("OK!");
                            w.WriteLine("Division by zero");
                            w.WriteLine("OK*");
                        }
                        else
                            w.WriteLine("{0}", float.Parse(command[1]) /
                                float.Parse(command[2]));
                        break;
                    case "BYE":
                        w.WriteLine("BYE"); end = true; break;
                    default:
                        w.WriteLine("Unknown command"); break;
                }
                w.Flush();
            }
        }
    }
}
