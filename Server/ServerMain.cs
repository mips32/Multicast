
using Configuration;
using System;

namespace Server
{
    class ServerMain
    {
        static void Main(string[] args)
        {
            CommonConfig config = new CommonConfig("config.xml");

            ServerThread st = new ServerThread(config, OnServerStarted, OnServerStopped);
            st.ThreadStart();

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
                        st.PrintStatistics();
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
                Console.Write("Enter command: ");
            }

            st.ThreadStop();
        }

        #region Events

        private static void OnServerStarted()
        {
            Console.WriteLine("Server started");
        }

        private static void OnServerStopped()
        {
            Console.WriteLine("Server stopped");
        }

        #endregion // Events

    }
}
