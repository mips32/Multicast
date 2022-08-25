using System;

namespace Client
{
    public class Statistics
    {
        public double Deviation { get; set; }
        public double Mode { get; set; }
        public double Median { get; set; }
        public double Mean { get; set; }
        public int Sum { get; set; }
        public int LostCount { get; set; }

        public void PrintStatistics()
        {
            Console.WriteLine("\n[ CLIENT STATISTICS ]");
            Console.WriteLine("Deviation........: {0,16}", this.Deviation);
            Console.WriteLine("Mode.............: {0,16}", this.Mode);
            Console.WriteLine("Median...........: {0,16}", this.Median);
            Console.WriteLine("Mean.............: {0,16}", this.Mean);
            Console.WriteLine("Sum..............: {0,16}", this.Sum);
            Console.WriteLine("Lost packet count: {0,16}", this.LostCount);
            Console.WriteLine();
        }
    }
}
