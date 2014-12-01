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
        static void Main(string[] args)
        {

          

            Console.ForegroundColor = ConsoleColor.Gray;

            ThreadPool.SetMinThreads(200, 100);
            ThreadPool.SetMaxThreads(1000, 200);
            var statusCodes = new ConcurrentBag<HttpStatusCode>();

            var commandLineOptions = new CommandLineOptions();
            bool isHelp = args.Any(x=>x=="-?");

            var success = Parser.Default.ParseArguments(args, commandLineOptions);

            if (!success || isHelp)
            {
                if(!isHelp && args.Length>0)
                    Console.Error.WriteLine("error parsing command line");
                return;
            }

            try
            {
                var requester = new Requester(commandLineOptions);
                var writer = new StreamWriter(commandLineOptions.LogFile) { AutoFlush = true };
                var stopwatch = Stopwatch.StartNew();
                var timeTakens = new ConcurrentBag<double>();
                int total = 0;
                Console.ForegroundColor = ConsoleColor.Cyan;
                var result = Parallel.For(0, commandLineOptions.IsDryRun ? 1 : commandLineOptions.NumberOfRequests,
                             new ParallelOptions()
                             {
                                 MaxDegreeOfParallelism = commandLineOptions.Concurrency
                             },
                                 (i) =>
                                 {
                                     var sw = Stopwatch.StartNew();
                                     IDictionary<string, object> parameters;
                                     var statusCode = requester.Next(i, out parameters);
                                     sw.Stop();
                                     statusCodes.Add(statusCode);
                                     timeTakens.Add(sw.ElapsedTicks);
                                     var n = Interlocked.Increment(ref total);
                                     Task.Run(() =>
                                        WriteLine(writer, n, (int)statusCode, sw.ElapsedMilliseconds, parameters));
                                     if (!commandLineOptions.Verbose)
                                         Console.Write("\r" + total);
                                 }
                    );


                while (!result.IsCompleted)
                {
                    Thread.Sleep(100);
                }
                stopwatch.Stop();
                double[] orderedList = (from x in timeTakens
                                        orderby x
                                        select x).ToArray<double>();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("---------------Finished!----------------");

                // ----- adding stats of statuses returned
                var stats = statusCodes.GroupBy(x => x)
                           .Select(y => new { Status = y.Key, Count = y.Count() }).OrderByDescending(z => z.Count);

                foreach (var stat in stats)
                {
                    int statusCode = (int)stat.Status;
                    if (statusCode >= 400 && statusCode < 600)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }

                    Console.WriteLine(string.Format("Status {0}:    {1}", statusCode, stat.Count));
                }

                Console.WriteLine();


                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("TPS: " + (commandLineOptions.NumberOfRequests * 1000 / stopwatch.ElapsedMilliseconds));
                Console.WriteLine(" (requests/second)");
                Console.WriteLine("Max: " + (timeTakens.Max() * 1000 / Stopwatch.Frequency) + "ms");
                Console.WriteLine("Min: " + (timeTakens.Min() * 1000 / Stopwatch.Frequency) + "ms");
                Console.WriteLine("Avg: " + (timeTakens.Average() * 1000 / Stopwatch.Frequency) + "ms");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine();
                Console.WriteLine("50%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(50M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                Console.WriteLine("60%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(60M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                Console.WriteLine("70%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(70M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                Console.WriteLine("80%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(80M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                Console.WriteLine("90%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(90M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                Console.WriteLine("95%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(95M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                Console.WriteLine("98%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(98M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                Console.WriteLine("99%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(99M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");
                Console.WriteLine("99.9%\tbelow " + Math.Round((double)((orderedList.Percentile<double>(99.9M) * 1000.0) / ((double)Stopwatch.Frequency))) + "ms");

            }
            catch (Exception exception)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception);
            }
 
            Console.ResetColor();
        }

        private static void WriteLine(StreamWriter writer, 
            int n,
            int statusCode, 
            long millis, 
            IDictionary<string, object> parameters)
        {
            lock (writer)
            {
                try
                {
                    var s = string.Join("\t", new[]
                    {
                        n.ToString(),
                        statusCode.ToString(),
                        millis.ToString(),
                    }.Concat(parameters.Select(x => x.Key + "=" + x.Value)));

                    writer.WriteLine(s);
                }
                catch (Exception e)
                {
                    // not to throw UNOBSERVED EXCEPTION
                    Trace.TraceWarning(e.ToString());
                }
                
            }
        }
    }
}
