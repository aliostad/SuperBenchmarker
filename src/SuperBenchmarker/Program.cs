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
using Newtonsoft.Json;
using SuperBenchmarker.Reporting;

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

        private static void SetupSsl(CommandLineOptions commandLineOptions)
        {
            if (commandLineOptions.TlsVersion.HasValue)
            {
                switch (commandLineOptions.TlsVersion.Value)
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
        }

        private static void WithOptionDoItBoy(CommandLineOptions commandLineOptions)
        {
            if (commandLineOptions.IsDryRun)
                commandLineOptions.NumberOfRequests = 1;

            SetupSsl(commandLineOptions);

            var then = DateTime.Now;
            var reportFolder = Path.Combine(Environment.CurrentDirectory, then.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(reportFolder);

            ConsoleWriteLine(ConsoleColor.DarkCyan, "Starting at {0}", then);

            try
            {
                var requester = string.IsNullOrEmpty(commandLineOptions.TimeField)
                    ? (IAsyncRequester)new Requester(commandLineOptions)
                    : (IAsyncRequester)new TimeBasedRequester(commandLineOptions);

                var writer = new StreamWriter(commandLineOptions.LogFile) { AutoFlush = false };
                _stopwatch.Restart();
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

                var reporter = Run(commandLineOptions, source, requester, total, reportFolder);

                Console.WriteLine();
                _stopwatch.Stop();

                ConsoleWriteLine(ConsoleColor.Magenta, "---------------Finished!----------------");
                var now = DateTime.Now;
                ConsoleWriteLine(ConsoleColor.DarkCyan, "Finished at {0} (took {1})", now, now - then);

                // waiting for log to catch up
                Thread.Sleep(1000);

                source.Cancel();
                var report = reporter.Finish();
                SaveReport(report, reportFolder);

                foreach (var stat in report.StatusCodeSummary)
                {
                    int statusCode = stat.Key;
                    if (statusCode >= 400 && statusCode < 600)
                    {
                        ConsoleWriteLine(ConsoleColor.Red, string.Format("Status {0}:    {1}", statusCode, stat.Value));
                    }
                    else
                    {
                        ConsoleWriteLine(ConsoleColor.Green, string.Format("Status {0}:    {1}", statusCode, stat.Value));
                    }
                }

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                if (report.Percentiles.Any())
                {
                    Console.Write("RPS: " + report.Rps);
                    Console.WriteLine(" (requests/second)");
                    Console.WriteLine("Max: " + report.Max + "ms");
                    Console.WriteLine("Min: " + report.Min + "ms");
                    Console.WriteLine("Avg: " + report.Average + "ms");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine();
                    Console.WriteLine("  50%\tbelow " + report.Percentiles[50] + "ms");
                    Console.WriteLine("  60%\tbelow " + report.Percentiles[60] + "ms");
                    Console.WriteLine("  70%\tbelow " + report.Percentiles[70] + "ms");
                    Console.WriteLine("  80%\tbelow " + report.Percentiles[80] + "ms");
                    Console.WriteLine("  90%\tbelow " + report.Percentiles[90] + "ms");
                    Console.WriteLine("  95%\tbelow " + report.Percentiles[95] + "ms");
                    Console.WriteLine("  98%\tbelow " + report.Percentiles[98] + "ms");
                    Console.WriteLine("  99%\tbelow " + report.Percentiles[99] + "ms");
                    Console.WriteLine("99.9%\tbelow " + report.Percentiles[99.9M] + "ms");
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


        private static Reporter Run(CommandLineOptions commandLineOptions, CancellationTokenSource source,
            IAsyncRequester requester, int total, string reportFolder)
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

            var reporter = new Reporter(Environment.CommandLine, commandLineOptions.ReportSliceSeconds, () => customThreadPool.WorkerCount);
            if (commandLineOptions.WarmupSeconds == 0)
                reporter.Start();

            customThreadPool.WarmupFinished += (sender, args) =>
            {
                _stopwatch.Restart();
                reporter.Start();
            };

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

                reporter.AddResponse((HttpStatusCode)args.Result.Status, (long) args.Result.Ticks);
                ConsiderUpdatingReport(reporter, reportFolder);

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
                    ConsoleWrite(ConsoleColor.DarkYellow, "\r{0}	(RPS: {1})			", total, Math.Round(total * 1000f / _stopwatch.ElapsedMilliseconds, 1));
            };

            customThreadPool.Start(commandLineOptions.NumberOfRequests);

            while (!source.IsCancellationRequested)
            {
                Thread.Sleep(200);
            }

            return reporter;
        }

        private static void ConsiderUpdatingReport(Reporter reporter, string reportFolder)
        {
            var report = reporter.InterimReport();
            if(report != null)
            {
                SaveReport(report, reportFolder);
            }
        }

        private static void SaveReport(Report report, string reportFolder)
        {
            try
            {
                File.WriteAllText(Path.Combine(reportFolder, 
                    report.IsFinal ? "report.json" : "interim.json"), 
                    JsonConvert.SerializeObject(report));
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
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
