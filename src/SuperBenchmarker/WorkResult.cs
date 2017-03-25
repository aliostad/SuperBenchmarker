using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public class WorkResult
    {
        public int Status { get; set; }

        public IDictionary<string, object> Parameters { get; set; }

        public int Index { get; set; }

        public double Ticks { get; set; }

        public bool NoWork { get; set; }
    }
}
