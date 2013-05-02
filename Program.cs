using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            ThreadPool.SetMinThreads(200, 100);
            ThreadPool.SetMaxThreads(1000, 200);

            var commandLineOptions = new CommandLineOptions();
            var success = Parser.Default.ParseArguments(args, commandLineOptions);

            if (!success)
            {
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
                                     requester.Next(i);
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
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("---------------Finished!----------------");
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("TPS: " + (commandLineOptions.NumberOfRequests * 1000 / stopwatch.ElapsedMilliseconds));
            Console.WriteLine("Max: " + (timeTakens.Max() / 10000) + "ms");
            Console.WriteLine("Min: " + (timeTakens.Min() / 10000) + "ms");
            Console.WriteLine("Avg: " + (timeTakens.Average() / 10000) + "ms");
            Console.ResetColor();
        }
    }
}
