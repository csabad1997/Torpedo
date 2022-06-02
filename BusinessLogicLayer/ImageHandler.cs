using DataContract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BusinessLogicLayer
{
    public static class ImageHandler
    {
        private static Pen blackPen = new Pen(Color.Black, 1);
        private static SolidBrush redBrush = new SolidBrush(Color.Red);
        private static SolidBrush whiteBrush = new SolidBrush(Color.White);
        private static SolidBrush bgBrush = new SolidBrush(Color.FromArgb(55, 55, 200));
        private static SolidBrush greyBrush = new SolidBrush(Color.LightGray);
        private static SolidBrush shipBrush = new SolidBrush(Color.DarkGray);

        public static Bitmap DrawMap(GameBoard gameBoard, Bitmap bmp = null, bool isOpponentTable = false)
        {
            if (bmp == null)
            {
                bmp = new Bitmap(311, 311);
            }
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(bgBrush, 0, 0, bmp.Width, bmp.Height);
                for (int i = 0; i < gameBoard.Matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < gameBoard.Matrix.GetLength(1); j++)
                    {
                        TableCell cell = gameBoard.Matrix[i, j];
                        if (cell.isHovered)
                        {
                            g.FillRectangle(greyBrush, (i * 31) + 1, (j * 31) + 1, 30, 30);
                        }
                        else
                        {
                            switch (cell.state)
                            {
                                case TableCellSateEnum.EMPTY:
                                    //g.FillRectangle(bgBrush, (i * 30) + 1, (j * 30) + 1, 30, 30);
                                    break;
                                case TableCellSateEnum.SHIP:
                                    //if(!isOpponentTable)
                                    g.FillRectangle(shipBrush, (i * 31) + 1, (j * 31) + 1, 30, 30);
                                    break;
                                case TableCellSateEnum.MISS:
                                    g.FillRectangle(whiteBrush, (i * 31) + 1, (j * 31) + 1, 30, 30);
                                    break;
                                case TableCellSateEnum.HIT:
                                    g.FillRectangle(redBrush, (i * 31) + 1, (j * 31) + 1, 30, 30);
                                    break;
                                default:
                                    g.FillRectangle(bgBrush, (i * 31) + 1, (j * 31) + 1, 30, 30);
                                    break;
                            }
                        }
                    }
                }
                for (int i = 0; i < 11; i++)
                {
                    g.DrawLine(blackPen, 0, (i * 30) + i, bmp.Width, (i * 30) + i);
                    g.DrawLine(blackPen, (i * 30) + i, 0, (i * 30) + i, bmp.Height);
                }
            }
            return bmp;
        }
    }
}
