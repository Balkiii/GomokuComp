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
            List<IBot> bots = [new RandomBot()];

            Tournament tournament = new Tournament(bots);
            tournament.RunTournament(1000);

            Console.ReadLine();
        }
    }
}
