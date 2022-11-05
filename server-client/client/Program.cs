using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Configuration; // ezt külön a References-be is be kellett hivatkozni
using System.IO;
using System.Threading;

namespace client
{
    internal class Program
    {
        static StreamReader reader;
        static StreamWriter writer;
        static TcpClient tcpClient;
        // a többsoros visszajövő adat megjelenítése érdekében
        // egy külön szálon olvassuk be azt
        static Thread ReaderThread = new Thread(ReadInfo);
        static void Main(string[] args)
        {
            // az ip és port lekérdezése az App.config fájlból
            string ip = ConfigurationManager.AppSettings["ip"].ToString();
            int port = int.Parse(ConfigurationManager.AppSettings["port"].ToString());
            try
            {
                // csatlakozás a megadott ip portjára
                tcpClient = new TcpClient(ip, port);
                // stream-ek lekérdezése a kommunikációhoz
                writer = new StreamWriter(tcpClient.GetStream());
                reader = new StreamReader(tcpClient.GetStream());
                // az adatok beolvasásának elindítása
                ReaderThread.Start();
                Console.WriteLine("Connected to {0}:{1}", ip, port);
                // itt kérjük majd be a felhasználótól a küldeni kívánt adatokat
                SendCommands();
            }
            catch (Exception)
            {
                // ha valami baj lenne a csatlakozáskor
                Console.WriteLine("Something went wrong.");
                Console.ReadLine();
            }
            
        }
        static void SendCommands()
        {
            try
            {
                while (true)
                {
                    // beolvasás és küldés
                    WriteCommand(Console.ReadLine());
                }
            }
            catch (Exception)
            { }
            finally
            {
                // ha baj lenne a küldéskor, akkor zárja be a használt erőforrásokat
                Close();
            }
        }
        static void WriteCommand(string info)
        {
            // hogy azonnal elküldje az adatot kell használni a Flush() metódust az írónak
            writer.WriteLine(info);
            writer.Flush();
        }
        static void ReadInfo() // külön szálon fut
        {
            try
            {
                // olvasás, míg van valamit
                while (!reader.EndOfStream)
                {
                    Console.WriteLine(reader.ReadLine());
                }
            }
            catch (Exception)
            { }
            finally 
            {
                // bezárni a használt erőforrásokat, figyelmeztetni a felhasználót,
                // hogy lecsatlakoztunk a szerverről 
                Close();
                Console.WriteLine("Disconnected from the server!");
            }
        }
        static void Close()
        {
            // használt erőforrások felszabadítása, bezárása
            writer.Close();
            reader.Close();
            tcpClient.Close();
        }
    }
}
