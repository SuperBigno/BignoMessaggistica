using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using ConsoleApp1_Server;
using System.Threading;
using System.Runtime.InteropServices.ComTypes;
using System.Linq;

namespace ConsoleApplication1
{
    class GestioneServer
    {
        private TcpClient client;
        private string telefonoAttivo;
        bool exit = false;

        public GestioneServer(TcpClient c)
        {
            client = c;
            new Thread(Attivita).Start();
        }

        public void Attivita()
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[2097152];
            string comando = "";
            bool uscita = false;

            try
            {
                do
                {
                    try
                    {
                        // Legge i dati dal client
                        int nLetti = stream.Read(buffer, 0, buffer.Length);

                        // Controlla se il client ha chiuso la connessione
                        if (nLetti == 0)
                        {
                            Console.WriteLine("Il client ha chiuso la connessione.");
                            uscita = true;
                            break;
                        }

                        comando = Encoding.ASCII.GetString(buffer, 0, nLetti);

                        // Visualizza il comando ricevuto
                        Console.WriteLine("Comando ricevuto: " + comando);

                        if (comando.StartsWith("CLOSE"))
                        {
                            uscita = true;
                        }
                        else
                        {
                            // Ottieni la risposta
                            object risposta = Response(comando);

                            // Gestione delle risposte
                            byte[] datiRisposta;
                            if (risposta is byte[])
                            {
                                datiRisposta = risposta as byte[];
                            }
                            else
                            {
                                datiRisposta = Encoding.ASCII.GetBytes(risposta as string);
                            }

                            // Invio della risposta al client
                            stream.Write(datiRisposta, 0, datiRisposta.Length);
                        }
                        
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("Errore Socket: " + ex.Message);
                        uscita = true;
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Errore IO: " + ex.Message);
                        uscita = true;
                    }
                } while (!uscita);
            }
            finally
            {
                // Chiudi il client e rilascia le risorse
                if (client != null)
                {
                    Console.WriteLine("Chiusura della connessione per il client.");
                    client.Close();
                    client = null;
                }
            }
        }


        public object Response(string comando)
        {
            string[] parti = comando.Split('#');
            object risposta = "Comando non valido.";

            if (parti.Length < 1)
                return risposta;

