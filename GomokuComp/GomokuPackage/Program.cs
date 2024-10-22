using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace GomokuPackage
{

    class Program
    {
        static void Main(string[] args)
        {
            List<IBot> bots = [new RandomBot()]; // Modify list contents to have different bots compete against each other

            Tournament tournament = new Tournament(bots);

            int numberOfGamesToRunPerMatch = 1000;
            int millisecondsPerMove = 100;
            tournament.RunTournament(numberOfGamesToRunPerMatch, millisecondsPerMove);

            Console.ReadLine();
        }
    }
}
