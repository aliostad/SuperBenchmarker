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
            StatusBreakdown = new Dictionary<int, int>();
        }

        public int TotalRequests { get; set; }

        public DateTimeOffset CutTaken { get; set; }

        public Dictionary<int, int> StatusBreakdown { get; set; }

        public double Rps { get; set; }

        public int Concurrency { get; set; }
    }
}
