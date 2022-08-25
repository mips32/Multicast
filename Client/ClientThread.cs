using Configuration;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    public class ClientThread : IThread
    {
        public delegate void OnClientStartedCallback();
        public delegate void OnClientStoppedCallback();

        private event OnClientStartedCallback onClientStarted;
        private event OnClientStoppedCallback onClientStopped;

        private ConcurrentQueue<byte[]> messageQueue;
        private AutoResetEvent signal;

        // Create UDP client
        private UdpClient client;

        // Create new IPEndPoint
        private IPEndPoint localEp;

        // IP address
        private IPAddress multicastaddress;

        private ICommonConfig config;

        private Thread thread;

        private bool isRunning = false;

        private ClientThread()
        {
        }

        public ClientThread(ICommonConfig config, ConcurrentQueue<byte[]> messageQueue, AutoResetEvent signal, OnClientStartedCallback onClientStarted, OnClientStoppedCallback onClientStopped) : this()
        {
            this.config = config;
            this.messageQueue = messageQueue;
            this.signal = signal;
            this.onClientStarted = onClientStarted;
            this.onClientStopped = onClientStopped;
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
            this.onClientStopped?.Invoke();
        }

        private void ThreadProc()
        {
            // Initialization
            this.client = new UdpClient();
            this.localEp = new IPEndPoint(IPAddress.Any, this.config.Port);
            this.client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.client.Client.Bind(localEp);
            this.multicastaddress = IPAddress.Parse(this.config.IpAddress);

            try
            {
                client.JoinMulticastGroup(multicastaddress);
                this.isRunning = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                this.isRunning = false;
                onClientStopped?.Invoke();
                return;
            }

            onClientStarted?.Invoke();

            // Receive data
            while (this.isRunning)
            {
                byte[] data = client.Receive(ref localEp);

                messageQueue.Enqueue(data);
                signal.Set();

#if DEBUG
                Thread.Sleep(TimeSpan.FromMilliseconds(this.config.ClientDelay));
#endif
            }

            // Leave the group
            client.DropMulticastGroup(multicastaddress);

            // Close connection
            client.Close();
        }
    }

}
