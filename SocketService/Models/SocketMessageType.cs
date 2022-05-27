using System;
using System.Collections.Generic;
using System.Text;

namespace SocketService
{
    public enum SocketMessageType
    {
        Broadcast = 1,
        JoinRoom = 2,
        LeaveRoom = 3,
        ServerDisconnected = 4,
        GameData = 5,
        HandShake = 6,
    }
}
