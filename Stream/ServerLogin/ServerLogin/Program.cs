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

namespace ServerLogin
{
    class User
    {
        public string Username;
        public string Password;
    }
    class ClientComm
    {
        StreamWriter w = null;
        StreamReader r = null;
        TcpClient cl;

        bool loggedIn = false;
        string loggedUser = "";
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
                    switch (line[0].ToUpper().Trim())
                    {
                        case "LOGIN": Login(line); break;
                        case "LOGOUT": Logout(); break;
                        case "ADD": Add(int.Parse(line[1]), int.Parse(line[2])); break;
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
        void Login(string[] line)
        {
            if (line.Length < 3) { w.WriteLine("ERR|LOGIN|user|pass"); return; }
            if (loggedIn) { w.WriteLine("ERR|Already logged in"); return; }

            string u = line[1];
            string p = line[2];

            if (Program.Users.Any(user => user.Username == u && user.Password == p))
            {
                loggedIn = true;
                loggedUser = u;
                w.WriteLine("OK|LOGIN");
            }
            else
            {
                w.WriteLine("ERR|Bad credentials");
            }
        }

        void Logout()
        {
            if (!loggedIn) { w.WriteLine("ERR|Not logged in"); return; }
            loggedIn = false;
            loggedUser = "";
            w.WriteLine("OK|LOGOUT");
        }

        void Add(int a, int b)
        {
            w.WriteLine("OK|{0}", a + b);
        }

        void Help()
        {
            w.WriteLine("OK*");
            w.WriteLine("LOGIN|user|pass   - belepes");
            w.WriteLine("LOGOUT            - kilepes");
            w.WriteLine("LIST              - user lista");
            w.WriteLine("ADD|a|b           - ket szam osszeadasa");
            w.WriteLine("HELP              - parancsok listaja");
            w.WriteLine("EXIT              - kapcsolat bontasa");
            w.WriteLine("OK!");
        }
    }

    class Program
    {
        public static List<User> Users = new List<User>();
        static List<User> LoadUsers(string path)
        {
            List<User> users = new List<User>();
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines) { 
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] p = line.Split(';');
                User u = new User();
                u.Username = p[0];
                u.Password = p[1];
                users.Add(u);
            }
            return users;
        }
        static void Main(string[] args)
        {
            Users = LoadUsers("users.txt");

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
