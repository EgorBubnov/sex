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
    public partial class MainWindow : Window
    {
        public List<Tile> allTilles;
        public List<Tile> TilesOnCanvas;
        
        public Player player1;
        public Player player2;
        
        GameEngine engine = new GameEngine();
        private DateTime _lastClickTime = DateTime.MinValue;
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
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            var namesWindow = new PlayerNamesWindow();
            var result = namesWindow.ShowDialog();

            if (result != true) return;

            InitializeNewGame(namesWindow);
        }

        private void InitializeNewGame(PlayerNamesWindow namesWindow)
        {
            player1 = new Player { name = namesWindow.Player1Name };
            player2 = new Player { name = namesWindow.Player2Name };
            engine.PlayerNOW = player1;
            CurrentPlayerText.Text = player1.name;

            ClearGameState();

            engine.mainw(this);
            allTilles = GenerateAllTiles();
            ShuffleTiles(allTilles);

            DealTilesToPlayers();
            
            engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
            BoneyardCountText.Text = allTilles.Count.ToString();
            
            engine.GameStart(this);
        }

        private void ClearGameState()
        {
            allTilles?.Clear();
            player1?.hand?.Clear();
            player2?.hand?.Clear();
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
                for (int j = i; j <= 6; ++j)
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
            engine.Skip_move_count = 0;
        }

        private void PassButton_Click(object sender, RoutedEventArgs e)
        {
            if (engine.Skip_move_count >= 1)
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
        }

        private int CalculatePlayerScore(Player player)
        {
            return player?.hand?.Sum(tile => tile.value1 + tile.value2) ?? 0;
        }

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

        private void ExiteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода", 
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        public bool CanvasLeftClickedFLAG = false;
        private void CanvasLeftClicked(object sender, MouseButtonEventArgs e)
        {
            if (CanvasLeftClickedFLAG) return;
            
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 300)
                return;

            _lastClickTime = DateTime.Now;

            engine.CanvasLeftClicked(sender, e);
        }

        private void CanvasRightClicked(object sender, MouseButtonEventArgs e)
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

        private void ChangeModeMusic(object sender, RoutedEventArgs e)
        {
            if (_musicPaused)
            {
                PauseMusic.Source = new BitmapImage(new Uri("/Resource/SOUNDON.png", UriKind.Relative));
                _musicPlayer.Play();
                _musicPaused = false;
            }
            else
            {
                PauseMusic.Source = new BitmapImage(new Uri("/Resource/SOUNDOFF.png", UriKind.Relative));
                _musicPlayer.Pause();
                _musicPaused = true;
            }
        }

        private void MusicPlayer_MediaEnded(object sender, EventArgs e)
        {
            _musicPlayer.Position = TimeSpan.Zero;
            _musicPlayer.Play();
        }

        private void StatsButton_Click(object sender, RoutedEventArgs e)
        {
            var statsWindow = new StatsWindow();
            
            // Добавляем текущую статистику (можно заменить на загрузку из файла)
            if (player1 != null)
                statsWindow.AddStatItem($"{player1.name}: {player1.Score} очков");
            if (player2 != null)
                statsWindow.AddStatItem($"{player2.name}: {player2.Score} очков");
            
            statsWindow.ShowDialog();
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
        
        public bool IsDouble()
        {
            return value1 == value2;
        }
    }

    public class Player
    {
        public string name;
        public List<Tile> hand = new List<Tile>();
        
        public int Score => hand?.Sum(tile => tile.value1 + tile.value2) ?? 0;
    }
}
