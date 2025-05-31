using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DOMINO
{
    public partial class StatsWindow : Window
    {
        public StatsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void AddStatItem(string gameInfo)
        {
            var textBlock = new TextBlock
            {
                Text = gameInfo,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 5, 0, 5),
                TextWrapping = TextWrapping.Wrap
            };
            StatsContainer.Children.Add(textBlock);
        }
    }
}
