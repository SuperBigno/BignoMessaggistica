using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace BignoMessaggistica_live
{
    /// <summary>
    /// Logica di interazione per Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        // Lista simulata di utenti
        private List<string> utenti = new List<string> ();
        private string chatSelezionata = string.Empty;
        private string telefono, nickname;
        MainWindow mainWindow;
        private string fileAllegato = null;

        public Dashboard(MainWindow mw, string telefono)
        {
            InitializeComponent();
            mainWindow = mw;
            this.telefono = telefono;
            nickname = mainWindow.GetNickname(telefono);
        }

        // Evento per la selezione di un utente dalla ListBox
        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserList.SelectedItem != null)
            {
                chatSelezionata = UserList.SelectedItem.ToString();
                ChatContent.Inlines.Clear();
                FormatChat(mainWindow.CaricaChat(mainWindow.GetTelefono(chatSelezionata)));
            }
        }

        private void FormatChat(string chatContents)
        {
            ChatContent.Inlines.Clear();

            if (chatContents != "Nessun messaggio trovato.")
            {
                string[] messages = chatContents.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var message in messages)
                {
                    string[] components = message.Split('|');

                    if (components.Length >= 4)
                    {
                        string data = components[0];
                        string mittente = components[1];
                        string corpoMessaggio = components[3];
                        string fileAllegato = components.Length > 4 ? components[4] : null;

                        // Parsing della data e formattazione con ora e minuti
                        DateTime dataMessaggio = DateTime.Parse(data);
                        string dataFormattata = dataMessaggio.ToString("d-MM-yyyy HH:mm"); // Aggiunta di ora e minuti

                        mittente = mittente == nickname ? "Tu" : mittente;
                        Brush color = mittente == "Tu" ? Brushes.Blue : Brushes.Green;

                        // Mostra la data e il mittente
                        ChatContent.Inlines.Add(new Run($"{dataFormattata} - ")); // Ora inclusa
                        ChatContent.Inlines.Add(new Run(mittente) { Foreground = color });
                        ChatContent.Inlines.Add(new Run(": "));

                        // Se il messaggio ha un corpo testuale, mostralo
                        if (!string.IsNullOrWhiteSpace(corpoMessaggio))
                        {
                            ChatContent.Inlines.Add(new Run(corpoMessaggio));
                        }

                        // Se c'è un file allegato, aggiungi un pulsante per il download
                        if (!string.IsNullOrEmpty(fileAllegato))
                        {
                            Button downloadButton = new Button
                            {
                                Content = $"📎 {fileAllegato}",
                                Margin = new Thickness(5, 0, 0, 0),
                                Background = Brushes.LightGray
                            };
                            downloadButton.Click += (sender, e) => mainWindow.ScaricaFile(mainWindow.GetTelefono(chatSelezionata), fileAllegato);

                            InlineUIContainer container = new InlineUIContainer(downloadButton);
                            ChatContent.Inlines.Add(container);
                        }

                        ChatContent.Inlines.Add(new Run("\n"));
                    }
                    else
                    {
                        ChatContent.Inlines.Add(new Run("Errore nel formato del messaggio.") { Foreground = Brushes.Red });
                    }
                }
            }
            else
            {
                ChatContent.Inlines.Add(new Run(chatContents));
            }
        }




        private void AllegaFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                fileAllegato = openFileDialog.FileName;
                FileAllegatoText.Text = "File allegato: " + Path.GetFileName(fileAllegato);
                FileAllegatoText.Visibility = Visibility.Visible;
            }
        }

        // Evento per inviare il messaggio
        private void InviaMessage_Click(object sender, RoutedEventArgs e)
        {
            string messaggio = MessageInput.Text.Trim();

            if (string.IsNullOrEmpty(messaggio) && string.IsNullOrEmpty(fileAllegato))
            {
                MessageBox.Show("Inserisci un messaggio o allega un file.");
                return;
            }

            string destinatario = mainWindow.GetTelefono(chatSelezionata);

            if (!string.IsNullOrEmpty(fileAllegato))
            {
                string ans = mainWindow.CaricaAllegato(destinatario, fileAllegato);
                if (ans.StartsWith("Attenzione"))
                {
                    MessageBox.Show(ans, "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

            if (!string.IsNullOrEmpty(messaggio) || !string.IsNullOrEmpty(fileAllegato))
            {
                mainWindow.InviaMessaggio(destinatario, messaggio, Path.GetFileName(fileAllegato));
                FormatChat(mainWindow.CaricaChat(destinatario));

                MessageInput.Clear();
                fileAllegato = null;
                FileAllegatoText.Visibility = Visibility.Collapsed;
            }
        }

        private void AggiungiChat_Click(object sender, RoutedEventArgs e)
        {
            AddChatDialog dialog = new AddChatDialog();
            bool? result = dialog.ShowDialog();

            if (result == true && !string.IsNullOrEmpty(dialog.NumeroTelefono))
            {
                string telefono = dialog.NumeroTelefono;
                mainWindow.CreaChat(telefono);
                LoadChat();
                UserList.ItemsSource = null;
                UserList.ItemsSource = utenti;
                if (utenti.Count > 0)
                    MessageInput.IsEnabled = true;
                else
                    MessageInput.IsEnabled = false;
                MessageBox.Show($"Chat con {telefono} aggiunta.");
            }
        }

        private void EliminaChat_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(chatSelezionata))
            {
                // Mostra una MessageBox di avvertimento per confermare l'eliminazione
                MessageBoxResult result = MessageBox.Show(
                    "Sei sicuro di voler eliminare questa chat? L'operazione è irreversibile.",
                    "Conferma eliminazione",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    string risposta = mainWindow.EliminaChat(mainWindow.GetTelefono(chatSelezionata));
                    LoadChat();
                    UserList.ItemsSource = null;
                    UserList.ItemsSource = utenti;

                    if (utenti.Count > 0)
                        MessageInput.IsEnabled = true;
                    else
                        MessageInput.IsEnabled = false;

                    chatSelezionata = string.Empty;
                    ChatContent.Text = "Seleziona una chat.";
                    MessageBox.Show(risposta, "Elimina chat", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Nessuna chat selezionata.");
            }
        }


        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.EffettuaLogout();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadChat();

            UserList.ItemsSource = null;
            UserList.ItemsSource = utenti;
        }

        private void Aggiorna_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem != null)
            {
                chatSelezionata = UserList.SelectedItem.ToString();
                ChatContent.Inlines.Clear();
                FormatChat(mainWindow.CaricaChat(mainWindow.GetTelefono(chatSelezionata)));
            }
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InviaMessage_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void LoadChat()
        {
            if (mainWindow.CaricaUtenti(telefono) == "Nessuna chat esistente.")
            {
                utenti = new List<string>();
                MessageInput.IsEnabled = false;
            }
            else
            {
                utenti = mainWindow.CaricaUtenti(telefono).Split('_').ToList();
                MessageInput.IsEnabled = true;
            }
        }
    }
}
