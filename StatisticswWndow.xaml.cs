using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DOMINO
{
    /// <summary>
    /// Логика взаимодействия для StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Добавляет текстовую строку в список статистики
        /// </summary>
        /// <param name="text">Текст строки</param>
        public void AddStatistic(string text)
        {
            var statText = new TextBlock
            {
                Text = text,
                FontSize = 14,
                Margin = new Thickness(5),
                Foreground = Brushes.Black
            };
            StatsStackPanel.Children.Add(statText);
        }
    }
}
