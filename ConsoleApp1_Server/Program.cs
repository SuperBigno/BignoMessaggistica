using ConsoleApplication1;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ConsoleApp1_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int porta = 21212;
            TcpListener ascoltatore = new TcpListener(IPAddress.Any, porta);

            try
            {
                ascoltatore.Start();
                Console.WriteLine($"Server in ascolto sulla porta {porta}...");

                while (true)
                {
                    TcpClient client = ascoltatore.AcceptTcpClient();
                    Console.WriteLine("Nuova connessione accettata.");

                    Thread clientThread = new Thread(() => new GestioneServer(client));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore del server: {ex.Message}");
            }
            finally
            {
                ascoltatore.Stop();
                Console.WriteLine("Server arrestato.");
            }
        }
    }
}
