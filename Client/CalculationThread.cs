using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class CalculationThread : IThread
    {
        public delegate void OnStatisticsRequestCallback(Statistics stat);
        public delegate void OnCalculationStartedCallback();
        public delegate void OnCalculationStoppedCallback();

        private event OnStatisticsRequestCallback onStatisticsRequest;
        private event OnCalculationStartedCallback onCalculationStarted;
        private event OnCalculationStoppedCallback onCalculationStopped;

        private ConcurrentQueue<byte[]> messageQueue;
        private AutoResetEvent signal;

        private Thread thread;

        private bool isRunning = false;

        private int packetCount = 0;

        private double deviation = 0.0;
        private int mode = 0;
        private double median = 0.0;
        private double mean = 0.0;
        private int sum = 0;

        private Statistics stat;
        private ConcurrentDictionary<int, Statistics> statistics;

        // Median data
        private List<int> sorted = new List<int>();

        // Mode data
        private SortedList<int, int> modeRaw = new SortedList<int, int>();

        private CalculationThread()
        {
        }

        public CalculationThread(ConcurrentQueue<byte[]> messageQueue, AutoResetEvent signal, OnStatisticsRequestCallback onStatisticsRequest, OnCalculationStartedCallback onCalculationStarted, OnCalculationStoppedCallback onCalculationStopped) : this()
        {
            this.modeRaw.Add(0, 0);

            this.stat = new Statistics()
            {
                Deviation = this.deviation,
                Mode = this.mode,
                Median = this.median,
                Mean = this.mean,
                Sum = this.sum,
                LostCount = 0,
            };
            this.statistics = new ConcurrentDictionary<int, Statistics>();
            this.statistics.AddOrUpdate(0, (key) => stat, (key, value) => stat);

            this.messageQueue = messageQueue;
            this.signal = signal;

            this.onStatisticsRequest += onStatisticsRequest;
            this.onCalculationStarted += onCalculationStarted;
            this.onCalculationStopped += onCalculationStopped;
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
            onCalculationStopped?.Invoke();
        }

        public void GetStatistics()
        {
            new Task(() =>
            {
                this.deviation = this.CalculateStandardDeviation(this.sorted);
                this.median = this.CalculateMedian(this.sorted);

                statistics[0].Deviation = this.deviation;
                statistics[0].Median = this.median;

                onStatisticsRequest?.Invoke(new Statistics()
                {
                    Deviation = statistics[0].Deviation,
                    Mode = statistics[0].Mode,
                    Median = statistics[0].Median,
                    Mean = statistics[0].Mean,
                    Sum = statistics[0].Sum,
                    LostCount = statistics[0].LostCount,
                });
            }).Start();
        }

        private void ThreadProc()
        {
            this.isRunning = true;
            onCalculationStarted?.Invoke();

            while (this.isRunning)
            {
                signal.WaitOne();

                byte[] data = null;
                while (messageQueue.TryDequeue(out data))
                {
                    int packetNumber = BitConverter.ToInt32(data, 0);
                    int packetData = BitConverter.ToInt32(data, 4);

                    this.packetCount++;
                    this.processData(packetNumber, packetData);
                }
            }
        }

        private void processData(int packetNumber, int packetData)
        {
            // Median data prepare
            int index = sorted.BinarySearch(packetData);
            if (index < 0)
                index = ~index;
            sorted.Insert(index, packetData);

            // Mode data prepare
            int temp;
            if (this.modeRaw.TryGetValue(packetData, out temp))
            {
                temp++;
                this.modeRaw[packetData] = temp;
            }
            else
            {
                temp = 1;
                this.modeRaw.Add(packetData, 1);
            }
            if (temp > this.modeRaw[this.mode])
                this.mode = packetData;

            // Simple precalculation
            this.deviation += 0.1;
            this.median += 0.1;
            this.sum += packetData;
            this.mean = this.sum / (double)this.packetCount;
            int lostPacketCount = packetNumber - this.packetCount;

            this.statistics[0].Deviation = this.deviation;
            this.statistics[0].Mode = this.mode;
            this.statistics[0].Median = this.median;
            this.statistics[0].Mean = this.mean;
            this.statistics[0].Sum = this.sum;
            this.statistics[0].LostCount = lostPacketCount;
        }

        private double CalculateStandardDeviation(List<int> values)
        {
            double res = 0.0;

            int count = values.Count;
            for (int i = 0; i < count; i++)
            {
                res += Math.Pow(values[i] - mean, 2);
            }

            return Math.Sqrt(res / (count - 1));
        }

        private double CalculateMedian(List<int> arr)
        {
            int idx = arr.Count / 2;
            if (arr.Count % 2 == 0)
                return ((double) (arr[idx] + arr[idx - 1]) / 2);
            else
                return arr[idx];
        }

    }

}
