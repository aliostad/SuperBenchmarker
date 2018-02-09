using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace SuperBenchmarker
{
    class Program
    {
        public const string ResponseRegexExtractParamName = "###Response_Regex###";
        public const string JsonCount = "###Json_Count###";
        private static Stopwatch _stopwatch = new Stopwatch();

        private struct LogData
        {
            public DateTimeOffset EventDate;
            public int Index;
            public int StatusCode;
            public long Millis;
            public IDictionary<string, object> Parameters;
        }

        private static ConcurrentQueue<LogData> _logDataQueue = new ConcurrentQueue<LogData>();

        private static async Task<bool> ProcessLogQueueAsync(StreamWriter writer, bool donCapLogging, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    await writer.FlushAsync();
                    writer.Close();
                    return true;
                }

                LogData data;
                if (_logDataQueue.TryDequeue(out data))
                {

                    var s = string.Join("\t", new[]
                   {
                        data.EventDate.ToString(),
                        data.Index.ToString(),
                        data.StatusCode.ToString(),
                        data.Millis.ToString(),
                    }.Concat(data.Parameters.Select(x => {
                        var vs = x.Value.ToString();
                        return x.Key + "=" +
                            (donCapLogging ? vs : vs.Substring(0, Math.Min(vs.Length, 50)));
                        }
                    )));

                    try
                    {
                        await writer.WriteLineAsync(s);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.ToString());
                    }
                }
                else
                {
                    await Task.Delay(250, cancellationToken);
                }
            }
        }

        static void Main(string[] args)
        {

            // to cover our back for all those fire and forgets
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            ServicePointManager.DefaultConnectionLimit = 10000;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            Console.ForegroundColor = ConsoleColor.Gray;

            ThreadPool.SetMinThreads(200, 100);
            ThreadPool.SetMaxThreads(1000, 200);

            bool isHelp = args.Any(x => x == "-?");

            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(x => WithOptionDoItBoy(x))
                .WithNotParsed(y => Console.WriteLine(y));
        }

        private static void WithOptionDoItBoy(CommandLineOptions commandLineOptions)
        {
            var statusCodes = new ConcurrentBag<HttpStatusCode>();

            if (commandLineOptions.IsDryRun)
                commandLineOptions.NumberOfRequests = 1;

            if(commandLineOptions.TlsVersion.HasValue)
            {
                switch(commandLineOptions.TlsVersion.Value)
                {
                    case 0:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                        break;
                    case 1:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                        break;
                    case 2:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        break;
                    case 3:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                        break;
                    default:
                        throw new InvalidOperationException("TLS version not supported.");
                }

            }

            var then = DateTime.Now;
            ConsoleWriteLine(ConsoleColor.DarkCyan, "Starting at {0}", then);

            try
            {
                var requester = string.IsNullOrEmpty(commandLineOptions.TimeField)
                    ? (IAsyncRequester)new Requester(commandLineOptions)
                    : (IAsyncRequester)new TimeBasedRequester(commandLineOptions);

                var writer = new StreamWriter(commandLineOptions.LogFile) { AutoFlush = false };
                _stopwatch.Restart();
                var timeTakens = new ConcurrentBag<double>();
                if (commandLineOptions.SaveResponses)
                {
                    if (string.IsNullOrEmpty(commandLineOptions.ResponseFolder))
                    {
                        commandLineOptions.ResponseFolder = Path.Combine(Environment.CurrentDirectory, "Responses");
                    }

                    if (!Directory.Exists(commandLineOptions.ResponseFolder))
                        Directory.CreateDirectory(commandLineOptions.ResponseFolder);
                }

                ConsoleWriteLine(ConsoleColor.Yellow, "[Press C to stop the test]");
                int total = 0;
                var stop = new ConsoleKeyInfo();
                Console.ForegroundColor = ConsoleColor.Cyan;
                var source = new CancellationTokenSource(TimeSpan.FromDays(7));
                var logSourece = new CancellationTokenSource(TimeSpan.FromDays(7));

                Task.Run(() => ProcessLogQueueAsync(writer, commandLineOptions.DontCapLoggingParameters, logSourece.Token), logSourece.Token);

                Task.Run(() =>
                {
                    while (true)
                    {
                        stop = Console.ReadKey(true);
                        if (stop.KeyChar == 'c')
                            break;
                    }

                    ConsoleWriteLine(ConsoleColor.Red, "...");
                    ConsoleWriteLine(ConsoleColor.Green, "Exiting.... please wait! (it might throw a few more requests)");
                    ConsoleWriteLine(ConsoleColor.Red, "");
                    source.Cancel();

                }, source.Token); // NOT MEANT TO BE AWAITED!!!!

                Run(commandLineOptions, source, requester, statusCodes, timeTakens, total);
                total = timeTakens.Count;

                Console.WriteLine();
                _stopwatch.Stop();

                ConsoleWriteLine(ConsoleColor.Magenta, "---------------Finished!----------------");
                var now = DateTime.Now;
                ConsoleWriteLine(ConsoleColor.DarkCyan, "Finished at {0} (took {1})", now, now - then);

                // waiting for log to catch up
                Thread.Sleep(1000);

                source.Cancel();
                double[] orderedList = (from x in timeTakens
                                        orderby x
                                        select x).ToArray<double>();

                // ----- adding stats of statuses returned
                var stats = statusCodes.GroupBy(x => x)
                           .Select(y => new { Status = y.Key, Count = y.Count() }).OrderByDescending(z => z.Count);

                foreach (var stat in stats)
                {
                    int statusCode = (int)stat.Status;
                    if (statusCode >= 400 && statusCode < 600)
                    {
                        ConsoleWriteLine(ConsoleColor.Red, string.Format("Status {0}:    {1}", statusCode, stat.Count));
                    }
                    else
                    {
                        ConsoleWriteLine(ConsoleColor.Green, string.Format("Status {0}:    {1}", statusCode, stat.Count));
                    }

                }

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                if (!timeTakens.IsEmpty)
                {
                    Console.Write("TPS: " + Math.Round(total * 1000f / _stopwatch.ElapsedMilliseconds, 1));
                    Console.WriteLine(" (requests/second)");
                    Console.WriteLine("Max: " + (timeTakens.Max() * 1000 / Stopwatch.Frequency) + "ms");
                    Console.WriteLine("Min: " + (timeTakens.Min() * 1000 / Stopwatch.Frequency) + "ms");
                    Console.WriteLine("Avg: " + (timeTakens.Average() * 1000 / Stopwatch.Frequency) + "ms");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine();
                    Console.WriteLine("  50%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(50M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                    Console.WriteLine("  60%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(60M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                    Console.WriteLine("  70%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(70M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                    Console.WriteLine("  80%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(80M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                    Console.WriteLine("  90%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(90M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                    Console.WriteLine("  95%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(95M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                    Console.WriteLine("  98%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(98M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                    Console.WriteLine("  99%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(99M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                    Console.WriteLine("99.9%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(99.9M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                }

                Thread.Sleep(500);
                logSourece.Cancel();
                Thread.Sleep(500);

                writer.Flush();
            }
            catch (Exception exception)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception);
            }

            Console.ResetColor();
        }


        private static void Run(CommandLineOptions commandLineOptions, CancellationTokenSource source,
            IAsyncRequester requester, ConcurrentBag<HttpStatusCode> statusCodes, ConcurrentBag<double> timeTakens, int total)
        {
            var warmUpTotal = 0;
            var customThreadPool = new CustomThreadPool(new WorkItemFactory(
                requester,
                commandLineOptions.NumberOfRequests,
                commandLineOptions.DelayInMillisecond,
                commandLineOptions.WarmupSeconds), 
                source, 
                commandLineOptions.Concurrency, 
                commandLineOptions.WarmupSeconds);

            customThreadPool.WarmupFinished += CustomThreadPool_WarmupFinished;
            customThreadPool.WorkItemFinished += (sender, args) =>
            {
                if (args.Result.NoWork)
                    return;

                if (args.Result.IsWarmUp)
                {
                    var t = Interlocked.Increment(ref warmUpTotal);
                    if (!commandLineOptions.Verbose)
                        ConsoleWrite(ConsoleColor.Green, "\rWarmup [Users {1}]: {0}", warmUpTotal, customThreadPool.WorkerCount);

                    return;
                }

                statusCodes.Add((HttpStatusCode) args.Result.Status);
                timeTakens.Add(args.Result.Ticks);
                var n = Interlocked.Increment(ref total);
                var logData = new LogData()
                {
                    Millis = (int)args.Result.Ticks / TimeSpan.TicksPerMillisecond,
                    Index = args.Result.Index,
                    EventDate = DateTimeOffset.Now,
                    StatusCode = args.Result.Status,
                    Parameters = args.Result.Parameters
                };

                _logDataQueue.Enqueue(logData);

                if(!commandLineOptions.Verbose)
                    ConsoleWrite(ConsoleColor.DarkYellow, "\r{0}	(TPS: {1})			", total, Math.Round(total * 1000f / _stopwatch.ElapsedMilliseconds, 1));
            };

            customThreadPool.Start(commandLineOptions.NumberOfRequests);

            while (!source.IsCancellationRequested)
            {
                Thread.Sleep(200);
            }

        }

        private static void CustomThreadPool_WarmupFinished(object sender, EventArgs e)
        {
            _stopwatch.Restart();
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                Trace.WriteLine(e.Exception.ToString());
            }
            catch
            {

            }
        }

        internal static void ConsoleWrite(ConsoleColor color, string value, params object[] args)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value, args);
            Console.ForegroundColor = foregroundColor;
        }

        internal static void ConsoleWriteLine(ConsoleColor color, string value, params object[] args)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value, args);
            Console.ForegroundColor = foregroundColor;
        }

        class WorkItemFactory : IWorkItemFactory
        {
            private int _delayInMilli;
            private readonly int _warmupSeconds;
            private IAsyncRequester _requester;
            private ConcurrentQueue<int> _indices;
            private DateTimeOffset _start = DateTimeOffset.Now;
            private DateTimeOffset? _warmUpEnd;

            public WorkItemFactory(IAsyncRequester requester, int count, int delayInMilli = 0, int warmupSeconds = 0)
            {
                _requester = requester;
                _delayInMilli = delayInMilli;
                _warmupSeconds = warmupSeconds;
                _indices = new ConcurrentQueue<int>(Enumerable.Range(0, count));
                _warmUpEnd = warmupSeconds == 0 ? (DateTimeOffset?) null : _start.AddSeconds(warmupSeconds);
            }

            public async Task<WorkResult> GetWorkItem()
            {
                bool isWarmup = _warmUpEnd.HasValue
                    ? _warmUpEnd.Value > DateTimeOffset.Now
                    : false;

                int i = 0;
                var tryDequeue = _indices.TryDequeue(out i);
                if (!tryDequeue)
                    return new WorkResult()
                    {
                        NoWork = true
                    };

                var stopwatch = Stopwatch.StartNew();
                var result = await _requester.NextAsync(i);
                stopwatch.Stop();
                
                if(_delayInMilli > 0)
                    await Task.Delay(_delayInMilli);

                return new WorkResult()
                {
                    Status = (int) result.Item2,
                    Index = i,
                    Parameters = result.Item1,
                    Ticks = stopwatch.ElapsedTicks,
                    IsWarmUp = isWarmup
                };
            }
        }
    }
}
