using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTipMix
{
    class User
    {
        public string Name;
        public string Password;
    }

    class FutballMD
    {
        public string MatchId;
        public string Home;
        public string Away;
        public int GoalsForSum;
        public int GoalsAgainstSum;
        public int PredictCount;

        public override string ToString()
            => $"{MatchId};{Home};{Away};{GoalsForSum};{GoalsAgainstSum};{PredictCount}";

        public string ToAvgString()
        {
            if (PredictCount <= 0)
                return $"{MatchId};{Home};{Away};0.00;0.00;0";

            double homeAvg = (double)GoalsForSum / PredictCount;
            double awayAvg = (double)GoalsAgainstSum / PredictCount;

            return $"{MatchId};{Home};{Away};{homeAvg.ToString("0.00", CultureInfo.InvariantCulture)};{awayAvg.ToString("0.00", CultureInfo.InvariantCulture)};{PredictCount}";
        }
    }

    class ServerState
    {
        public Dictionary<string, User> Users = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, FutballMD> Matches = new Dictionary<string, FutballMD>(StringComparer.OrdinalIgnoreCase);
        public object Sync = new object();

        public void LoadUsers(string path)
        {
            if (!File.Exists(path)) File.WriteAllText(path, "");
            foreach (var line in File.ReadAllLines(path))
            {
                var t = line.Trim();
                if (t.Length == 0) continue;
                var p = t.Split(';');
                if (p.Length < 2) continue;
                Users[p[0].Trim()] = new User { Name = p[0].Trim(), Password = p[1].Trim() };
            }
        }

        public void LoadMatches(string path)
        {
            if (!File.Exists(path)) File.WriteAllText(path, "");
            foreach (var line in File.ReadAllLines(path))
            {
                var t = line.Trim();
                if (t.Length == 0) continue;
                var p = t.Split(';');
                if (p.Length < 6) continue;

                if (!int.TryParse(p[3], out int gf)) gf = 0;
                if (!int.TryParse(p[4], out int ga)) ga = 0;
                if (!int.TryParse(p[5], out int pc)) pc = 0;

                var m = new FutballMD
                {
                    MatchId = p[0].Trim(),
                    Home = p[1].Trim(),
                    Away = p[2].Trim(),
                    GoalsForSum = gf,
                    GoalsAgainstSum = ga,
                    PredictCount = pc
                };
                Matches[m.MatchId] = m;
            }
        }

        public void SaveMatches(string path)
        {
            lock (Sync)
            {
                File.WriteAllLines(path, Matches.Values
                    .OrderBy(x => x.MatchId, StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.ToString()));
            }
        }
    }

    class ClientComm
    {
        readonly TcpClient cl;
        readonly StreamReader r;
        readonly StreamWriter w;
        readonly ServerState state;
        readonly string matchesPath;

        bool loggedIn = false;
        string loggedUser = null;

        public ClientComm(TcpClient client, ServerState s, string matchesFile)
        {
            cl = client;
            state = s;
            matchesPath = matchesFile;

            var ns = cl.GetStream();
            r = new StreamReader(ns, Encoding.UTF8);
            w = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };
        }

        public void Start()
        {
            try
            {
                w.WriteLine("OK|WELCOME");

                while (true)
                {
                    string cmd = r.ReadLine();
                    if (cmd == null) break;

                    cmd = cmd.Trim();
                    if (cmd.Length == 0)
                    {
                        w.WriteLine("ERR|Ures parancs");
                        continue;
                    }

                    var parts = cmd.Split('|');
                    var op = parts[0].Trim().ToUpperInvariant();

                    if (op == "EXIT")
                    {
                        w.WriteLine("OK|BYE");
                        break;
                    }

                    switch (op)
                    {
                        case "LIST":
                            HandleList();
                            break;

                        case "LOGIN":
                            HandleLogin(parts);
                            break;

                        case "LOGOUT":
                            HandleLogout();
                            break;

                        case "PREDICT":
                            HandlePredict(parts);
                            break;

                        case "MATCH":
                            HandleMatch(parts);
                            break;

                        default:
                            w.WriteLine("ERR|Ismeretlen parancs");
                            break;
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (SocketException)
            {
            }
            catch
            {
            }
            finally
            {
                try { r.Dispose(); } catch { }
                try { w.Dispose(); } catch { }
                try { cl.Close(); } catch { }
            }
        }

        void HandleList()
        {
            w.WriteLine("OK*");
            lock (state.Sync)
            {
                foreach (var m in state.Matches.Values.OrderBy(x => x.MatchId, StringComparer.OrdinalIgnoreCase))
                    w.WriteLine(m.ToString());
            }
            w.WriteLine("OK!");
        }

        void HandleLogin(string[] parts)
        {
            if (parts.Length < 3)
            {
                w.WriteLine("ERR|LOGIN formatum: LOGIN|nev|jelszo");
                return;
            }

            var name = parts[1].Trim();
            var pass = parts[2].Trim();

            if (name.Length == 0 || pass.Length == 0)
            {
                w.WriteLine("ERR|Hibas adatok");
                return;
            }

            if (!state.Users.TryGetValue(name, out var u) || u.Password != pass)
            {
                w.WriteLine("ERR|Hibas nev vagy jelszo");
                return;
            }

            loggedIn = true;
            loggedUser = u.Name;
            w.WriteLine("OK|LOGIN");
        }

        void HandleLogout()
        {
            loggedIn = false;
            loggedUser = null;
            w.WriteLine("OK|LOGOUT");
        }

        void HandlePredict(string[] parts)
        {
            if (!loggedIn)
            {
                w.WriteLine("ERR|Nincs bejelentkezve");
                return;
            }

            if (parts.Length < 4)
            {
                w.WriteLine("ERR|PREDICT formatum: PREDICT|matchid|lott|kapott");
                return;
            }

            var id = parts[1].Trim();
            if (!int.TryParse(parts[2], out int gf) || !int.TryParse(parts[3], out int ga) || gf < 0 || ga < 0)
            {
                w.WriteLine("ERR|Hibas gol ertek");
                return;
            }

            FutballMD m;
            lock (state.Sync)
            {
                if (!state.Matches.TryGetValue(id, out m))
                {
                    w.WriteLine("ERR|Nincs ilyen meccs");
                    return;
                }

                m.GoalsForSum += gf;
                m.GoalsAgainstSum += ga;
                m.PredictCount += 1;

                state.SaveMatches(matchesPath);
            }

            w.WriteLine("OK|PREDICTED");
        }

        void HandleMatch(string[] parts)
        {
            if (parts.Length < 2)
            {
                w.WriteLine("ERR|MATCH formatum: MATCH|matchid");
                return;
            }

            var id = parts[1].Trim();
            lock (state.Sync)
            {
                if (!state.Matches.TryGetValue(id, out var m))
                {
                    w.WriteLine("ERR|Nincs ilyen meccs");
                    return;
                }
                w.WriteLine("OK|" + m.ToAvgString());
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string ipStr = "127.0.0.1";
            int port = 12345;

            string usersPath = "lusers.txt";
            string matchesPath = "matches.txt";

            var state = new ServerState();
            state.LoadUsers(usersPath);
            state.LoadMatches(matchesPath);


            Console.WriteLine("=== TipMix SERVER START ===");
            Console.WriteLine("RUN DIR: " + Directory.GetCurrentDirectory());

            if (File.Exists(usersPath))
                Console.WriteLine($"lusers.txt OK ({File.ReadAllLines(usersPath).Length} sor)");
            else
                Console.WriteLine("lusers.txt HIANYZIK");

            if (File.Exists(matchesPath))
                Console.WriteLine($"matches.txt OK ({File.ReadAllLines(matchesPath).Length} sor)");
            else
                Console.WriteLine("matches.txt HIANYZIK");


            var ip = IPAddress.Parse(ipStr);
            var listener = new TcpListener(ip, port);
            listener.Start();
                
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                var comm = new ClientComm(client, state, matchesPath);
                new Thread(comm.Start).Start();
            }
        }
    }
}
