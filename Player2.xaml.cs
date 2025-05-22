using System.Windows;

namespace DOMINO
{
    public partial class PlayerNamesWindow : Window
    {
        public string Player1Name { get; private set; } = "Игрок 1";
        public string Player2Name { get; private set; } = "Игрок 2";

        public PlayerNamesWindow()
        {
            InitializeComponent();
            Player1TextBox.Focus();
            Player1TextBox.SelectAll();
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Устанавливаем имена игроков, заменяя пустые значения на "Игрок 1/2"
            Player1Name = string.IsNullOrWhiteSpace(Player1TextBox.Text) 
                ? "Игрок 1" 
                : Player1TextBox.Text.Trim();
            
            Player2Name = string.IsNullOrWhiteSpace(Player2TextBox.Text) 
                ? "Игрок 2" 
                : Player2TextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Player1TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Player2TextBox.Focus();
                Player2TextBox.SelectAll();
            }
        }

        private void Player2TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartGameButton_Click(sender, e);
            }
        }
    }
}
