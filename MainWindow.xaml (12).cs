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
    public partial class MainWindow : Window
    {
        // Все доминошки
        public List<Tile> allTilles;
        public List<Tile> TilesOnCanvas;
        // Данный ход игрока
        public Player player1;
        public Player player2;
        // Подключение игрового движка 
        GameEngine engine = new GameEngine();
        // Убирание дабл клика после нажатия
        private DateTime _lastClickTime = DateTime.MinValue;
        // Медиа плеер
        private MediaPlayer _musicPlayer;
        // Список для хранения статистики игр
        private List<string> gameStats = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            GameBoard.MouseLeftButtonDown += CanvasLeftClicked;
            GameBoard.MouseRightButtonDown += CanvasRightClicked;
            // Добавление плеера
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

            int player1Score = CalculatePlayerScore(player1);
            int player2Score = CalculatePlayerScore(player2);
            
            // Обновление текста статистики
            CurrentPlayerText.Text = $"{engine.PlayerNOW.name} (Ход: {engine.Skip_move_count + 1})";
            BoneyardCountText.Text = $"{allTilles.Count} | {player1.name}: {player1Score} | {player2.name}: {player2Score}";
        }

        // Метод для подсчета очков игрока
        private int CalculatePlayerScore(Player player)
        {
            int score = 0;
            foreach (var tile in player.hand)
            {
                score += tile.value1 + tile.value2;
            }
            return score;
        }

        // Кнопка создания новой игры  
        void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            var namesWindow = new PlayerNamesWindow();
            var result = namesWindow.ShowDialog();

            if (result != true)
            {
                return;
            }

            // Инициализация новой игры
            player1 = new Player { name = namesWindow.Player1Name };
            player2 = new Player { name = namesWindow.Player2Name };
            engine.PlayerNOW = player1;
            
            // Очистка предыдущей игры
            if (allTilles != null) allTilles.Clear();
            if (player1.hand.Count != 0) player1.hand.Clear();
            if (player2.hand.Count != 0) player2.hand.Clear();
            HandPlayer.Children.Clear();
            GameBoard.Children.Clear();
            
            engine.mainw(this);
            engine.Clicked_rectangle = false;
            allTilles = new List<Tile>();
            TilesOnCanvas = new List<Tile>();
            
            // Создание всех костяшек
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
            
            // Раздача костяшек игрокам
            engine.GiveHandPlayer(ref player1, ref allTilles);
            engine.GiveHandPlayer(ref player2, ref allTilles);
            
            // Отрисовка костяшек игрока 1
            engine.DrawHandTile(ref player1, ref HandPlayer);
            
            // Инициализация статистики
            UpdateGameStats();
            engine.GameStart(this);
        }

        // Метод перемешивает элементы в List
        public static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count();
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        // Метод дает игроку дополнительную кость
        public void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            if (player1 == null || allTilles.Count == 0) return;

            engine.PlayerNOW.hand.Add(allTilles[0]);
            HandPlayer.Children.Clear();
            engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
            allTilles.RemoveAt(0);
            
            UpdateGameStats();
        }

        // Метод смены игрока при нажатии на кнопку
        public void PassButton_Click(object sender, RoutedEventArgs e)
        {
            if (player1 == null) return;
            
            // Проверка на конец игры
            if (engine.Skip_move_count >= 2)
            {
                EndGame();
                return;
            }
            
            SwitchPlayer();
            UpdateGameStats();
        }

        // Метод завершения игры
        private void EndGame()
        {
            int player1Score = CalculatePlayerScore(player1);
            int player2Score = CalculatePlayerScore(player2);
            
            string winner = player1Score <= player2Score ? player1.name : player2.name;
            AddGameToStats(winner, player1Score, player2Score);
            
            MessageBox.Show($"Игра окончена! Победитель: {winner}\n{player1.name}: {player1Score} очков\n{player2.name}: {player2Score} очков", 
                          "Конец игры", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);
        }

        // Метод смены игрока
        private void SwitchPlayer()
        {
            HandPlayer.Children.Clear();
            engine.PlayerNOW = engine.PlayerNOW == player1 ? player2 : player1;
            engine.DrawHandTile(ref engine.PlayerNOW, ref HandPlayer);
            engine.Skip_move_count++;
            engine.GameStart(this);
        }

        // Остальные методы остаются без изменений
        // ...
    }
}
