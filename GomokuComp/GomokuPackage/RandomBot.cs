using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GomokuPackage
{
    using System;
    using System.Collections.Generic;

    public class RandomBot : IBot
    {
        public string Name => "RandomBot";

        private Random random = new Random();

        public (int x, int y) MakeMove(int[,] board, int player)
        {
            List<(int x, int y)> availableMoves = new List<(int x, int y)>();
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (board[i, j] == 0)
                        availableMoves.Add((i, j));
                }
            }
            if (availableMoves.Count > 0)
            {
                int index = random.Next(availableMoves.Count);
                return availableMoves[index];
            }
            return (-1, -1); // No moves available
        }
    }

}
