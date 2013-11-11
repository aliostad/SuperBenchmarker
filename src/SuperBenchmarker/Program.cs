using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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


            var requester = new Requester(commandLineOptions);
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
                                     statusCodes.Add(requester.Next(i));
                                     sw.Stop();
                                     timeTakens.Add(sw.ElapsedTicks);
                                     Interlocked.Increment(ref total);
                                     if(!commandLineOptions.Verbose)
                                        Console.Write("\r" + total);
                                 }
                );


            while (!result.IsCompleted)
            {
                Thread.Sleep(100);
            }
            stopwatch.Stop();
            var ordered = timeTakens.OrderBy(x => x).ToArray();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("---------------Finished!----------------");

            // ----- adding stats of statuses returned
            var stats = statusCodes.GroupBy(x => x)
                       .Select(y => new {Status = y.Key, Count = y.Count()}).OrderByDescending(z => z.Count);

            foreach (var stat in stats)
            {
                int statusCode = (int) stat.Status;
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
            Console.WriteLine("Max: " + (timeTakens.Max() / 10000) + "ms");
            Console.WriteLine("Min: " + (timeTakens.Min() / 10000) + "ms");
            Console.WriteLine("Avg: " + (timeTakens.Average() / 10000) + "ms");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine();
            Console.WriteLine("50% below " + (ordered[ordered.Length * 5 / 10])/10000 + "ms");
            Console.WriteLine("60% below " + (ordered[ordered.Length * 6 / 10])/10000 + "ms");
            Console.WriteLine("70% below " + (ordered[ordered.Length * 7 / 10])/10000 + "ms");
            Console.WriteLine("80% below " + (ordered[ordered.Length * 8 / 10])/10000 + "ms");
            Console.WriteLine("90% below " + (ordered[ordered.Length * 9 / 10])/10000 + "ms");

            Console.ForegroundColor = ConsoleColor.Gray;

        }
    }
}
