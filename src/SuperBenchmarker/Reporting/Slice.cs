using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker.Reporting
{
    public class Slice
    {
        public Slice()
        {
            StatusBreakdown = new List<KeyValuePair<int, int>>();
        }

        public int TotalRequests { get; set; }

        public DateTimeOffset CutTaken { get; set; }

        public List<KeyValuePair<int, int>> StatusBreakdown { get; set; }

        public double Rps { get; set; }

        public int Concurrency { get; set; }

        public double AverageResponseTime { get; set; }

        public double MedianResponseTime { get; set; }

    }
}
