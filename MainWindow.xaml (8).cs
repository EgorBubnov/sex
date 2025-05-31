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
        // Все доминошки
        public List<Tile> allTilles;
        public List<Tile> TilesOnCanvas;
        
        // Игроки
        public Player player1;
        public Player player2;
        
        // Игровой движок
        GameEngine engine = new GameEngine();
        
        // Защита от двойного клика
        private DateTime _lastClickTime = DateTime.MinValue;
        
        // Медиа плеер
        private MediaPlayer _musicPlayer;
        private bool _musicPaused = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindowSettings();
            InitializeMusicPlayer();
        }

        private void InitializeWindowSettings()
        {
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void InitializeMusicPlayer()
        {
            _musicPlayer = new MediaPlayer();
            _musicPlayer.Volume = 0.3;
            _musicPlayer.Open(new Uri("Resource/MUSIC.mp3", UriKind.Relative));
            _musicPlayer.MediaEnded += MusicPlayer_MediaEnded;
            _musicPlayer.Play();
            
            // Инициализация изображений для кнопки звука
            imageON.Source = bitmapON;
            imageOFF.Source = bitmapOFF;
        }

        // Кнопка создания новой игры  
        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            var namesWindow = new PlayerNamesWindow();
            var result = namesWindow.ShowDialog();

            if (result != true)
            {
                return; // Просто выходим, не закрывая приложение
            }

            InitializeNewGame(namesWindow);
        }

        private void InitializeNewGame(PlayerNamesWindow namesWindow)
        {
            // Создаем игроков
            player1 = new Player { name = namesWindow.Player1Name };
            player2 = new Player { name = namesWindow.Player2Name };
            engine.PlayerNOW = player1;
            CurrentPlayerText.Text = player1.name;

            // Очищаем предыдущую игру
            ClearGameState();

            // Инициализируем новую игру
            engine.mainw(this);
            allTilles = GenerateAllTiles();
            ShuffleTiles(allTilles);

            // Раздаем костяшки игрокам
            DealTilesToPlayers();
            
            // Отрисовываем руку текущего игрока
            engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
            BoneyardCountText.Text = allTilles.Count.ToString();
            
            // Начинаем игру
            engine.GameStart(this);
        }

        private void ClearGameState()
        {
            if (allTilles != null) allTilles.Clear();
            if (player1?.hand != null) player1.hand.Clear();
            if (player2?.hand != null) player2.hand.Clear();
            HandPlayer.Children.Clear();
            GameBoard.Children.Clear();
            engine.Clicked_rectangle = false;
            allTilles = new List<Tile>();
            TilesOnCanvas = new List<Tile>();
        }

        private List<Tile> GenerateAllTiles()
        {
            var tiles = new List<Tile>();
            for (int i = 0; i <= 6; ++i)
            {
                for (int j = i; j <= 6; ++j) // Изменено для генерации уникальных костяшек
                {
                    Tile tile = new Tile();
                    tile.value1 = i;
                    tile.value2 = j;
                    tile.direction = 1;
                    tiles.Add(tile);
                }
            }
            return tiles;
        }

        private void ShuffleTiles(List<Tile> tiles)
        {
            Random rng = new Random();
            int n = tiles.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (tiles[k], tiles[n]) = (tiles[n], tiles[k]);
            }
        }

        private void DealTilesToPlayers()
        {
            engine.GiveHandPlayer(ref player1, ref allTilles);
            engine.GiveHandPlayer(ref player2, ref allTilles);
        }

        // Метод дает игроку дополнительную кость
        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            if (allTilles.Count == 0)
            {
                MessageBox.Show("Базар пуст!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DrawTileForCurrentPlayer();
        }

        private void DrawTileForCurrentPlayer()
        {
            engine.PlayerNOW.hand.Add(allTilles[0]);
            HandPlayer.Children.Clear();
            engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
            allTilles.RemoveAt(0);
            BoneyardCountText.Text = allTilles.Count.ToString();
            engine.Skip_move_count = 0; // Сброс счетчика пропусков при взятии костяшки
        }

        // Метод смены игрока при нажатии на кнопку
        private void PassButton_Click(object sender, RoutedEventArgs e)
        {
            if (engine.Skip_move_count >= 1) // Проверяем, был ли предыдущий ход пропущен
            {
                EndGameBySkip();
                return;
            }

            SwitchPlayer();
            engine.Skip_move_count++;
        }

        private void EndGameBySkip()
        {
            int score1 = CalculatePlayerScore(player1);
            int score2 = CalculatePlayerScore(player2);

            string winnerName = score1 < score2 ? player1.name : player2.name;
            MessageBox.Show($"Игра окончена! Победил {winnerName} с наименьшим количеством очков: {Math.Min(score1, score2)}", 
                            "Конец игры", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);

            // Можно добавить сохранение статистики здесь
        }

        private int CalculatePlayerScore(Player player)
        {
            return player.hand.Sum(tile => tile.value1 + tile.value2);
        }

        // Метод смены игрока (используется в движке игры)
        public void PassButton_Click()
        {
            SwitchPlayer();
            engine.Skip_move_count = 0;
        }

        private void SwitchPlayer()
        {
            HandPlayer.Children.Clear();
            engine.PlayerNOW = engine.PlayerNOW == player1 ? player2 : player1;
            CurrentPlayerText.Text = engine.PlayerNOW.name;
            engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
            engine.GameStart(this);
        }

        // Кнопка выхода из программы
        private void ExiteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода", 
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        // Нажатие левой кнопки мыши по канвасу
        public bool CanvasLeftClickedFLAG = false;
        private void CanvasLeftClicked(object sender, MouseButtonEventArgs e)
        {
            if (CanvasLeftClickedFLAG) return;
            
            // Защита от двойного клика
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 300)
                return;

            _lastClickTime = DateTime.Now;

            engine.CanvasLeftClicked(sender, e);
        }

        // Нажатие правой кнопки мыши по канвасу
        private void CanvasRightClicked(object sender, MouseButtonEventArgs e)
        {
            // Защита от двойного клика
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 300)
                return;

            _lastClickTime = DateTime.Now;

            engine.CanvasRightClicked(sender, e, this);
        }

        public void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            engine.Rectangle_MouseDown(sender, e);
        }

        // Управление музыкой
        private readonly Image imageON = new Image { Width = 50, Height = 50, Stretch = Stretch.Uniform };
        private readonly BitmapImage bitmapON = new BitmapImage(new Uri("/Resource/SOUNDON.png", UriKind.Relative));
        private readonly Image imageOFF = new Image { Width = 50, Height = 50, Stretch = Stretch.Uniform };
        private readonly BitmapImage bitmapOFF = new BitmapImage(new Uri("/Resource/SOUNDOFF.png", UriKind.Relative));

        private void ChangeModeMusic(object sender, RoutedEventArgs e)
        {
            if (_musicPaused)
            {
                PauseMusic.Source = bitmapON;
                _musicPlayer.Play();
                _musicPaused = false;
            }
            else
            {
                PauseMusic.Source = bitmapOFF;
                _musicPlayer.Pause();
                _musicPaused = true;
            }
        }

        // Метод зацикливания музыки
        private void MusicPlayer_MediaEnded(object sender, EventArgs e)
        {
            _musicPlayer.Position = TimeSpan.Zero;
            _musicPlayer.Play();
        }

        // Кнопка открытия статистики
        private void StatsButton_Click(object sender, RoutedEventArgs e)
        {
            var statsWindow = new StatsWindow();
            
            // TODO: Добавить загрузку реальной статистики
            statsWindow.AddStatItem($"{player1?.name ?? "Игрок 1"}: 0 побед");
            statsWindow.AddStatItem($"{player2?.name ?? "Игрок 2"}: 0 побед");
            
            statsWindow.ShowDialog();
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
        // 1 - верх, 2 - право, 3 - вниз, 4 - влево
        public int direction;
        public int end_tile;
        
        public bool IsDouble()
        {
            return value1 == value2;
        }
    }

    // Класс игрока
    public class Player
    {
        public string name;
        public List<Tile> hand = new List<Tile>();
        
        public int Score => hand.Sum(tile => tile.value1 + tile.value2);
    }
}
