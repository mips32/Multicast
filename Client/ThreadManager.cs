using Configuration;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Client
{
    public class ThreadManager
    {
        private readonly ConcurrentQueue<byte[]> queue = new ConcurrentQueue<byte[]>();
        private AutoResetEvent signal = new AutoResetEvent(false);

        private ICommonConfig config;

        private IThread clientThread;
        private IThread calculationThread;

        private ThreadManager()
        {
        }

        public ThreadManager(ICommonConfig config) : this()
        {
            this.config = config;
        }

        public void Start()
        {
            clientThread = new ClientThread(config, queue, signal, OnClientStarted, OnClientStopped);
            calculationThread = new CalculationThread(queue, signal, OnPrintStatistics, OnCalculationStarted, OnCalculationStopped);

            clientThread.ThreadStart();
            calculationThread.ThreadStart();
        }

        public void Stop()
        {
            clientThread.ThreadStop();
            calculationThread.ThreadStop();
        }

        public void PrintStatistics()
        {
            ((CalculationThread) this.calculationThread).GetStatistics();
        }

        #region Events

        private void OnClientStarted()
        {
            Console.WriteLine("Client started");
        }

        private void OnClientStopped()
        {
            Console.WriteLine("Client stopped");
        }

        private void OnPrintStatistics(Statistics stat)
        {
            stat.PrintStatistics();
            Console.Write("Enter command: ");
        }

        private void OnCalculationStarted()
        {
            Console.WriteLine("Calculation started");
        }

        private void OnCalculationStopped()
        {
            Console.WriteLine("Calculation stopped");
        }

        #endregion // Events


    }
}
