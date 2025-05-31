using System.Windows;

namespace DOMINO
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Метод для добавления элемента статистики (будет дополнен после реализации JSON)
        public void AddStatisticItem(string gameInfo)
        {
            var textBlock = new TextBlock
            {
                Text = gameInfo,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 5, 0, 5),
                TextWrapping = TextWrapping.Wrap
            };
            StatisticsContainer.Children.Add(textBlock);
        }
    }
}
