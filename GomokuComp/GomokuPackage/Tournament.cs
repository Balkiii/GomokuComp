using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GomokuPackage
{
    using System.Collections.Generic;

    public class Tournament
    {
        private List<IBot> bots;

        public Tournament(List<IBot> bots)
        {
            this.bots = bots;
        }

        public void RunTournament(int gamesPerMatch, int millisecondsPerMove)
        {
            var results = new Dictionary<string, int>();
            
            foreach (var bot in bots)
                results[bot.Name] = 0;

            for (int i = 0; i < bots.Count; i++)
            {
                for (int j = i + 1; j < bots.Count; j++)
                {

                    RunMatch(bots[i], bots[j], gamesPerMatch, millisecondsPerMove, results);
                }
            }

            // Display results
            foreach (var result in results)
            {
                Console.WriteLine($"{result.Key}: {result.Value} wins");
            }
        }

        private void RunMatch(IBot bot1, IBot bot2, int games, int millisecondsPerMove, Dictionary<string, int> results)
        {
            for (int g = 0; g < games; g++)
            {
                var game = new GomokuGame(millisecondsPerMove);
                int winner;

                if (g%2 == 0)
                {
                    winner = PlayGame(game, bot1, bot2);
                    
                    if (winner == 1)
                        results[bot1.Name]++;
                    else if (winner == 2)
                        results[bot2.Name]++;
                }
                else
                {
                    winner = PlayGame(game, bot2, bot1);

                    if (winner == 1)
                        results[bot2.Name]++;
                    else if (winner == 2)
                        results[bot1.Name]++;
                }
            }
        }

        private int PlayGame(GomokuGame game, IBot bot1, IBot bot2)
        {
            while (!game.IsBoardFull())
            {
                var currentPlayer = game.GetCurrentPlayer();
                IBot bot = currentPlayer == 1 ? bot1 : bot2;

                game.sw.Start();
                var move = bot.MakeMove(game.GetBoard(), currentPlayer);

                if(game.RemainingTime() < 0)
                {
                    Console.WriteLine($"{bot.Name} timed out");
                    return 3 - currentPlayer;
                }

                if (game.MakeMove(currentPlayer, move.x, move.y))
                {
                    int winner = game.CheckWinner();
                    if (winner != 0)
                        return winner;
                    game.NextPlayer();
                }
                else
                {
                    // Invalid move, opponent wins
                    return 3 - currentPlayer;
                }
            }
            return 0; // Draw
        }
    }

}
