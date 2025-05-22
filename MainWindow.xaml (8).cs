using DOMINO.Modules;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DOMINO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Все доминошки
        public List<Tile> allTilles;
        public List<Tile> TilesOnCanvas;
        
        // Данный ход игрока
        public int PlayerMove;
        public Player player1;
        public Player player2;
        
        // Подключение игрового движка
        GameEngine engine = new GameEngine();
        
        // Убирание дабл клика после нажатия
        private DateTime _lastClickTime = DateTime.MinValue;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            GameBoard.MouseLeftButtonDown += CanvasLeftClicked;
            GameBoard.MouseRightButtonDown += CanvasRightClicked;
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

        // Кнопка создания новой игры
        void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            var namesWindow = new PlayerNamesWindow();
            var result = namesWindow.ShowDialog();
            
            if (result != true)
            {
                return; // Просто выходим, не закрывая приложение
            }

            // Инициализация новой игры
            player1 = new Player { name = namesWindow.Player1Name };
            player2 = new Player { name = namesWindow.Player2Name };
            engine.PlayerNOW = player1;
            CurrentPlayerText.Text = player1.name;

            // Очистка перед новой игрой
            allTilles?.Clear();
            player1?.hand?.Clear();
            player2?.hand?.Clear();
            HandPlayer.Children.Clear();
            GameBoard.Children.Clear();

            engine.mainw(this);
            engine.Clicked_rectangle = false;
            allTilles = new List<Tile>();
            TilesOnCanvas = new List<Tile>();

            // Создание всех костяшек
            for (int i = 0; i <= 6; ++i)
            {
                for (int j = i; j <= 6; ++j)
                {
                    allTilles.Add(new Tile { value1 = i, value2 = j, direction = 1 });
                }
            }

            Shuffle(allTilles);
            engine.GiveHandPlayer(ref player1, ref allTilles);
            engine.GiveHandPlayer(ref player2, ref allTilles);
            engine.DrawHandTile(ref player1, ref HandPlayer);
            BoneyardCountText.Text = allTilles.Count.ToString();
            engine.GameStart(this);
        }

        public static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реализовать функционал взятия из базара
        }

        public void PassButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реализовать функционал пропуска хода
        }

        public void ExiteButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void CanvasLeftClicked(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 300)
                return;

            _lastClickTime = DateTime.Now;
            engine.CanvasLeftClicked(sender, e, this);
        }

        public void CanvasRightClicked(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 300)
                return;

            _lastClickTime = DateTime.Now;
            engine.CanvasRightClicked(sender, e, this);
        }

        public void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            engine.Rectangle_MouseDown(sender, e);
        }
    }

    public class Tile
    {
        public int value1;
        public int value2;
        public int width = 30;
        public int height = 60;
        public Canvas rectangle = new Canvas();
        public int direction;
        public int end_tile;
    }

    public class Player
    {
        public string name;
        public List<Tile> hand = new List<Tile>();
    }
}
