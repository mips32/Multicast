using System;
using Configuration;

namespace Client
{
    class ClientMain
    {
        static void Main(string[] args)
        {
            CommonConfig config = new CommonConfig("config.xml");

            ThreadManager tm = new ThreadManager(config);
            tm.Start();

            const string COMMAND_HELP = "Commands: 'Enter' or stat - statistics, exit - exit program, help - this help";

            bool isExit = false;
            string msg = String.Empty;
            Console.WriteLine(COMMAND_HELP);
            while (!isExit)
            {
                msg = Console.ReadLine();

                switch (msg)
                {
                    case "":
                    case "stat":
                        tm.PrintStatistics();
                        break;
                    case "exit":
                        isExit = true;
                        break;
                    case "help":
                        Console.WriteLine(COMMAND_HELP);
                        break;
                    default:
                        Console.WriteLine("Wrong command");
                        break;
                }

            }

            tm.Stop();

        }

    }

}
