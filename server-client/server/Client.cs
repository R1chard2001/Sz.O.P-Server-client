using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace server
{
    internal class Client // kliensek kezelését segítő osztály
    {
        // kliens "adatbázis", tartalmazza az összes csatlakozott klienst
        public static List<Client> clients = new List<Client>();
        public static void CloseAllClient() // szerver leállítását segíti
        {
            foreach (Client c in new List<Client>(Client.clients))
            {
                c.Close();
            }
        }

        // a kliensen jelenleg bejelentkezett felhasználó, ha null, akkor
        // még nem jelentkezett be senki
        private User currentUser = null;
        public Client(TcpClient TcpClient)
        {
            this.tcpClient = TcpClient;
            // stream lekérdezése
            reader = new StreamReader(TcpClient.GetStream());
            writer = new StreamWriter(TcpClient.GetStream());
            // belerakás az adatbázisba
            clients.Add(this);
            // bejövő adatok beolvasásának indítása
            ClientThread = new Thread(ReadCommands);
            ClientThread.Start();
            Console.WriteLine("New client started!");
        }
        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;
        public Thread ClientThread;
        private void ReadCommands()
        {
            try
            {
                while (true)
                {
                    // bejövő adat lementése
                    string command = reader.ReadLine();
                    Console.WriteLine("Command recived: {0}", command);
                    // és értelmezése
                    Interpret(command);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                // ha valami baj történne, akkor szabadítsuk fel a használt
                // erőforrásokat
                Close();
                Console.WriteLine("Client disconnected");
            }
        }
        private void Interpret(string cmd) // értelmező
        {
            string[] data = cmd.Split(' '); // bejövő adat szétválasztása
            switch (data[0]) // maga a parancs értelmezése, majd az alapján cselekedni
            {
                case "exit":
                    Close();
                    break;
                case "login":
                    Login(data); // ha paraméteres lenne a parancs, akkor elküldjük a paramétereket is
                    break;
                case "logout":
                    Logout();
                    break;
                case "list":
                    ListResources();
                    break;
                case "add":
                    AddResource(data);
                    break;
                case "search":
                    SearchResource(data);
                    break;
                default:
                    SendInformation("Unknown command!");
                    break;
            }
        }
        private void Login(string[] data)
        {
            if (data.Length != 3) // paraméterek számának ellenőrzése (maga a parancs is benne lesz)
            {
                SendInformation("Incorrect parameter list!");
                return;
            }
            if (currentUser != null)
            {
                SendInformation("You are already logged in!");
                return;
            }
            User u = User.LoginTry(data[1], data[2]);
            if (u == null)
            {
                SendInformation("Incorrect username or password!");
            }
            else
            {
                SendInformation(String.Format("User {0} logged in.", u.Username));
                currentUser = u;
            }
        }
        private void Logout()
        {
            if (currentUser == null)
            {
                SendInformation("You are not logged in!");
            }
            else
            {
                currentUser = null;
                SendInformation("Logging out");
            }
        }
        private void ListResources()
        {
            foreach (Resource r in Resource.ResourceList)
            {
                SendInformation(String.Format("Resource:\n - someString: {0}\n - someInt: {1}", r.SomeString, r.SomeInt));
            }
        }
        private void AddResource(string[] data)
        {
            if (data.Length != 3)
            {
                SendInformation("Incorrect parameter list!");
                return;
            }
            int i;
            if (!int.TryParse(data[2], out i))
            {
                SendInformation("Parameter must be integer!");
                return;
            }
            Resource.ResourceList.Add(new Resource(data[1], i));
            SendInformation("New resource added!");
        }
        private void SearchResource(string[] data)
        {
            if (data.Length != 2)
            {
                SendInformation("Incorrect parameter list!");
                return;
            }
            foreach (Resource r in Resource.ResourceList.FindAll(r => r.SomeString == data[1]))
            {
                SendInformation(String.Format("Resource:\n - someString: {0}\n - someInt: {1}", r.SomeString, r.SomeInt));
            }
        }
        private void SendInformation(string info)
        {
            // adat küldése, kell ide is a Flush(), mint a kliens programba is kellett
            writer.WriteLine(info);
            writer.Flush();
        }
        public void Close()
        {
            // erőforrások bezárása, és a kliens példányunk eltávolítása a listából
            reader.Close();
            writer.Close();
            tcpClient.Close();
            clients.Remove(this);
        }
    }
}
