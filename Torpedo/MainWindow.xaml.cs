using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BusinessLogicLayer;
using DataContract;

namespace Torpedo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Game gameView = null;
        private List<ServerData> servers = new List<ServerData>();
        public MainWindow()
        {
            InitializeComponent();
            if (GameFlowDirector.GameFlowStep == GameFlowStepEnum.Login || GameFlowDirector.User == null)
            {
                Login();
            }
            lblUsername.Content = GameFlowDirector.User.UserName;
        }
        public void Login()
        {
            GameFlowDirector.GameFlowStep = GameFlowStepEnum.Login;
            Login loginView = new Login();
            loginView.ShowDialog();
        }
        public void OnClientConnected(GameData gameData)
        {
            using (var context = new BusinessLogicContext())
            {
                GameFlowDirector.Opponent = context.GetUserById(gameData.SourceUserId);
            }
            Application.Current.Dispatcher.BeginInvoke(
             DispatcherPriority.Background,
             new Action(() => {
                 gameView = new Game();
                 gameView.ShowDialog();
             }));
        }
        public void OnClientDisconnected(GameData gameData)
        {
            if (gameView != null)
            {
                gameView.Close();
            }
        }
        public void OnServerSearchDone(ServerData server)
        {
            Application.Current.Dispatcher.BeginInvoke(
             DispatcherPriority.Background,
             new Action(() => {
                 lblSearch.Visibility = Visibility.Collapsed;
                 if (server != null && !listServerList.Items.Contains(server.Name))
                 {
                     servers.Add(server);
                     listServerList.Items.Add(server.Name);
                 }
             }));
        }
        private void btnMainJoin_Click(object sender, RoutedEventArgs e)
        {
            HideGroups();
            GameFlowDirector.IsServer = false;
            grJoinGameItems.Visibility = Visibility.Visible;
            lblSearch.Visibility = Visibility.Visible;
            Task.Run(() => {
                using (ISocketBusinessContext context = new BusinessLogicContext(false))
                {
                    context.SocketClient().UserData = GameFlowDirector.User;
                    context.FindServers(OnServerSearchDone);
                }
            });
        }

        private void btnMainHost_Click(object sender, RoutedEventArgs e)
        {
            HideGroups();
            GameFlowDirector.IsServer = true;
            chkAI.IsEnabled = true;
            btnCreatedGame.IsEnabled = true;
            lblWaiting.Visibility = Visibility.Collapsed;
            grHostGameItems.Visibility = Visibility.Visible;

        }

        private void btnMainLogout_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            HideGroups();
            grMainmenuItems.Visibility = Visibility.Visible;
            DisposeSocket();
        }
        private void HideGroups()
        {
            grMainmenuItems.Visibility = Visibility.Collapsed;
            grHostGameItems.Visibility = Visibility.Collapsed;
            grJoinGameItems.Visibility = Visibility.Collapsed;
        }

        private void btnCreatedGame_Click(object sender, RoutedEventArgs e)
        {
            chkAI.IsEnabled = true;
            btnCreatedGame.IsEnabled = true;
            lblWaiting.Visibility = Visibility.Collapsed;
            if (chkAI.IsChecked.Value)
            {
                GameFlowDirector.IsAiOpponent = true;
                GameFlowDirector.Opponent = new UserData()
                {
                    Id = 0,
                    UserName = "AI"
                };
                gameView = new Game();
                gameView.ShowDialog();
            }
            else
            {
                using (ISocketBusinessContext context = new BusinessLogicContext(GameFlowDirector.IsServer))
                {
                    context.SocketServer().UserData = GameFlowDirector.User;
                    BusinessLogicContext.joinGameCallBack = OnClientConnected;
                    BusinessLogicContext.leaveGameCallBack = OnClientDisconnected;
                    context.BeginHosting();
                }
                chkAI.IsEnabled = false;
                btnCreatedGame.IsEnabled = false;
                lblWaiting.Visibility = Visibility.Visible;
            }
        }

        private void btnJoin_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new BusinessLogicContext(GameFlowDirector.IsServer))
            {
                context.IsSearching = false;
                Thread.Sleep(500);
                var server = servers.FirstOrDefault(x => x.Name == listServerList.SelectedValue.ToString());
                if (server != null)
                {
                    context.JoinGame(new GameData()
                    {
                        GameDataType = GameDataEnum.UserInfo,
                        SourceUserId = GameFlowDirector.User.Id,
                        UserId = server.UserId,
                    }, server.IpAddress, server.Port
                    );
                    Task.Run(() =>
                    {
                        BusinessLogicContext.joinGameCallBack = OnClientConnected;
                        context.Receive();
                    });
                }
            }
        }
        private void DisposeSocket()
        {
            if (BusinessLogicContext.socket != null)
            {
                BusinessLogicContext.socket.Dispose();
            }
        }

        private void listServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnJoin.IsEnabled = true;
        }
    }
}
