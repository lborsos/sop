using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientTipMix
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TcpClient("127.0.0.1", 12345);
            var r = new StreamReader(client.GetStream(), Encoding.UTF8);
            var w = new StreamWriter(client.GetStream(), Encoding.UTF8) { AutoFlush = true };

            Console.WriteLine(r.ReadLine());

            while (true)
            {
                Console.Write("> ");
                string cmd = Console.ReadLine();
                if (cmd == null) break;

                w.WriteLine(cmd);

                string ans = r.ReadLine();
                if (ans == null) break;

                if (ans == "OK*")
                {
                    while (true)
                    {
                        string line = r.ReadLine();
                        if (line == null) return;
                        if (line == "OK!") break;
                        Console.WriteLine(line);
                    }
                    continue;
                }

                var p = ans.Split('|');
                if (p.Length > 0 && p[0] == "OK")
                {
                    Console.WriteLine(p.Length > 1 ? p[1] : "OK");
                    if (p.Length > 1 && p[1] == "BYE") break;
                }
                else if (p.Length > 0 && p[0] == "ERR")
                {
                    Console.WriteLine(p.Length > 1 ? "Hiba: " + p[1] : "Hiba");
                }
                else
                {
                    Console.WriteLine(ans);
                }
            }

            try { client.Close(); } catch { }
        }
    }
}
