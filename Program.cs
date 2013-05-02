using System;
using System.Collections.Generic;
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
            var result = Parallel.For(0, commandLineOptions.IsDryRun ? 1 : commandLineOptions.NumberOfRequests,
                         new ParallelOptions()
                             {
                                 MaxDegreeOfParallelism = commandLineOptions.Concurrency
                             },
                             requester.Next
                );


            while (!result.IsCompleted)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("---------------Finished!");
            
        }
    }
}
