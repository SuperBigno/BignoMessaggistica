using System;
using System.Windows;

namespace BignoMessaggistica_live
{
    public partial class AddChatDialog : Window
    {
        public string NumeroTelefono { get; private set; }

        public AddChatDialog()
        {
            InitializeComponent();
        }

        // Evento per il pulsante "Aggiungi"
        private void AggiungiButton_Click(object sender, RoutedEventArgs e)
        {
            NumeroTelefono = TelefonoTextBox.Text;
            if (!string.IsNullOrEmpty(NumeroTelefono))
            {
                this.DialogResult = true;  // Chiudiamo la finestra e passiamo indietro il numero
            }
            else
            {
                MessageBox.Show("Per favore, inserisci un numero di telefono valido.");
            }
        }

        // Evento per il pulsante "Annulla"
        private void AnnullaButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;  // Chiudiamo la finestra senza passare nulla
        }
    }
}
