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
            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string botsDirectory = Path.Combine(projectRoot, "botloader");

            if (!Directory.Exists(botsDirectory))
            {
                Console.WriteLine($"Bots directory not found at {botsDirectory}. Creating it.");
                Directory.CreateDirectory(botsDirectory);
            }

            Console.WriteLine($"Attempting to load bots from {botsDirectory}");
            List<IBot> bots = LoadBots(botsDirectory);
            Console.WriteLine($"Total bots loaded: {bots.Count}");

            Tournament tournament = new Tournament(bots);
            tournament.RunTournament(1000);

            Console.ReadLine();
        }

        public static List<IBot> LoadBots(string botsDirectory)
        {
            List<IBot> bots = new List<IBot>();

            // Ensure bots directory exists
            if (!Directory.Exists(botsDirectory))
            {
                Console.WriteLine($"Bots directory not found at {botsDirectory}. Creating it.");
                Directory.CreateDirectory(botsDirectory);
            }

            Console.WriteLine($"Attempting to load bots from {botsDirectory}");

            foreach (var file in Directory.GetFiles(botsDirectory, "*.dll"))
            {
                Console.WriteLine($"Found file: {file}");
                try
                {
                    Assembly assembly = Assembly.LoadFrom(file);
                    Console.WriteLine($"Loaded assembly: {assembly.FullName}");
                    foreach (Type type in assembly.GetTypes())
                    {
                        Console.WriteLine($"Found type: {type.FullName}");
                        if (typeof(IBot).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            Console.WriteLine($"Type {type.FullName} implements IBot");
                            IBot bot = (IBot)Activator.CreateInstance(type);
                            bots.Add(bot);
                            Console.WriteLine($"Loaded bot: {bot.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"Type {type.FullName} does not implement IBot");
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine($"Failed to load types from {file}: {ex.Message}");
                    foreach (var loaderException in ex.LoaderExceptions)
                    {
                        Console.WriteLine($"LoaderException: {loaderException.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load bot from {file}: {ex.Message}");
                }
            }

            Console.WriteLine($"Total bots loaded: {bots.Count}");
            return bots;
        }




    }

}
