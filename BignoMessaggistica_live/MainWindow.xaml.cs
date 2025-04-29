using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace BignoMessaggistica_live
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 21212;
        int buffer_dim = 2097152;

        public MainWindow()
        {
            InitializeComponent();
            ConnettiAlServer();
            Content = new Accesso(this);
        }

        private void ConnettiAlServer()
        {
            try
            {
                client = new TcpClient(SERVER_IP, SERVER_PORT);
                stream = client.GetStream();
                Console.WriteLine("Connesso al server!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore di connessione al server: " + ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string InviaComando(string comando)
        {
            try
            {
                if (client == null || !client.Connected)
                    return "Errore: non connesso al server.";

                byte[] dati = Encoding.ASCII.GetBytes(comando);
                stream.Write(dati, 0, dati.Length);

                byte[] buffer = new byte[buffer_dim];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                return Encoding.ASCII.GetString(buffer, 0, bytesRead);
            }
            catch (Exception ex)
            {
                return "Errore di comunicazione: " + ex.Message;
            }
        }

        public string RegistraUtente(string telefono, string nickname)
        {
            string comando = $"REG#{telefono}#{nickname}";
            return InviaComando(comando);
        }

        public string LoginUtente(string telefono)
        {
            string comando = $"LOG#{telefono}";
            return InviaComando(comando);
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            try
            {
                InviaComando("CLOSE");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la chiusura della connessione: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ChiudiConnessione();
            }
        }

        private void ChiudiConnessione()
        {
            try
            {
                if (stream != null) stream.Close();
                if (client != null) client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la chiusura della connessione: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string InviaMessaggio(string destinatario, string messaggio, string allegato)
        {
            string comando;

            if (allegato!=null)
                comando = $"MES#{destinatario}#{messaggio}#{allegato}";
            else
                comando = $"MES#{destinatario}#{messaggio}";


            return InviaComando(comando);
        }

        public string CreaChat(string telefonoDestinatario)
        {
            string comando = $"CRE#{telefonoDestinatario}";
            return InviaComando(comando);
        }

        public string EliminaChat(string telefonoDestinatario)
        {
            string comando = $"DEL#{telefonoDestinatario}";
            return InviaComando(comando);
        }

        public string CaricaUtenti(string telefono)
        {
            string comando = $"LOA#{telefono}";
            return InviaComando(comando);
        }

        public string CaricaChat(string telefonoDestinatario)
        {
            string comando = $"CHA#{telefonoDestinatario}";
            return InviaComando(comando);
        }

        public void EffettuaLogout()
        {
            string risposta = InviaComando("OUT");
            MessageBox.Show(risposta);
            Content = new Accesso(this);
        }

        public string GetTelefono(string nickname)
        {
            string comando = $"GUS#{nickname}";
            return InviaComando(comando);
        }

        public string GetNickname(string telefono)
        {
            string comando = $"GU2#{telefono}";
            return InviaComando(comando);
        }

        public string CaricaAllegato(string telefonoDestinatario, string path) 
        {
            byte[] fileBytes = File.ReadAllBytes(path);
            string fileBase64 = Convert.ToBase64String(fileBytes);

            if (fileBytes.Length > buffer_dim)
                return "Attenzione: superata la dimensione limite per gli allegati (2MB)";

            string comando = $"UPL#{telefonoDestinatario}#{fileBytes.Length}#{Path.GetFileName(path)}#{fileBase64}";
            return InviaComando(comando);
        }


        public void ScaricaFile(string telefonoDestinatario, string nomeFile)
        {
            try
            {
                // Invia il comando GET al server
                string risposta = InviaComando($"GET#{telefonoDestinatario}#{nomeFile}");


                if (risposta.StartsWith("Errore"))
                {
                    MessageBox.Show(risposta, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Divide la risposta per estrarre lunghezza e nome file
                string[] partiRisposta = risposta.Split('#');
                if (partiRisposta.Length < 3)
                {
                    MessageBox.Show("Risposta del server non valida.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Recupera lunghezza file
                if (!int.TryParse(partiRisposta[0], out int lunghezzaFile) || lunghezzaFile <= 0)
                {
                    MessageBox.Show("Lunghezza del file non valida.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Recupera il nome file
                string nomeFileCompleto = partiRisposta[1];

                // Determina l'indice di inizio dei dati Base64
                int indiceDati = risposta.IndexOf(nomeFileCompleto) + nomeFileCompleto.Length + 1;
                if (indiceDati >= risposta.Length)
                {
                    MessageBox.Show("Errore: dati del file mancanti.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Recupera i dati Base64
                string fileBase64 = risposta.Substring(indiceDati);

                // Decodifica Base64
                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(fileBase64);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Errore nella decodifica Base64 del file.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Controllo sulla lunghezza effettiva dei byte ricevuti
                if (fileBytes.Length != lunghezzaFile)
                {
                    MessageBox.Show("Errore: dimensione del file non corrispondente.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Compongo il nome puro e l'estensione del file per il salvataggio corretto
                List<string> pezzettini = nomeFileCompleto.Split('.').ToList();
                string estensione, nomePuro;

                estensione = pezzettini[pezzettini.Count-1];
                pezzettini.Remove(estensione);
                nomePuro = string.Join(".", pezzettini);

                string descrizioneFiltro = $"File {estensione.ToUpper()}";

                string filtro = $"{descrizioneFiltro} (*.{estensione})|*.{estensione}|Tutti i file (*.*)|*.*";

                // Finestra per il salvataggio del file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = nomePuro,
                    Filter = filtro,
                    DefaultExt = estensione,
                    AddExtension = true
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, fileBytes);
                    MessageBox.Show("File salvato con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il download del file: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }






    }
}
