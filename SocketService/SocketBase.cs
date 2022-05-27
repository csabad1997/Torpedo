using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Net;
using DataContract;

namespace SocketService
{
    public class SocketBase : UdpClient, IDisposable
    {
        public IPEndPoint RemoteIpEndPoint = new IPEndPoint(System.Net.IPAddress.Any, 0);
        public UserData UserData { get; set; }
        public SocketBase()
        {

        }
        public SocketBase(int port) : base(port)
        {
            RemoteIpEndPoint.Port = port;
        }
        public void Send(SocketMessage msg)
        {
            try
            {

                msg.SenderIp = GetLocalIPAddress();
                msg.SenderPort = GetLocalPort();
                string data = JsonConvert.SerializeObject(msg);
                Byte[] sendBytes = Encoding.UTF8.GetBytes(data);
                if (Client != null && Client.Connected)
                {
                    base.Send(sendBytes, sendBytes.Length);
                }
                else
                {
                    base.Send(sendBytes, sendBytes.Length, msg.DestinationIp, msg.DestionationPort);
                }
            }
            catch (Exception e)
            {

            }
        }
        public SocketMessage Receive()
        {
            try
            {
                Byte[] receiveBytes = base.Receive(ref RemoteIpEndPoint);
                SocketMessage decoded = JsonConvert.DeserializeObject<SocketMessage>(Encoding.UTF8.GetString(receiveBytes));
                return decoded;
            }
            catch (Exception e)
            {
                return new SocketMessage();
            }
        }
        public int GetLocalPort()
        {
            try
            {
                if (this.Client == null || ((IPEndPoint)this.Client.LocalEndPoint) == null)
                    return 0;
                return ((IPEndPoint)this.Client.LocalEndPoint).Port;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "0.0.0.0";
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
