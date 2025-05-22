using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DOMINO
{
    public partial class MainWindow : Window
    {
        public List<Tile> allTilles;
        public List<Tile> TilesOnCanvas;
        public int PlayerMove;
        public Player player1;
        public Player player2;
        GameEngine engine = new GameEngine();
        private DateTime _lastClickTime = DateTime.MinValue;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private bool ShowPlayerNamesWindow()
        {
            var namesWindow = new PlayerNamesWindow();
            if (namesWindow.ShowDialog() == true)
            {
                player1 = new Player { name = namesWindow.Player1Name };
                player2 = new Player { name = namesWindow.Player2Name };
                engine.PlayerNOW = player1;
                CurrentPlayerText.Text = player1.name;
                return true;
            }
            return false;
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowPlayerNamesWindow()) return;
            
            // Инициализация новой игры
            allTilles = new List<Tile>();
            TilesOnCanvas = new List<Tile>();
            HandPlayer.Children.Clear();
            GameBoard.Children.Clear();

            // Создание костяшек
            for (int i = 0; i <= 6; i++)
                for (int j = i; j <= 6; j++)
                    allTilles.Add(new Tile { value1 = i, value2 = j });

            Shuffle(allTilles);
            BoneyardCountText.Text = allTilles.Count.ToString();
        }

        public static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void ExiteButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }

    public class Tile
    {
        public int value1;
        public int value2;
        public int width = 30;
        public int height = 60;
        public Canvas rectangle = new Canvas();
        public int direction = 1;
        public int end_tile;
    }

    public class Player
    {
        public string name;
        public List<Tile> hand = new List<Tile>();
    }
}
