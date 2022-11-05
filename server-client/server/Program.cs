using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Configuration; // ezt külön a References-be is be kellett hivatkozni

namespace server
{
    internal class Program
    {
        // a szervernek kell egy TCP listener, mellyel majd megnyitja az adott portot
        private static bool listen = true;
        private static TcpListener listener;

        // az adatbázisfájlok elérés módosításának könnyítésére 
        private const string userFile = "Users.xml";
        private const string resourceFile = "Resources.xml";
        static void Main(string[] args)
        {
            // felhasználók és a resource példányok betöltése
            User.LoadUsers(userFile);
            Console.WriteLine("Users loaded successfully!");

            Resource.LoadResources(resourceFile);
            Console.WriteLine("Resources loaded successfully!");

            // ip és port betöltése az App.config fájlból
            IPAddress ip = IPAddress.Parse(ConfigurationManager.AppSettings["ip"].ToString());
            int port = int.Parse(ConfigurationManager.AppSettings["port"]);
            Console.WriteLine("Configuration loaded!\n - IP: {0}\n - Port: {1}", ip, port);

            // TCP listener példányosítása, és a hallgatózás elindítása
            listener = new TcpListener(ip, port);
            listener.Start();
            // maga a TCP kliensek elfogadását külön szálon csináljuk
            Thread listenerThread = new Thread(WaitingForClients);
            listenerThread.Start();
            Console.WriteLine("Listener started!");

            // a szerver megállítása, majd enter nyomására kikapcsolása
            Console.WriteLine("Press ENTER to close the program!");
            Console.ReadLine();

            listen = false;
            listener.Stop();
            Client.CloseAllClient();
            // kikapcsoláskor "adatbázis" lementése a fájlokba
            User.SaveUsers(userFile);
            Resource.SaveResources(resourceFile);
        }
        static private void WaitingForClients() // klienst elfogadó metódus
        {
            while (listen)
            {
                if (listener.Pending())
                {
                    TcpClient client = listener.AcceptTcpClient();
                    new Client(client);
                }
            }
        }
    }
}
