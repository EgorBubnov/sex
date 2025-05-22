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
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            Player1Name = string.IsNullOrWhiteSpace(Player1TextBox.Text) ? "Игрок 1" : Player1TextBox.Text.Trim();
            Player2Name = string.IsNullOrWhiteSpace(Player2TextBox.Text) ? "Игрок 2" : Player2TextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
