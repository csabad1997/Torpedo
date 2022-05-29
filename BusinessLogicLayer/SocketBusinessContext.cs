using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SocketService;
using System.Threading.Tasks;
using DataContract;
using Newtonsoft.Json;
using System.Threading;

namespace BusinessLogicLayer
{
    public partial class BusinessLogicContext : ISocketBusinessContext
    {
        public static SocketBase socket = null;
        public static bool IsServerMode = false;
        private CancellationTokenSource SearchTokenSource;
        private static bool _IsSearching { get; set; }
        public bool IsSearching
        {
            get
            {
                return _IsSearching;
            }
            set
            {
                if (!value && SearchTokenSource != null)
                {
                    SearchTokenSource.Cancel();
                }
                _IsSearching = value;
            }
        }
        public static Action<GameData> gameDataCallBack { get; set; }
        public static Action<GameData> joinGameCallBack { get; set; }
        public static Action<GameData> leaveGameCallBack { get; set; }
        public BusinessLogicContext(bool isServer)
        {
            if (isServer)
            {
                if (socket == null || IsServerMode == false)
                {
                    socket = new SocketServer();
                }
            }
            else
            {
                if (socket == null || IsServerMode == true)
                {
                    socket = new SocketClient();
                }
            }
            IsServerMode = isServer;
        }
        public SocketServer SocketServer()
        {
            if (!IsServerMode)
            {
                throw new Exception("Végzetes socket beállítási hiba!");
            }
            return socket as SocketServer;
        }
        public SocketClient SocketClient()
        {
            if (IsServerMode)
            {
                throw new Exception("Végzetes socket beállítási hiba!");
            }
            return socket as SocketClient;
        }
        public void BeginHosting()
        {
            SocketServer().Listen(ProccessSocketMessage);
        }
        public void Receive()
        {
            ProccessSocketMessage(socket.Receive());
        }
        private void ProccessSocketMessage(SocketMessage msg)
        {
            switch (msg.MessageType)
            {
                case SocketMessageType.JoinRoom:
                    if (IsServerMode)
                    {
                        var data = JsonConvert.DeserializeObject<GameData>(msg.Data);
                        SocketServer().Send(new SocketMessage()
                        {
                            DestinationIp = msg.SenderIp,
                            DestionationPort = msg.SenderPort,
                            MessageType = SocketMessageType.JoinRoom,
                            Data = JsonConvert.SerializeObject(new GameData()
                            {
                                GameDataType = GameDataEnum.UserInfo,
                                SourceUserId = data.UserId,
                                UserId = data.SourceUserId
                            })
                        });
                    }
                    joinGameCallBack.Invoke(JsonConvert.DeserializeObject<GameData>(msg.Data));
                    break;
                case SocketMessageType.HandShake:
                    break;
                case SocketMessageType.LeaveRoom:
                    leaveGameCallBack.Invoke(JsonConvert.DeserializeObject<GameData>(msg.Data));
                    break;
                case SocketMessageType.GameData:
                    gameDataCallBack.Invoke(JsonConvert.DeserializeObject<GameData>(msg.Data));
                    break;
                default:
                    break;
            }
        }
        private void SendHandShake(int userId)
        {
            if (IsServerMode)
            {
                var other = SocketServer().Clients.FirstOrDefault(x => x.Id == userId);
                socket.Send(new SocketMessage()
                {
                    MessageType = SocketMessageType.HandShake,
                    Data = userId.ToString(),
                    DestinationIp = other.Ip,
                    DestionationPort = other.Port
                });
            }
            else
            {
                socket.Send(new SocketMessage()
                {
                    MessageType = SocketMessageType.HandShake,
                    Data = userId.ToString()
                });
            }
        }
        public void SendGameData(GameData gameData, string destIp = "", int destPort = 0)
        {
            if (IsServerMode)
            {
                var other = SocketServer().Clients.FirstOrDefault(x => x.Id == gameData.UserId);
                socket.Send(new SocketMessage()
                {
                    MessageType = SocketMessageType.GameData,
                    Data = JsonConvert.SerializeObject(gameData),
                    DestinationIp = other.Ip,
                    DestionationPort = other.Port
                });
            }
            else
            {
                socket.Send(new SocketMessage()
                {
                    MessageType = SocketMessageType.GameData,
                    Data = JsonConvert.SerializeObject(gameData)
                });
            }

        }
        public void JoinGame(GameData gameData, string destIp = "", int destPort = 0)
        {
            if (IsServerMode)
            {
                var other = SocketServer().Clients.FirstOrDefault(x => x.Id == gameData.UserId);
                SocketServer().Send(new SocketMessage()
                {
                    MessageType = SocketMessageType.JoinRoom,
                    Data = JsonConvert.SerializeObject(gameData),
                    DestinationIp = other.Ip,
                    DestionationPort = other.Port
                });
            }
            else
            {
                SocketClient().ConnectTo(destIp, destPort);
                SendHandShake(gameData.SourceUserId);
                SocketClient().Send(new SocketMessage()
                {
                    MessageType = SocketMessageType.JoinRoom,
                    Data = JsonConvert.SerializeObject(gameData),
                    DestinationIp = destIp,
                    DestionationPort = destPort
                });
            }

        }
        public void FindServers(Action<ServerData> callBack)
        {
            IsSearching = true;
            SearchTokenSource = new CancellationTokenSource();
            Task.Run(() => {
                //while (IsSearching)
                //{
                //    if (!SearchTokenSource.IsCancellationRequested)
                //    {
                //        var serverData = SocketClient().SearchServer();
                //        callBack.Invoke(serverData);
                //    }
                //}
                if (!SearchTokenSource.IsCancellationRequested)
                {
                    var serverData = SocketClient().SearchServer();
                    callBack.Invoke(serverData);
                }
            }, SearchTokenSource.Token);
            Task.Run(() => {
                Thread.Sleep(20000);
                IsSearching = false;
            });
        }

    }
}
