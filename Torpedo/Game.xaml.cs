using BusinessLogicLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DataContract;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Windows.Threading;

namespace Torpedo
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : Window
    {
        private bool isMouseHoveringOpponentBoard = false;
        private bool isMouseHoveringPlayerBoard = false;
        private bool _enemyReady { get; set; }
        private bool _playerReady { get; set; }
        private bool enemyReady
        {
            get
            {
                return _enemyReady;
            }
            set
            {
                lblOpponentReady.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                lblOpponentNotReady.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                _enemyReady = value;
                StartRound();
            }
        }
        private bool playerReady
        {
            get
            {
                return _playerReady;
            }
            set
            {
                lblPlayerReady.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                lblPlayerNotReady.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                _playerReady = value;
                StartRound();
            }
        }
        private int selectedShip = -1;
        public Game()
        {
            InitializeComponent();
            if (GameFlowDirector.Opponent != null)
            {
                lblOpponentName.Content = " vs " + GameFlowDirector.Opponent.UserName;
            }
            if (GameFlowDirector.User != null)
            {
                lblPlayerName.Content = GameFlowDirector.User.UserName;
            }
            GameFlowDirector.GameFlowStep = GameFlowStepEnum.GamePreparation;
            GameFlowDirector.GameBoard = new GameBoard();
            UpdateShipList();
            if (!GameFlowDirector.IsAiOpponent)
            {
                Task.Run(() =>
                {
                    using (var context = new BusinessLogicContext(GameFlowDirector.IsServer))
                    {
                        BusinessLogicContext.gameDataCallBack = gameDataCallBack;
                        if (!GameFlowDirector.IsServer)
                        {
                            while (true)
                            {
                                context.Receive();
                                Thread.Sleep(1000);
                            }
                        }
                    }
                });
            }
            else
            {
                AIPrepare();
            }
            UpdateGameTable();
        }

        private void UpdateShipList()
        {
            listShips.Items.Clear();
            foreach (var ship in GameFlowDirector.GameBoard.Ships)
            {
                listShips.Items.Add(ship.Length);
            }
            if (GameFlowDirector.GameBoard.Ships.Length == 0)
            {
                btnPlayerReady.IsEnabled = true;
            }
        }
        #region MouseEvents
        private void boardOpponent_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GameFlowDirector.GameFlowStep != GameFlowStepEnum.GameStop)
            {
                if ((GameFlowDirector.ServerTurn && GameFlowDirector.IsServer) ||
                    (!GameFlowDirector.ServerTurn && !GameFlowDirector.IsServer))
                {
                    var click = e.GetPosition(boardOpponent);
                    var normalized = GetNormalizedPosition(click.X, click.Y);
                    ClickPosition(normalized, GameFlowDirector.EnemyGameBoard);
                }
            }
        }

        private void boardOpponent_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseHoveringOpponentBoard = true;
        }

        private void boardOpponent_MouseLeave(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < GameFlowDirector.EnemyGameBoard.Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < GameFlowDirector.EnemyGameBoard.Matrix.GetLength(1); j++)
                {
                    GameFlowDirector.EnemyGameBoard.Matrix[i, j].isHovered = false;
                }
            }
            isMouseHoveringOpponentBoard = false;
            UpdateGameTable();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseHoveringOpponentBoard)
            {
                for (int i = 0; i < GameFlowDirector.EnemyGameBoard.Matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < GameFlowDirector.EnemyGameBoard.Matrix.GetLength(1); j++)
                    {
                        GameFlowDirector.EnemyGameBoard.Matrix[i, j].isHovered = false;
                    }
                }
                var click = e.GetPosition(boardOpponent);
                var normalized = GetNormalizedPosition(click.X, click.Y);
                GameFlowDirector.EnemyGameBoard.Matrix[normalized.Item1, normalized.Item2].isHovered = true;
                UpdateGameTable();
            }
            if (isMouseHoveringPlayerBoard)
            {
                var click = e.GetPosition(boardPlayer);
                var normalized = GetNormalizedPosition(click.X, click.Y);
                for (int i = 0; i < GameFlowDirector.GameBoard.Matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < GameFlowDirector.GameBoard.Matrix.GetLength(1); j++)
                    {
                        GameFlowDirector.GameBoard.Matrix[i, j].isHovered = false;
                    }
                }
                if (selectedShip > -1)
                {
                    var ship = GameFlowDirector.GameBoard.Ships[selectedShip];
                    if (isShipPlacementValid(ship, normalized))
                    {
                        if (ship.IsVertical)
                        {
                            for (int i = 0; i < ship.Length; i++)
                            {
                                GameFlowDirector.GameBoard.Matrix[normalized.Item1, normalized.Item2 - i].isHovered = true;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < ship.Length; i++)
                            {
                                GameFlowDirector.GameBoard.Matrix[normalized.Item1 + i, normalized.Item2].isHovered = true;
                            }
                        }
                    }
                }
                UpdateGameTable();
            }
        }

        private void boardPlayer_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseHoveringPlayerBoard = true;
        }

        private void boardPlayer_MouseLeave(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < GameFlowDirector.GameBoard.Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < GameFlowDirector.GameBoard.Matrix.GetLength(1); j++)
                {
                    GameFlowDirector.GameBoard.Matrix[i, j].isHovered = false;
                }
            }
            isMouseHoveringPlayerBoard = false;
            UpdateGameTable();
        }

        private void boardPlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GameFlowDirector.GameFlowStep == GameFlowStepEnum.GamePreparation && selectedShip > -1)
            {
                var ship = GameFlowDirector.GameBoard.Ships[selectedShip];
                var click = e.GetPosition(boardPlayer);
                var normalized = GetNormalizedPosition(click.X, click.Y);
                if (isShipPlacementValid(ship, normalized))
                {
                    if (ship.IsVertical)
                    {
                        for (int i = 0; i < ship.Length; i++)
                        {
                            GameFlowDirector.GameBoard.Matrix[normalized.Item1, normalized.Item2 - i].state = TableCellSateEnum.SHIP;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < ship.Length; i++)
                        {
                            GameFlowDirector.GameBoard.Matrix[normalized.Item1 + i, normalized.Item2].state = TableCellSateEnum.SHIP;
                        }
                    }
                    GameFlowDirector.GameBoard.Ships[selectedShip].IsPlaced = true;
                    selectedShip = -1;
                    btnRotateShip.IsEnabled = false;
                    UpdateShipList();
                    UpdateGameTable();
                }
            }
        }
        #endregion
        private bool isShipPlacementValid(Ship ship, Tuple<int, int> click)
        {
            if (ship.IsVertical)
            {
                if (click.Item2 - ship.Length > -2)
                {
                    for (int i = 0; i < ship.Length; i++)
                    {
                        if (GameFlowDirector.GameBoard.Matrix[click.Item1, click.Item2 - i].state == TableCellSateEnum.SHIP)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            else
            {
                if (click.Item1 + ship.Length < 11)
                {
                    for (int i = 0; i < ship.Length; i++)
                    {
                        if (GameFlowDirector.GameBoard.Matrix[click.Item1 + i, click.Item2].state == TableCellSateEnum.SHIP)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public void gameDataCallBack(GameData data)
        {
            Application.Current.Dispatcher.BeginInvoke(
                     DispatcherPriority.Background,
                     new Action(() =>
                     {
                         switch (data.GameDataType)
                         {
                             case GameDataEnum.UserInfo:
                                 break;
                             case GameDataEnum.Step:
                                 if (GameFlowDirector.GameBoard.Matrix[data.StepX, data.StepY].state == TableCellSateEnum.SHIP)
                                 {
                                     GameFlowDirector.GameBoard.Matrix[data.StepX, data.StepY].state = TableCellSateEnum.HIT;
                                 }
                                 else if (GameFlowDirector.GameBoard.Matrix[data.StepX, data.StepY].state == TableCellSateEnum.EMPTY)
                                 {
                                     GameFlowDirector.GameBoard.Matrix[data.StepX, data.StepY].state = TableCellSateEnum.MISS;
                                 }
                                 GameFlowDirector.ServerTurn = !GameFlowDirector.ServerTurn;
                                 UpdateGameTable();
                                 SetNextPlayerText();
                                 break;
                             case GameDataEnum.GameResult:
                                 var args = data.Data.Split(";");
                                 if (args[0] == "True")
                                 {
                                     GameFlowDirector.WinnerName = args[1];
                                     FinishGame();
                                 }
                                 break;
                             case GameDataEnum.ReadyUp:
                                 if (GameFlowDirector.GameFlowStep == GameFlowStepEnum.GamePreparation)
                                 {
                                     enemyReady = data.IsPlayerReady;
                                     if (enemyReady)
                                     {
                                         GameFlowDirector.EnemyGameBoard = JsonConvert.DeserializeObject<GameBoard>(data.Data);
                                         UpdateGameTable();
                                     }
                                 }
                                 break;
                             default:
                                 break;
                         }
                     }));

        }

        public void StartRound()
        {
            if (!enemyReady || !playerReady)
            {
                return;
            }
            GameFlowDirector.StepForward();
            var flowstep = GameFlowDirector.GameFlowStep;
            SetNextPlayerText();
            if (GameFlowDirector.IsAiOpponent)
            {
                AIClick();
            }
            preparationGroup.Visibility = Visibility.Collapsed;
            opponentGroup.Visibility = Visibility.Visible;
        }

        private void btnPlayerReady_Click(object sender, RoutedEventArgs e)
        {
            playerReady = !playerReady;
            if (!GameFlowDirector.IsAiOpponent)
            {
                using (var context = new BusinessLogicContext(GameFlowDirector.IsServer))
                {
                    context.SendGameData(new GameData()
                    {
                        GameDataType = GameDataEnum.ReadyUp,
                        IsPlayerReady = playerReady,
                        Data = JsonConvert.SerializeObject(GameFlowDirector.GameBoard),
                        SourceUserId = GameFlowDirector.User.Id,
                        UserId = GameFlowDirector.Opponent.Id,
                    });
                }
            }

        }

        private void UpdateGameTable(bool onlyOpponentTable = false)
        {
            if (GameFlowDirector.EnemyGameBoard != null)
            {
                boardOpponent.Source = BitmapConvert(ImageHandler.DrawMap(GameFlowDirector.EnemyGameBoard, isOpponentTable: true));
            }
            if (!onlyOpponentTable && GameFlowDirector.GameBoard != null)
            {
                boardPlayer.Source = BitmapConvert(ImageHandler.DrawMap(GameFlowDirector.GameBoard));
            }
        }

        private Tuple<int, int> GetNormalizedPosition(double x, double y)
        {
            double fX = Math.Floor(x) - 15.5;
            double fY = Math.Floor(y) - 15.5;
            int intX = Convert.ToInt32(fX / 31.1);
            int intY = Convert.ToInt32(fY / 31.1);
            if (intX > 9)
            {
                intX = 9;
            }
            if (intY > 9)
            {
                intY = 9;
            }
            if (intX < 0)
            {
                intX = 0;
            }
            if (intY < 0)
            {
                intY = 0;
            }
            return new Tuple<int, int>(intX, intY);
        }

        private BitmapImage BitmapConvert(System.Drawing.Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            GameFlowDirector.EnemyGameBoard = null;
            GameFlowDirector.GameBoard = null;
            GameFlowDirector.IsAiOpponent = false;
            GameFlowDirector.WinnerName = "";
            GameFlowDirector.Opponent = null;
            if (!GameFlowDirector.IsServer)
            {
                using (ISocketBusinessContext context = new BusinessLogicContext(GameFlowDirector.IsServer))
                {
                    //context.SocketClient().Client.Disconnect(true);
                }
            }
            base.OnClosing(e);
        }

        private void listShips_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedShip = (int)listShips.SelectedIndex;
            btnRotateShip.IsEnabled = true;
        }

        private void btnRotateShip_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShip > -1)
            {
                GameFlowDirector.GameBoard.Ships[selectedShip].IsVertical = !GameFlowDirector.GameBoard.Ships[selectedShip].IsVertical;
            }
        }

        private void SetNextPlayerText()
        {
            if (GameFlowDirector.GameFlowStep == GameFlowStepEnum.GameStop)
            {
                if (!GameFlowDirector.IsAiOpponent)
                {
                    using (var context = new BusinessLogicContext(GameFlowDirector.IsServer))
                    {
                        context.SendGameData(new GameData()
                        {
                            GameDataType = GameDataEnum.GameResult,
                            SourceUserId = GameFlowDirector.User.Id,
                            UserId = GameFlowDirector.Opponent.Id,
                            Data = "True;" + GameFlowDirector.WinnerName,
                        });
                    }
                }

                FinishGame();
                return;
            }
            if (GameFlowDirector.ServerTurn && GameFlowDirector.IsServer)
            {
                lblUserTurn.Content = GameFlowDirector.User.UserName + " lép";
            }
            else if (!GameFlowDirector.ServerTurn && !GameFlowDirector.IsServer)
            {
                lblUserTurn.Content = GameFlowDirector.User.UserName + " lép";
            }
            else
            {
                lblUserTurn.Content = GameFlowDirector.Opponent.UserName + " lép";
            }
        }

        private void AIPrepare()
        {
            if (GameFlowDirector.EnemyGameBoard == null)
            {
                GameFlowDirector.EnemyGameBoard = new GameBoard();
            }
            Random ran = new Random();
            while (GameFlowDirector.EnemyGameBoard.Ships.Length > 0)
            {
                var ship = GameFlowDirector.EnemyGameBoard.Ships[0];
                ship.IsVertical = ran.Next(0, 1) > 0;
                int x;
                int y;
                while (!isShipPlacementValid(ship, new Tuple<int, int>(x = ran.Next(0, 10), y = ran.Next(0, 10)))) ;
                if (ship.IsVertical)
                {
                    for (int i = 0; i < ship.Length; i++)
                    {
                        GameFlowDirector.EnemyGameBoard.Matrix[x, y - i].state = TableCellSateEnum.SHIP;
                    }
                }
                else
                {
                    for (int i = 0; i < ship.Length; i++)
                    {
                        GameFlowDirector.EnemyGameBoard.Matrix[x + i, y].state = TableCellSateEnum.SHIP;
                    }
                }
                ship.IsPlaced = true;
            }
            enemyReady = true;
        }

        private void AIClick()
        {
            if (GameFlowDirector.ServerTurn && GameFlowDirector.IsServer)
            {
                return;
            }
            Random ran = new Random();
            while (!ClickPosition(new Tuple<int, int>(ran.Next(0, 10), ran.Next(0, 10)), GameFlowDirector.GameBoard)) ;
        }

        private bool ClickPosition(Tuple<int, int> normalized, GameBoard gameBoard)
        {
            if (gameBoard.Matrix[normalized.Item1, normalized.Item2].state == TableCellSateEnum.SHIP ||
                        gameBoard.Matrix[normalized.Item1, normalized.Item2].state == TableCellSateEnum.EMPTY)
            {
                if (gameBoard.Matrix[normalized.Item1, normalized.Item2].state == TableCellSateEnum.SHIP)
                {
                    gameBoard.Matrix[normalized.Item1, normalized.Item2].state = TableCellSateEnum.HIT;
                }
                else if (gameBoard.Matrix[normalized.Item1, normalized.Item2].state == TableCellSateEnum.EMPTY)
                {
                    gameBoard.Matrix[normalized.Item1, normalized.Item2].state = TableCellSateEnum.MISS;
                }
                GameFlowDirector.ServerTurn = !GameFlowDirector.ServerTurn;
                if (GameFlowDirector.IsAiOpponent)
                {
                    AIClick();
                }
                else
                {
                    using (var context = new BusinessLogicContext(GameFlowDirector.IsServer))
                    {
                        context.SendGameData(new GameData()
                        {
                            GameDataType = GameDataEnum.Step,
                            SourceUserId = GameFlowDirector.User.Id,
                            StepX = normalized.Item1,
                            StepY = normalized.Item2,
                            UserId = GameFlowDirector.Opponent.Id
                        });
                    }
                }
                if (GameFlowDirector.IsGameOver())
                {
                    FinishGame();
                }
                SetNextPlayerText();
                return true;
            }
            return false;
        }

        private void FinishGame()
        {
            lblUserTurn.Content = GameFlowDirector.WinnerName + " nyert";
        }
    }
}

