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
            InitializeWindowSettings();
            SetupEventHandlers();
        }

        private void InitializeWindowSettings()
        {
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void SetupEventHandlers()
        {
            GameBoard.MouseLeftButtonDown += CanvasLeftClicked;
            GameBoard.MouseRightButtonDown += CanvasRightClicked;
        }

        // Показываем окно ввода имени
        private bool ShowPlayerNamesWindow()
        {
            var namesWindow = new PlayerNamesWindow();
            if (namesWindow.ShowDialog() == true)
            {
                InitializePlayers(namesWindow);
                return true;
            }
            return false;
        }

        private void InitializePlayers(PlayerNamesWindow namesWindow)
        {
            player1 = new Player { name = namesWindow.Player1Name };
            player2 = new Player { name = namesWindow.Player2Name };
            engine.PlayerNOW = player1;
            CurrentPlayerText.Text = player1.name;
        }

        // Кнопка создания новой игры
        void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!StartNewGame()) return;
            
            InitializeGameState();
            CreateAllTiles();
            ShuffleAndDealTiles();
            StartGame();
        }

        private bool StartNewGame()
        {
            if (!ShowPlayerNamesWindow())
            {
                return false;
            }
            return true;
        }

        private void InitializeGameState()
        {
            // Связь данных о игроке с движком
            ref Player playerNOW = ref engine.PlayerNOW;
            playerNOW = player1;

            ClearPreviousGame();
            engine.mainw(this);
            engine.Clicked_rectangle = false;
        }

        private void ClearPreviousGame()
        {
            allTilles?.Clear();
            player1?.hand?.Clear();
            player2?.hand?.Clear();
            HandPlayer.Children.Clear();
            GameBoard.Children.Clear();
            
            allTilles = new List<Tile>();
            TilesOnCanvas = new List<Tile>();
        }

        private void CreateAllTiles()
        {
            for (int i = 0; i <= 6; ++i)
            {
                for (int j = i; j <= 6; ++j) // Изменено на j = i для правильных костяшек
                {
                    allTilles.Add(new Tile 
                    { 
                        value1 = i, 
                        value2 = j, 
                        direction = 1 
                    });
                }
            }
        }

        private void ShuffleAndDealTiles()
        {
            Shuffle(allTilles);
            engine.GiveHandPlayer(ref player1, ref allTilles);
            engine.GiveHandPlayer(ref player2, ref allTilles);
            engine.DrawHandTile(ref player1, ref HandPlayer);
            BoneyardCountText.Text = allTilles.Count.ToString();
        }

        private void StartGame()
        {
            engine.GameStart(this);
        }

        // Метод перемешивает элементы в List
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

        // На будущее
        public void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реализовать функционал взятия из базара
        }

        // На будущее
        public void PassButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реализовать функционал пропуска хода
        }

        // Кнопка выхода из программы
        public void ExiteButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Нажатие левой кнопки мышью по канвасу
        public void CanvasLeftClicked(object sender, MouseButtonEventArgs e)
        {
            if (IsDoubleClick()) return;
            _lastClickTime = DateTime.Now;
            engine.CanvasLeftClicked(sender, e, this);
        }

        // Нажатие правой кнопки мышью по канвасу
        public void CanvasRightClicked(object sender, MouseButtonEventArgs e)
        {
            if (IsDoubleClick()) return;
            _lastClickTime = DateTime.Now;
            engine.CanvasRightClicked(sender, e, this);
        }

        private bool IsDoubleClick()
        {
            return (DateTime.Now - _lastClickTime).TotalMilliseconds < 300;
        }

        public void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            engine.Rectangle_MouseDown(sender, e);
        }
    }

    // Класс костяшки домино
    public class Tile
    {
        public int value1;
        public int value2;
        public int width = 30;
        public int height = 60;
        public Canvas rectangle = new Canvas();
        
        // Направление: 1 - верх, 2 - право, 3 - вниз, 4 - влево
        public int direction;
        public int end_tile;
    }

    // Класс игрока
    public class Player
    {
        public string name;
        public List<Tile> hand = new List<Tile>();
    }
}
