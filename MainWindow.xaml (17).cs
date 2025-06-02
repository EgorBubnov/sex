using DOMINO.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DOMINO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Все доминошки
        public List<Tile> allTilles;
        public List<Tile> TilesOnCanvas;
        //Данный ход игрока
        public Player player1;
        public Player player2;
        //Подключение игрового движка 
        GameEngine engine = new GameEngine();
        //Убирание дабл клика после нажатия
        private DateTime _lastClickTime = DateTime.MinValue;
        //Медиа плеер
        private MediaPlayer _musicPlayer;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            GameBoard.MouseLeftButtonDown += CanvasLeftClicked;
            GameBoard.MouseRightButtonDown += CanvasRightClicked;
            //Добавление плеера
            _musicPlayer = new MediaPlayer();
            _musicPlayer.Volume = 0.3;
            _musicPlayer.Open(new Uri("Resource/MUSIC.mp3", UriKind.Relative));
            _musicPlayer.MediaEnded += MusicPlayer_MediaEnded;
            _musicPlayer.Play();
        }

        //Кнопка создания новой игры  
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

            if (allTilles != null) allTilles.Clear();
            if (player1.hand.Count != 0) player1.hand.Clear();
            if (player2.hand.Count != 0) player2.hand.Clear();
            HandPlayer.Children.Clear();
            GameBoard.Children.Clear();
            engine.mainw(this);
            engine.Clicked_rectangle = false;
            allTilles = new List<Tile>();
            TilesOnCanvas = new List<Tile>();
            for (int i = 0; i <= 6; ++i)
            {
                for (int j = 0; j <= 6; ++j)
                {
                    Tile tile = new Tile();
                    tile.value1 = i; tile.value2 = j; tile.direction = 1;
                    allTilles.Add(tile);
                }
            }
            Shuffle<Tile>(allTilles);
            //Раздача костяшек игрокам
            engine.GiveHandPlayer(ref player1, ref allTilles);
            engine.GiveHandPlayer(ref player2, ref allTilles);
            //Отрисовка костяшек игрока 1
            engine.DrawHandTile(ref player1, ref HandPlayer);
            BoneyardCountText.Text = allTilles.Count.ToString();
            engine.GameStart(this);
        }

        //Метод перемешивает элементы в List
        public static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count();
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]); // Обмен значениями
            }
        }

        //Метод дает игроку дополнительную кость
        public void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            if (player1 == null) { return; }
            if (allTilles.Count != 0)
            {
                engine.PlayerNOW.hand.Add(allTilles[0]);
                HandPlayer.Children.Clear();
                engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
                allTilles.RemoveAt(0);
                BoneyardCountText.Text = allTilles.Count.ToString();
            }
        }

        //Метод смены игрока при нажатии на кнопку
        public void PassButton_Click(object sender, RoutedEventArgs e)
        {
            if (player1 == null) return;
            if (engine.Skip_move_count >= 2)
            {
                int score1 = player1.hand.Sum(tile => tile.value1 + tile.value2);
                int score2 = player2.hand.Sum(tile => tile.value1 + tile.value2);
                
                string winnerName = score1 < score2 ? player1.name : player2.name;
                string gameResult = $"{player1.name}: {score1} очков\n{player2.name}: {score2} очков\nПобедитель: {winnerName}";
                
                // Добавляем результат в статистику
                var statsWindow = new StatsWindow();
                statsWindow.AddStatItem(gameResult);
                statsWindow.ShowDialog();
                
                return;
            }

            HandPlayer.Children.Clear();
            engine.PlayerNOW = engine.PlayerNOW == player1 ? player2 : player1;
            engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
            engine.Skip_move_count++;
            engine.GameStart(this);
        }

        //Метод смены игрока при нажатии на кнопку, для использования в движке игры
        public void PassButton_Click()
        {
            HandPlayer.Children.Clear();
            engine.PlayerNOW = engine.PlayerNOW == player1 ? player2 : player1;
            engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
            engine.Skip_move_count = 0;
            engine.GameStart(this);
        }

        //Кнопка выхода из программы
        public void ExiteButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //Нажатие левой кнопки мышью по канвасу
        public bool CanvasLeftClickedFLAG = false;
        public void CanvasLeftClicked(object sender, MouseButtonEventArgs e)
        {
            if (CanvasLeftClickedFLAG) return;
            //Защита от дабл клика
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 300)
                return;

            _lastClickTime = DateTime.Now;

            engine.CanvasLeftClicked(sender, e);
        }

        //Нажатие правой кнопки мышью по канвасу
        public void CanvasRightClicked(object sender, MouseButtonEventArgs e)
        {
            //Защита от дабл клика
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 300)
                return;

            _lastClickTime = DateTime.Now;

            engine.CanvasRightClicked(sender, e, this);
        }

        public void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            engine.Rectangle_MouseDown(sender, e);
        }

        //Изменение режима музыки
        Image imageON = new Image { Width = 50, Height = 50, Stretch = Stretch.Uniform };
        BitmapImage bitmapON = new BitmapImage(new Uri("/Resource/SOUNDON.png", UriKind.Relative));
        Image imageOFF = new Image { Width = 50, Height = 50, Stretch = Stretch.Uniform };
        BitmapImage bitmapOFF = new BitmapImage(new Uri("/Resource/SOUNDOFF.png", UriKind.Relative));
        public void ChangeModeMusic(object sender, RoutedEventArgs e)
        {
            if (PauseMusic.Source == bitmapOFF)
            {
                PauseMusic.Source = bitmapON;
                _musicPlayer.Play();
            }
            else
            {
                PauseMusic.Source = bitmapOFF;
                _musicPlayer.Pause();
            }
        }

        //Метод циклит музыку
        private void MusicPlayer_MediaEnded(object sender, EventArgs e)
        {
            _musicPlayer.Position = TimeSpan.Zero;
            _musicPlayer.Play();
        }

        //Кнопка показа статистики
        private void StatsButton_Click(object sender, RoutedEventArgs e)
        {
            var statsWindow = new StatsWindow();
            
            if (player1 != null && player2 != null)
            {
                int score1 = player1.hand.Sum(tile => tile.value1 + tile.value2);
                int score2 = player2.hand.Sum(tile => tile.value1 + tile.value2);
                
                statsWindow.AddStatItem($"Текущая игра:");
                statsWindow.AddStatItem($"{player1.name}: {score1} очков");
                statsWindow.AddStatItem($"{player2.name}: {score2} очков");
            }
            
            statsWindow.ShowDialog();
        }
    }

    //Класс кости
    public class Tile
    {
        public int value1;
        public int value2;
        public int width = 30;
        public int height = 60;
        public Canvas rectangle = new Canvas();
        //1 - верх, 2 - право, 3 -вниз, 4 -влево
        public int direction;
        public int end_tile;
    }

    //Класс игрока
    public class Player
    {
        public string name;
        public List<Tile> hand = new List<Tile>();        
    }
}
