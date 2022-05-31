using DataContract;
using SocketService;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer
{
    public interface ISocketBusinessContext : IDisposable
    {
        static Action<GameData> gameDataCallBack { get; set; }
        static Action<GameData> joinGameCallBack { get; set; }
        static Action<GameData> leaveGameCallBack { get; set; }
        SocketServer SocketServer();
        SocketClient SocketClient();
        void BeginHosting();
        void Receive();
        void SendGameData(GameData gameData, string destIp = "", int destPort = 0);
        void JoinGame(GameData gameData, string destIp = "", int destPort = 0);
        void FindServers(Action<ServerData> callBack);
    }
}
