using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GomokuPackage
{
    public class GomokuGame
    {
        private int[,] board = new int[15, 15];
        private int currentPlayer = 1;
        private int moveCount = 0;
        public int BoardSize = 15;
        public Stopwatch sw = new Stopwatch();
        private int millisecondsToThink;

        public GomokuGame(int timeToThink)
        {
            millisecondsToThink = timeToThink;
        }

        public long RemainingTime()
        {
            return millisecondsToThink - sw.ElapsedMilliseconds;
        }

        public int[,] GetBoard()
        {
            return (int[,])board.Clone();
        }

        public bool MakeMove(int player, int x, int y)
        {
            try
            {
                if (board[x, y] == 0)
                {
                    board[x, y] = player;
                    moveCount++;
                    return true;
                }
            }
            catch { }
            return false;
        }

        public int CheckWinner()
        {
            int boardSize = 15;
            int winLength = 5;

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    int player = board[i, j];
                    if (player == 0)
                        continue;

                    // Check horizontal (right)
                    if (j <= boardSize - winLength)
                    {
                        bool win = true;
                        for (int k = 1; k < winLength; k++)
                        {
                            if (board[i, j + k] != player)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win)
                            return player;
                    }

                    // Check vertical (down)
                    if (i <= boardSize - winLength)
                    {
                        bool win = true;
                        for (int k = 1; k < winLength; k++)
                        {
                            if (board[i + k, j] != player)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win)
                            return player;
                    }

                    // Check diagonal (down-right)
                    if (i <= boardSize - winLength && j <= boardSize - winLength)
                    {
                        bool win = true;
                        for (int k = 1; k < winLength; k++)
                        {
                            if (board[i + k, j + k] != player)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win)
                            return player;
                    }

                    // Check diagonal (down-left)
                    if (i <= boardSize - winLength && j >= winLength - 1)
                    {
                        bool win = true;
                        for (int k = 1; k < winLength; k++)
                        {
                            if (board[i + k, j - k] != player)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win)
                            return player;
                    }
                }
            }

            return 0; // No winner yet
        }


        public int GetCurrentPlayer()
        {
            return currentPlayer;
        }

        public void NextPlayer()
        {
            currentPlayer = (currentPlayer == 1) ? 2 : 1; // Switch between 1 and 2
        }

        public bool IsBoardFull()
        {
            return moveCount >= 225;
        }

        public void DisplayBoard(int startX, int startY)
        {
            // Set cursor position to the starting point
            Console.SetCursorPosition(startX, startY);

            // Column headers
            Console.Write("   "); // Spaces to align with row numbers
            for (int i = 0; i < BoardSize; i++)
            {
                Console.Write(" " + (char)('A' + i) + " ");
            }
            Console.WriteLine();

            for (int x = 0; x < BoardSize; x++)
            {
                // Move the cursor to the correct position
                Console.SetCursorPosition(startX, startY + x + 1);

                // Row numbers
                Console.Write((x + 1).ToString("D2") + " ");
                for (int y = 0; y < BoardSize; y++)
                {
                    char symbol = board[x, y] switch
                    {
                        1 => 'X',
                        2 => 'O',
                        _ => '.'
                    };
                    Console.Write(" " + symbol + " ");
                }
            }
        }

        // Other methods (MakeMove, GetBoard, CheckWinner, IsBoardFull, etc.)
    }

}
