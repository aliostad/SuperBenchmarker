using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public Slice Slice(int concurrency, IList<ResponseLog> responses, DateTimeOffset? cutTaken = null)
        {
            var stats = responses.Skip(_previousSliceResponseCount).ToArray();
            DateTimeOffset toTime = cutTaken ?? DateTimeOffset.Now;
            double seconds = toTime.Subtract(_previousSliceTime).TotalMilliseconds / 1000;

            _previousSliceTime = DateTimeOffset.Now;
            _previousSliceResponseCount = responses.Count;
            var median = 0d;
            if (stats.Length > 0)
            {
                var sorted = stats.Select(x => x.TicksTaken * 1000 / Stopwatch.Frequency).OrderByDescending(y => y).ToArray();
                median = Math.Round((double) sorted[sorted.Length / 2], 1);
            }
           

            return new Slice()
            {
                Concurrency = concurrency,
                CutTaken = DateTimeOffset.Now,
                TotalRequests = responses.Count,
                Rps = Math.Round(stats.Length / seconds, 1),
                StatusBreakdown = Reporter.BuildStatusSummary(stats.Select(x => x.StatusCode)).ToList(),
                AverageResponseTime = stats.Length == 0 ? 0 :
                    Math.Round(stats.Average(x => x.TicksTaken * 1000 / Stopwatch.Frequency), 1),
                MedianResponseTime = median
            };


        }
    }
}
