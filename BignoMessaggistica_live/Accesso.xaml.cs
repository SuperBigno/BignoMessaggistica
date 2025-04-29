using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BignoMessaggistica_live
{
    public partial class Accesso : UserControl
    {
        private MainWindow mainWindow;
        private string telefono;
        private string nickname;

        public Accesso(MainWindow mw)
        {
            InitializeComponent();
            mainWindow = mw;
        }

        private void tb_tel_TextChanged(object sender, TextChangedEventArgs e)
        {
            telefono = tb_tel.Text;
        }

        private void tb_nick_TextChanged(object sender, TextChangedEventArgs e)
        {
            nickname = tb_nick.Text;
        }

        private void bt_reg_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(telefono) || string.IsNullOrWhiteSpace(nickname))
            {
                MessageBox.Show("Inserisci telefono e nickname.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string risposta = mainWindow.RegistraUtente(telefono, nickname);
            MessageBox.Show(risposta, "Registrazione", MessageBoxButton.OK, MessageBoxImage.Information);
            if (risposta == "Registrazione completata.")
            {
                tb_nick.Clear();
                tb_tel.Clear();
            }
        }

        private void bt_log_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                MessageBox.Show("Inserisci il numero di telefono.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string risposta = mainWindow.LoginUtente(telefono);
            if (risposta== "Accesso effettuato.")
            {
                mainWindow.Content = new Dashboard(mainWindow, telefono);
            }
        }

        private void tb_tel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bt_log_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
