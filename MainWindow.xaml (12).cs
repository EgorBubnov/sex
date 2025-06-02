using DOMINO.Modules;
using System.Configuration.Internal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;

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
        //Список для хранения статистики игр
        private List<string> gameStats = new List<string>();

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

        // Метод для отображения статистики
        private void ShowStatsButton_Click(object sender, RoutedEventArgs e)
        {
            var statsWindow = new StatsWindow();
            
            // Добавляем все сохраненные записи статистики в окно
            foreach (var stat in gameStats)
            {
                statsWindow.AddStatItem(stat);
            }
            
            // Если статистика пуста, показываем сообщение
            if (gameStats.Count == 0)
            {
                statsWindow.AddStatItem("Статистика игр пока отсутствует");
            }
            
            statsWindow.ShowDialog();
        }

        // Метод для добавления записи о завершенной игре
        public void AddGameToStats(string winnerName, int player1Score, int player2Score)
        {
            string statEntry = $"{DateTime.Now:dd.MM.yyyy HH:mm} - {player1.name} ({player1Score}) vs {player2.name} ({player2Score}). Победитель: {winnerName}";
            gameStats.Add(statEntry);
        }

        // Метод для обновления статистики во время игры
        public void UpdateGameStats()
        {
            if (player1 == null || player2 == null) return;

            int player1Score = 0;
            int player2Score = 0;
            
            // Подсчет очков у игроков
            foreach (var tile in player1.hand)
            {
                player1Score += tile.value1 + tile.value2;
            }
            
            foreach (var tile in player2.hand)
            {
                player2Score += tile.value1 + tile.value2;
            }
            
            // Обновление текста статистики
            CurrentPlayerText.Text = $"{engine.PlayerNOW.name} (Ход: {engine.Skip_move_count + 1})";
            BoneyardCountText.Text = $"{allTilles.Count} | {player1.name}: {player1Score} | {player2.name}: {player2Score}";
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
            if (player1.hand.Count !=0) player1.hand.Clear();
            if (player2.hand.Count !=0) player2.hand.Clear();
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
            
            // Инициализация статистики при старте игры
            UpdateGameStats();
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

        //Метод дает игроку дополнительную  кость
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
                
                // Обновляем статистику после взятия кости
                UpdateGameStats();
            }
        }

        //Метод смены игрока при нажатии на кнопку
        public void PassButton_Click(object sender, RoutedEventArgs e)
        {
            if (player1 == null) return;
            
            // Обновляем статистику перед сменой хода
            UpdateGameStats();
            
            if (engine.Skip_move_count >= 2)
            {
                int scope1 = 0;
                int scope2 = 0;
                for (int i = 0; i <player1.hand.Count; i++)
                {
                    scope1 += player1.hand[i].value1;
                    scope1 += player1.hand[i].value2;
                }
                for (int i = 0; i < player2.hand.Count; i++)
                {
                    scope2 += player2.hand[i].value1;
                    scope2 += player2.hand[i].value2;
                }

                // Определяем победителя и добавляем запись в статистику
                string winner = scope1 <= scope2 ? player1.name : player2.name;
                AddGameToStats(winner, scope1, scope2);
                
                // Показываем сообщение о конце игры
                MessageBox.Show($"Игра окончена! Победитель: {winner}\n{player1.name}: {scope1} очков\n{player2.name}: {scope2} очков", 
                              "Конец игры", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
            
            HandPlayer.Children.Clear();
            engine.PlayerNOW = engine.PlayerNOW == player1 ? player2 : player1;
            engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
            engine.Skip_move_count++;
            engine.GameStart(this);
            
            // Обновляем статистику после смены хода
            UpdateGameStats();
        }

        //Метод смены игрока при нажатии на кнопку, для использования в движке игры
        public void PassButton_Click()
        {
            HandPlayer.Children.Clear();
            engine.PlayerNOW = engine.PlayerNOW == player1 ? player2 : player1;
            engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
            engine.Skip_move_count = 0;
            engine.GameStart(this);
            
            // Обновляем статистику после смены хода
            UpdateGameStats();
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
            
            // Обновляем статистику после хода
            UpdateGameStats();
        }

        //Нажатие правой кнопки мышью по канвасу
        public void CanvasRightClicked(object sender, MouseButtonEventArgs e)
        {
            //Защита от дабл клика
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 300)
                return;

            _lastClickTime = DateTime.Now;

            engine.CanvasRightClicked(sender, e, this);
            
            // Обновляем статистику после хода
            UpdateGameStats();
        }

        public void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            engine.Rectangle_MouseDown(sender, e);
            
            // Обновляем статистику после хода
            UpdateGameStats();
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
