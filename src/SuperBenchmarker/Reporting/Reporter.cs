using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperBenchmarker.Reporting
{
    public class Reporter
    {
        private Slicer _slicer = new Slicer();
        private string _commandLine;
        private readonly Func<int> _concurrencyProvider;
        private ConcurrentQueue<ResponseLog> _responses = new ConcurrentQueue<ResponseLog>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private ConcurrentQueue<Slice> _slices = new ConcurrentQueue<Slice>();
        private DateTimeOffset _start = DateTimeOffset.Now;
        private object _lock = new object();
        private int _lastSliceCount = 0;

        public int SliceSeconds { get; }

        public Reporter(string commandLine, int sliceSeconds, Func<int> concurrencyProvider)
        {
            _commandLine = commandLine;
            SliceSeconds = sliceSeconds;
            _concurrencyProvider = concurrencyProvider;
        }

        public void Start()
        {
            _start = DateTimeOffset.Now;
            _slices = new ConcurrentQueue<Slice>();
            _responses = new ConcurrentQueue<ResponseLog>();
            _slicer.Restart();
            RunSlices(_cts.Token); // DONT WAIT FOR IT
        }

        public void AddResponse(HttpStatusCode statusCode, long ticksTaken)
        {
            _responses.Enqueue(new ResponseLog() { StatusCode = statusCode, TicksTaken = ticksTaken });
        }

        private async Task RunSlices(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                await Task.Delay(SliceSeconds * 1000, token);
                _slices.Enqueue(_slicer.Slice(_concurrencyProvider(), _responses.ToList()));
            }
        }

        public Report InterimReport()
        {
            var doit = false;
            if(_lastSliceCount < _slices.Count)
            {
                lock (_lock)
                {
                    if (_lastSliceCount < _slices.Count)
                    {
                        doit = true;
                        _lastSliceCount = _slices.Count;
                    }
                }
            }

            return doit ? BuildReport() : null;
        }

        private Report BuildReport()
        {
            var millis = _responses.Select(x => (x.TicksTaken * 1000 / Stopwatch.Frequency)).ToArray();
            var r = new Report()
            {
                CommandLine = _commandLine,
                Start = _start,
                End = DateTimeOffset.Now,
                IsFinal = _cts.IsCancellationRequested,
                ReportingSliceSeconds = SliceSeconds,
                Slices = _slices.ToList(),
                Total = _responses.Count
            };

            if(millis.Any())
            {
                r.Percentiles = BuildPercentiles();
                r.StatusCodeSummary = BuildStatusSummary(_responses.Select(x => x.StatusCode)).ToList();
                r.Average = Math.Round(millis.Average(), 1);
                r.Max = (int)millis.Max();
                r.Min = (int)millis.Min();
                r.Rps = Math.Round(millis.Length * 1.0d / r.End.Subtract(r.Start).TotalSeconds, 1);
            }

            return r;
        }

        private Dictionary<decimal, int> BuildPercentiles()
        {
            int[] orderedList = (from x in _responses
                                  orderby x
                                  select x.TicksTaken).Select(y => (int) (y * 1000 / Stopwatch.Frequency)).ToArray();

            return new Dictionary<decimal, int>
            {
                {10M, orderedList.Percentile(10M) },
                {20M, orderedList.Percentile(20M) },
                {30M, orderedList.Percentile(30M) },
                {40M, orderedList.Percentile(40M) },
                {50M, orderedList.Percentile(50M) },
                {60M, orderedList.Percentile(60M) },
                {70M, orderedList.Percentile(70M) },
                {80M, orderedList.Percentile(80M) },
                {90M, orderedList.Percentile(90M) },
                {95M, orderedList.Percentile(95M) },
                {98M, orderedList.Percentile(98M) },
                {99M, orderedList.Percentile(99M) },
                {99.9M, orderedList.Percentile(99.9M) }
            };
        }

        internal static IEnumerable<KeyValuePair< int, int>> BuildStatusSummary(IEnumerable<HttpStatusCode> statuses)
        {
            return statuses.GroupBy(x => x)
                      .Select(y =>  new KeyValuePair<int, int>( (int) y.Key, y.Count()))
                      .OrderByDescending(z => z.Key);
        }

        public Report Finish(DateTimeOffset? end = null)
        {
            _cts.Cancel();
            return BuildReport();
        }
    }
}
