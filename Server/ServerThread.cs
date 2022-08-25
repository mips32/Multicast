using Configuration;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public class ServerThread : IThread
    {
        public delegate void OnServerStartedCallback();
        public delegate void OnServerStoppedCallback();

        private event OnServerStartedCallback onServerStarted;
        private event OnServerStoppedCallback onServerStopped;

        private ICommonConfig config;

        private Thread thread;

        private bool isRunning = false;

        int count = 0;

        private ServerThread()
        {
        }

        public ServerThread(ICommonConfig config, OnServerStartedCallback onServerStarted, OnServerStoppedCallback onServerStopped) : this()
        {
            this.config = config;
            this.onServerStarted = onServerStarted;
            this.onServerStopped = onServerStopped;
        }

        public bool IsRunning()
        {
            return this.isRunning;
        }

        public void ThreadStart()
        {
            this.thread = new Thread(this.ThreadProc);
            this.thread.Start();
        }

        public void ThreadStop()
        {
            this.thread.Abort();
            this.isRunning = false;
            this.onServerStopped?.Invoke();
        }

        public void PrintStatistics()
        {
            Console.WriteLine("\n[ SERVER STATISTICS ]");
            Console.WriteLine("Total packets sent: {0,16}", count);
            Console.WriteLine();
        }


        private void ThreadProc()
        {
            // Create object UdpClient
            UdpClient udpClient = new UdpClient();

            // IPAddress of group
            IPAddress multicastaddress = IPAddress.Parse(this.config.IpAddress);

            try
            {
                // Join group
                udpClient.JoinMulticastGroup(multicastaddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }

            // Create object IPEndPoint
            IPEndPoint remoteep = new IPEndPoint(multicastaddress, this.config.Port);

            this.isRunning = true;

            count = 0;
            while (this.isRunning)
            {
                count++;

                Random rng = new Random();
                int number = rng.Next(this.config.MinValue, this.config.MaxValue);

                byte[] numberBytes = BitConverter.GetBytes(number);
                byte[] countBytes = BitConverter.GetBytes(count);

                byte[] data = new byte[8];
                Array.Copy(countBytes, 0, data, 0, countBytes.Length);
                Array.Copy(numberBytes, 0, data, 4, numberBytes.Length);

                try
                {
                    // Send data using IPEndPoint
                    udpClient.Send(data, data.Length, remoteep);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.ToString());
                    this.isRunning = false;
                    this.onServerStopped?.Invoke();
                }
#if DEBUG
                Thread.Sleep(TimeSpan.FromMilliseconds(this.config.ServerDelay));
#endif
            }

            // Leave the group
            udpClient.DropMulticastGroup(multicastaddress);

            // Close connection
            udpClient.Close();

        }

    }
}
