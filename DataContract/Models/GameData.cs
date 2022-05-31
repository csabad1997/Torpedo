using System;
using System.Collections.Generic;
using System.Text;

namespace DataContract
{
    public class GameData
    {
        public int UserId { get; set; }
        public int SourceUserId { get; set; }
        //public int UserName { get; set; }
        public int StepX { get; set; }
        public int StepY { get; set; }
        public bool IsPlayerReady { get; set; }
        public string Data { get; set; }
        public GameDataEnum GameDataType { get; set; }
    }
}
