using System;
using System.Collections.Generic;
using System.Text;

namespace SocketService
{
    public class SocketMessage
    {
        public string SenderIp { get; set; }
        public int SenderPort { get; set; }
        public string DestinationIp { get; set; }
        public int DestionationPort { get; set; }
        public string Data { get; set; }
        public SocketMessageType MessageType { get; set; }

    }
}
