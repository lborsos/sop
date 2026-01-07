using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace ServerNv2
{
    class ClientComm
    {
        StreamWriter w = null;
        StreamReader r = null;
        TcpClient cl;
        public ClientComm(TcpClient client)
        {
            cl = client;
            w = new StreamWriter(cl.GetStream(), Encoding.UTF8) { AutoFlush = true };
            r = new StreamReader(cl.GetStream(), Encoding.UTF8);
        }

        public void Start()
        {
            try
            {
                w.WriteLine("Server v1.0");
                string command;

                while ((command = r.ReadLine()) != null && command != "EXIT")
                {
                    string[] line = command.Split('|');
                    switch (line[0].ToUpper())
                    {
                        case "ADD": Add(int.Parse(line[1]), int.Parse(line[2])); break;
                        case "PRIMEK": Primes(int.Parse(line[1])); break;
                        case "FIBO": Fibonacci(int.Parse(line[1])); break;
                        case "HELP":
                        case "?": Help(); break;
                        default: w.WriteLine("ERR|Nincs ilyen muvelet"); break;
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    w.WriteLine("ERR|" + ex.Message);
                }
                catch { }
            }
            finally
            {
                try { r?.Close(); } catch { }
                try { w?.Close(); } catch { }
                try { cl?.Close(); } catch { }
            }
        }
        void Add(int a, int b)
        {
            w.WriteLine("OK|{0}", a + b);
        }

        void Primes(int N)
        {
            w.WriteLine("OK*");
            for (int i = 2; i <= N; i++)
                if (Prime(i))
                    w.WriteLine(i);
            w.WriteLine("OK!");
        }

        static bool Prime(int num)
        {
            bool prim = true;
            for (int i = 2; i <= Math.Sqrt(num) && prim; i++)
                if (num % i == 0)
                    prim = false;
            return prim;
        }


        void Fibonacci(int N)
        {
            w.WriteLine("OK|8");
        }
        void Help()
        {
            w.WriteLine("OK*");
            w.WriteLine("ADD|a|b        - ket szam osszeadasa");
            w.WriteLine("PRIMEK|n       - primek n-ig");
            w.WriteLine("FIBO|n         - fibonacci sor (demo)");
            w.WriteLine("HELP           - parancsok listaja");
            w.WriteLine("EXIT           - kapcsolat bontasa");
            w.WriteLine("OK!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string ipAddr = ConfigurationManager.AppSettings["IP-addr"];
            string portNr = ConfigurationManager.AppSettings["port"];
            IPAddress ip = IPAddress.Parse(ipAddr);
            int port = int.Parse(portNr);
            TcpListener listener = new TcpListener(ip, port);
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ClientComm cl = new ClientComm(client);
                Thread t = new Thread(cl.Start);
                t.Start();
            }
        }
    }
}
