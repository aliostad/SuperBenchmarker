using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker.Reporting
{
    public class ResponseLog
    {
        public long TicksTaken { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
