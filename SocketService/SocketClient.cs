using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using DataContract;
using Newtonsoft.Json;

namespace SocketService
{
    public class SocketClient : SocketBase
    {
        public SocketClient()
        {

        }
        public void ConnectTo(string ip, int port)
        {
            if (Client != null && !Client.Connected)
            {
                RemoteIpEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(ip), port);
                base.Connect(RemoteIpEndPoint);
                Console.WriteLine("Client connected to: " + RemoteIpEndPoint.Address.ToString() + ":" + RemoteIpEndPoint.Port);
            }
        }
        public ServerData SearchServer()
        {
            Console.WriteLine("Searching servers from: " + GetLocalIPAddress() + ":" + GetLocalPort().ToString());
            Send(new SocketMessage()
            {
                MessageType = SocketMessageType.Broadcast,
                Data = UserData.Id.ToString(),
                DestinationIp = IPAddress.Broadcast.ToString(),
                DestionationPort = 11000
            });
            var data = Receive();
            if (data.MessageType == SocketMessageType.Broadcast)
            {
                //Connect(data.SenderIp, data.SenderPort);
                return JsonConvert.DeserializeObject<ServerData>(data.Data);
            }
            return null;
        }
    }
}