            switch (parti[0])
            {
                case "REG": // REG # telefono # nickname
                    if (parti.Length < 3) return "Errore: dati mancanti.";

                    string telefono = parti[1];
                    string nickname = parti[2];

                    Utente nuovoUtente = new Utente(nickname, telefono);
                    List<Utente> utenti = GestioneFile.ReadUsers(GestioneFile.USERS);

                    if (!utenti.Contains(nuovoUtente))
                    {
                        GestioneFile.AddUser(GestioneFile.USERS, nuovoUtente);
                        risposta = "Registrazione completata.";
                    }
                    else
                    {
                        risposta = "Errore: telefono già registrato.";
                    }
                    break;

                case "LOG": // LOG # telefono
                    if (parti.Length < 2) return "Errore: dati mancanti.";

                    string telLogin = parti[1];
                    List<Utente> utentiLog = GestioneFile.ReadUsers(GestioneFile.USERS);

                    if (utentiLog.Exists(u => u.Telefono == telLogin))
                    {
                        telefonoAttivo = telLogin;
                        risposta = "Accesso effettuato.";
                    }
                    else
                    {
                        risposta = "Errore: telefono non trovato.";
                    }
                    break;

                case "OUT": // Disconnessione
                    telefonoAttivo = null;
                    risposta = "Disconnessione avvenuta.";
                    break;

                case "CRE": // CRE # telefonoDestinatario
                    if (telefonoAttivo == null) return "Errore: utente non autenticato.";
                    if (parti.Length < 2) return "Errore: dati mancanti.";

                    string telefonoDestinatario = parti[1];
                    if (GestioneFile.CreateChat(telefonoAttivo, telefonoDestinatario))
                    {
                        risposta = "Chat creata con successo.";
                    }
                    else
                    {
                        risposta = "Errore: chat già esistente.";
                    }
                    break;

                case "MES": // MES # telefonoDestinatario # corpoMessaggio # [fileAllegato]
                    if (telefonoAttivo == null) return "Errore: utente non autenticato.";
                    if (parti.Length < 3) return "Errore: dati mancanti.";

                    string destinatario = parti[1];
                    string testoMessaggio = parti[2];
                    string fileAllegato = parti.Length > 3 ? parti[3] : null;

                    Message nuovoMessaggio = new Message(telefonoAttivo, destinatario, testoMessaggio, fileAllegato);
                    GestioneFile.AddMessage(telefonoAttivo, destinatario, nuovoMessaggio);

                    risposta = "Messaggio inviato.";
                    break;

                case "DEL": // DEL # telefonoDestinatario
                    if (telefonoAttivo == null) return "Errore: utente non autenticato.";
                    if (parti.Length < 2) return "Errore: dati mancanti.";

                    string telDaEliminare = parti[1];
                    if (GestioneFile.DeleteChat(telefonoAttivo, telDaEliminare))
                    {
                        risposta = "Chat eliminata con successo.";
                    }
                    else
                    {
                        risposta = "Errore: chat non trovata.";
                    }
                    break;

                case "UPL": // UPL # telefonoDestinatario # dimensioneFile # nomefile # datiBase64
                    if (telefonoAttivo == null)
                        return "Errore: utente non autenticato.";

                    if (parti.Length < 4)
                        return "Errore: dati mancanti.";

                    string telDestFile = parti[1];

                    // Controlla che la dimensione sia un numero valido
                    if (!int.TryParse(parti[2], out int dimensioneFile) || dimensioneFile <= 0)
                        return "Errore: dimensione file non valida.";

                    string nomefile = parti[3];

                    // Ricostruisci i dati Base64
                    int indiceDati = comando.IndexOf(nomefile) + nomefile.Length + 1;
                    if (indiceDati >= comando.Length)
                        return "Errore: dati del file mancanti.";

                    string datiBase64 = comando.Substring(indiceDati); // Prendi tutto il resto come dati
                    byte[] datiFile;

                    try
                    {
                        datiFile = Convert.FromBase64String(datiBase64);
                    }
                    catch (FormatException)
                    {
                        return "Errore: dati del file non validi (Base64 corrotto).";
                    }

                    // Controlla se la dimensione effettiva corrisponde a quella dichiarata
                    if (datiFile.Length != dimensioneFile)
                        return "Errore: dimensione file non corrispondente.";

                    if (GestioneFile.UploadFile(telefonoAttivo, telDestFile, nomefile, datiFile))
                    {
                        risposta = "File caricato con successo.";
                    }
                    else
                    {
                        risposta = "Errore nel caricamento del file.";
                    }
                    break;


                case "GET": // GET # telefonoDestinatario # nomefile
                    if (telefonoAttivo == null) return "Errore: utente non autenticato.";
                    if (parti.Length < 3) return "Errore: dati mancanti.";

                    string telFile = parti[1];
                    string nomeFileRichiesto = parti[2];

                    byte[] fileBytes = GestioneFile.GetFileContent(telefonoAttivo, telFile, nomeFileRichiesto);
                    string nomeFileCompleto = GestioneFile.GetNomeFileCompleto(telefonoAttivo, telFile, nomeFileRichiesto);

                    if (fileBytes != null)
                    {
                        // Codifica il contenuto del file in Base64
                        string fileBase64 = Convert.ToBase64String(fileBytes);

                        // Converti la lunghezza del file in Base64 (necessario per il client)
                        //byte[] fileLengthBytes = BitConverter.GetBytes(fileBase64.Length);

                        risposta = $"{fileBytes.Length}#{nomeFileCompleto}#{fileBase64}";
                    }
                    else
                    {
                        risposta = "Errore: file non trovato.";
                    }
                    break;




                case "CHA": // CHA # telefonoDestinatario
                    if (telefonoAttivo == null) return "Errore: utente non autenticato.";
                    if (parti.Length < 2) return "Errore: dati mancanti.";

                    string telChat = parti[1];
                    List<Message> messaggi = GestioneFile.ReadMessages(telefonoAttivo, telChat);

                    if (messaggi.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var msg in messaggi)
                        {
                            // Formattazione base del messaggio
                            sb.Append($"{msg.DataOra}|{GestioneFile.GetUser(msg.TelefonoMittente)}|{msg.CorpoMessaggio.Length}|{msg.CorpoMessaggio}");

                            // Aggiungi il file allegato se presente
                            if (!string.IsNullOrEmpty(msg.FileAllegato))
                            {
                                sb.Append($"|{msg.FileAllegato}");
                            }

                            sb.Append("|"); // Delimitatore finale per separare i messaggi
                            sb.AppendLine(); // Nuova riga per separare i messaggi nel buffer
                        }
                        risposta = sb.ToString();
                    }
                    else
                    {
                        risposta = "Nessun messaggio trovato.";
                    }
                    break;


                case "LOA": // LOA
                    if (telefonoAttivo == null) return "Errore: utente non autenticato.";

                    if (GestioneFile.GetAllChats(telefonoAttivo).Length > 0)
                        risposta = GestioneFile.GetAllChats(telefonoAttivo);
                    else
                        risposta = "Nessuna chat esistente.";
                    break;

                case "GUS": // GUS # nickname
                    if (telefonoAttivo == null) return "Errore: utente non autenticato.";

                    risposta = GestioneFile.GetUserByNick(parti[1]);
                    break;

                case "GU2": // GU2 # telefonoDestinatario
                    if (telefonoAttivo == null) return "Errore: utente non autenticato.";

                    risposta = GestioneFile.GetUser(parti[1]);
                    break;

                case "CLOSE": // CLOSE
                    if (client != null)
                    {
                        Console.WriteLine($"Il client {telefonoAttivo} si è disconnesso.");
                        client.Close();
                        client = null;
                    }
                    risposta = "Connessione chiusa dal client.";
                    break;

            }

            return risposta;
        }
    }
}
