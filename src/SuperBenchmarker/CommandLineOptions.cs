using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SuperBenchmarker
{
    public class CommandLineOptions
    {
        [Option('c', "concurrency" ,Required = false, HelpText = "Number of concurrent requests", DefaultValue = 1)]
        public int Concurrency { get; set; }

        [Option('n', "numberOfRequests", Required = false, HelpText = "Total number of requests", DefaultValue = 100)]
        public int NumberOfRequests { get; set; }

        [Option('u', "url", Required = true, HelpText = "Total number of requests")]
        public string Url { get; set; }

        [Option('m', "method", Required = false, HelpText = "HTTP Method to use", DefaultValue = "GET")]
        public string Method { get; set; }

        [Option('t', "template", Required = false, HelpText = "Path to request template to use")]
        public string Template { get; set; }

        [Option('p', "plugin", Required = false, HelpText = "Name of the plugin (DLL) to replace placeholders. Must reside in the same folder.")]
        public string Plugin { get; set; }

        [Option('f', "file", Required = false, HelpText = "Path to CSV file providing replacement values for the test")]
        public string ValuesFile { get; set; }

        [Option('d', "dryRun", Required = false, HelpText = "Runs a single dry run request to make sure all is good")]
        public bool IsDryRun { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Provides verbose tracing information")]
        public bool Verbose { get; set; }

        [Option('k', "cookies", Required = false, HelpText = "Outputs cookies")]
        public bool OutputCookies { get; set; }

        [Option('x', "useProxy", Required = false, HelpText = "Whether to use default browser proxy. Useful for seeing request/response in Fiddler.")]
        public bool UseProxy { get; set; }

        [Option('q', "onlyRequest", Required = false, HelpText = "In a dry-run (debug) mode shows only the request.")]
        public bool OnlyRequest { get; set; }

        [Option('h', "onlyRequest", Required = false, HelpText = "In a dry-run (debug) mode shows only the request.")]
        public bool OutputHeaders { get; set; }

    }
}
