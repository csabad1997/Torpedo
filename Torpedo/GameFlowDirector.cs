using System;
using System.Collections.Generic;
using System.Text;
using DataContract;

namespace Torpedo
{
    public static class GameFlowDirector
    {
        public static bool IsServer { get; set; }
        public static bool IsAiOpponent { get; set; }
        public static string WinnerName { get; set; }
        public static UserData User { get; set; }
        public static UserData Opponent { get; set; }

        public static GameFlowStepEnum GameFlowStep = GameFlowStepEnum.Login;
        public static GameBoard GameBoard { get; set; }
        public static GameBoard EnemyGameBoard { get; set; }

        public static bool ServerTurn = false;

        public static bool IsGameOver()
        {
            if (GameFlowStep == GameFlowStepEnum.GameOn)
            {
                int shipsFound = 0;
                for (int i = 0; i < GameBoard.Matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < GameBoard.Matrix.GetLength(1); j++)
                    {
                        if (GameBoard.Matrix[i, j].state == TableCellSateEnum.SHIP)
                        {
                            shipsFound++;
                        }
                    }
                }
                if (shipsFound == 0)
                {
                    WinnerName = Opponent.UserName;
                    StepForward();
                    return true;
                }
                else
                {
                    shipsFound = 0;
                    for (int i = 0; i < EnemyGameBoard.Matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < EnemyGameBoard.Matrix.GetLength(1); j++)
                        {
                            if (EnemyGameBoard.Matrix[i, j].state == TableCellSateEnum.SHIP)
                            {
                                shipsFound++;
                            }
                        }
                    }
                    if (shipsFound == 0)
                    {
                        WinnerName = User.UserName;
                        StepForward();
                        return true;
                    }
                }
            }
            return false;
        }

        public static void StepForward()
        {
            if (GameFlowStep != GameFlowStepEnum.GameStop)
                GameFlowStep++;
        }
        public static void StepBackward()
        {
            if (GameFlowStep != GameFlowStepEnum.Login)
                GameFlowStep--;
        }

    }
}
