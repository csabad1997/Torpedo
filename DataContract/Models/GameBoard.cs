using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DataContract
{
    public class GameBoard
    {
        public TableCell[,] Matrix { get; set; }
        private Ship[] _Ships = new Ship[5];
        public Ship[] Ships
        {
            get
            {
                return _Ships.ToList().Where(x => !x.IsPlaced).ToArray();
            }
        }
        public GameBoard()
        {
            Matrix = new TableCell[10, 10];
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    Matrix[i, j] = new TableCell();
                }
            }
            _Ships[0] = new Ship() { Length = 2 };
            _Ships[1] = new Ship() { Length = 3 };
            _Ships[2] = new Ship() { Length = 3 };
            _Ships[3] = new Ship() { Length = 4 };
            _Ships[4] = new Ship() { Length = 5 };
        }
    }
}
