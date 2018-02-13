using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker.Reporting
{
    public class Slicer
    {
        private DateTimeOffset _previousSliceTime = DateTimeOffset.Now;
        private int _previousSliceResponseCount = 0;

        public void Restart()
        {
            _previousSliceResponseCount = 0;
            _previousSliceTime = DateTimeOffset.Now;
        }

        public Slice Slice(int concurrency, IList<HttpStatusCode> statuses, DateTimeOffset? cutTaken = null)
        {
            var stats = statuses.Skip(_previousSliceResponseCount).ToArray();
            DateTimeOffset toTime = cutTaken ?? DateTimeOffset.Now;
            double seconds = toTime.Subtract(_previousSliceTime).TotalMilliseconds / 1000;

            _previousSliceTime = DateTimeOffset.Now;
            _previousSliceResponseCount = statuses.Count;

            return new Slice()
            {
                Concurrency = concurrency,
                CutTaken = DateTimeOffset.Now,
                TotalRequests = statuses.Count,
                Rps = Math.Round(stats.Length / seconds, 1)
            };


        }
    }
}
