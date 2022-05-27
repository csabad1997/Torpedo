using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using DataContract;

namespace SocketService
{
    public class SocketServer : SocketBase
    {
        public List<Client> Clients = new List<Client>();
        public bool IsRunning { get; set; }
        private CancellationTokenSource ListenTokenSource;
        public SocketServer() : base(11000)
        {
            IsRunning = true;
            EnableBroadcast = true;
        }
        public void Listen(Action<SocketMessage> callBack)
        {
            ListenTokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                Console.WriteLine("Server listening on: " + GetLocalIPAddress() + ":" + GetLocalPort());
                while (IsRunning)
                {
                    Console.WriteLine("Server listening...");
                    var received = Receive();
                    if (IsRunning)
                    {//Meg kell vizsgálni különben a dispose futásánál belefuthat
                        ManageNewConnection(received);
                        callBack.Invoke(received);
                        Thread.Sleep(500);
                    }
                }
            }, ListenTokenSource.Token);
        }
        public void StopListening()
        {
            ListenTokenSource.Cancel();
        }
        private void ManageNewConnection(SocketMessage msg)
        {
            if (msg.MessageType == SocketMessageType.Broadcast)
            {
                int userId = Convert.ToInt32(msg.Data);
                var knownClient = Clients.FirstOrDefault(x => x.Id == userId);
                if (knownClient == null)
                {
                    Clients.Add(new Client()
                    {
                        Id = userId,
                        Ip = msg.SenderIp,
                        Port = msg.SenderPort
                    });
                    Console.WriteLine("New Client connected from: " + msg.SenderIp + ":" + msg.SenderPort);
                    Send(new SocketMessage()
                    {
                        DestinationIp = RemoteIpEndPoint.Address.ToString(),
                        DestionationPort = RemoteIpEndPoint.Port,
                        Data = JsonConvert.SerializeObject(new ServerData()
                        {
                            Name = UserData.UserName,
                            IpAddress = GetLocalIPAddress(),
                            Port = GetLocalPort(),
                            UserId = UserData.Id
                        }),
                        MessageType = SocketMessageType.Broadcast,
                    });
                }
                else if (knownClient.Id != userId)
                {
                    knownClient.Ip = msg.SenderIp;
                    knownClient.Port = msg.SenderPort;
                    knownClient.Id = userId;
                }
            }
            else if (msg.MessageType == SocketMessageType.HandShake)
            {
                int userId = Convert.ToInt32(msg.Data);
                var knownClient = Clients.FirstOrDefault(x => x.Id == userId);
                if (knownClient != null)
                {
                    knownClient.Ip = msg.SenderIp;
                    knownClient.Port = msg.SenderPort;
                }
            }
        }
        protected override void Dispose(bool disposing)
        {
            IsRunning = false;
            base.Dispose(disposing);
        }
    }
}
