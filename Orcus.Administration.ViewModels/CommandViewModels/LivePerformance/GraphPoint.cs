using System;

namespace Orcus.Administration.ViewModels.CommandViewModels.LivePerformance
{
    public class GraphPoint
    {
        public GraphPoint()
        {
        }

        public GraphPoint(DateTime time, double percentage, double memoryUsage)
        {
            Time = time;
            Percentage = percentage;
            MemoryUsage = memoryUsage;
        }

        public DateTime Time { get; set; }
        public double Percentage { get; set; }
        public double MemoryUsage { get; set; }
    }
}