using DOMINO.Modules;
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
        public int PlayerMove;
        public Player player1;
        public Player player2;
        //Подключение игрового движка 
        GameEngine engine = new GameEngine();
        //Убирание дабл клика после нажатия
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
         private bool ShowPlayerNamesWindow() // тут показываю окна ввода имени
        {
            var namesWindow = new PlayerNamesWindow(); // Важно: класс из Player2.xaml.cs
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
        //Кнопка создания новой игры  
        void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowPlayerNamesWindow()) // Добавленный вызов окна ввода имен
            {
                Application.Current.Shutdown();
                return;
            }
            //Связь данных о игроке с дижком
            ref Player playerNOW = ref engine.PlayerNOW;
            playerNOW = player1;


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
        //На будущее
        public void DrawButton_Click(object sender, RoutedEventArgs e)
        {

        }
        //На будущее
        public void PassButton_Click(object sender, RoutedEventArgs e)
        {

        }
        //Кнопка выхода из проги
        public void ExiteButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        //Нажатие левой кнопки мышью по канвасу
        public void CanvasLeftClicked(object sender, MouseButtonEventArgs e)
        {
            //Защита от дабл клика
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 300)
                return;

            _lastClickTime = DateTime.Now;

            engine.CanvasLeftClicked(sender, e, this);
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
