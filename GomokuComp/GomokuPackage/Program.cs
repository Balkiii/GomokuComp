using GomokuPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GomokuPackage
{
    class Program
    {
        static void Main(string[] args)
        {
            List<IBot> bots = new List<IBot>
            {
                // Add bots to do a bot tournament
                // If there is only one bot in the list, a human match will start
                new RandomBot(),
            };

            if (bots.Count == 1)
            {
                HumanGame.StartHumanGame(bots);
            }
            else if (bots.Count > 1)
            {
                // Run the tournament as before
                Tournament tournament = new Tournament(bots);
                int numberOfGamesToRunPerMatch = 1000;
                int millisecondsPerMove = 100;
                tournament.RunTournament(numberOfGamesToRunPerMatch, millisecondsPerMove);
            }
            else
            {
                throw new ArgumentException("You cannot start the match without any bots> or add only one bot to play against it as a human");
            }
        }
    }
}
