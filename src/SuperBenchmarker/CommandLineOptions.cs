using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SuperBenchmarker
{
    public class CommandLineOptions : CommandOption
    {
        [Option('c', "concurrency" ,Required = false, HelpText = "Number of concurrent requests", DefaultValue = 1)]
        public int Concurrency { get; set; }

        [Option('n', "numberOfRequests", Required = false, HelpText = "Total number of requests", DefaultValue = 100)]
        public int NumberOfRequests { get; set; }

        [Option('u', "url", Required = true, HelpText = "Target URL to call. Can include placeholders.")]
        public string Url { get; set; }

        [Option('m', "method", Required = false, HelpText = "HTTP Method to use", DefaultValue = "GET")]
        public string Method { get; set; }

        [Option('t', "template", Required = false, HelpText = "Path to request template to use")]
        public string Template { get; set; }

        [Option('p', "plugin", Required = false, HelpText = "Name of the plugin (DLL) to replace placeholders. Should contain one class which implements IValueProvider. Must reside in the same folder.")]
        public string Plugin { get; set; }

        [Option('l', "logfile", Required = false, HelpText = "Path to the log file storing run stats", DefaultValue = "run.log")]
        public string LogFile { get; set; }

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

        [Option('h', "headers", Required = false, HelpText = "Displays headers for request and response.")]
        public bool OutputHeaders { get; set; }

        [Option('?', "help", Required = false, HelpText = "Displays this help.")]
        public bool IsHelp { get; set; }

        [HelpOption('?', "help")]
        public string GetTheHelp()
        {
            return GetHelp();
        }

        [Example]
        public string GetExample()
        {
            return @"
-u http://google.com
-u http://google.com -n 1000 -c 10
-u http://google.com -n 1000 -c 10 -d (runs only once)
-u http://localhost/api/myApi/ -t template text (file contains headers to be sent for GET. format is same as HTTP request)
-u http://localhost/api/myApi/ -m POST -t template.txt (file contains headers to be sent for POST. format is same as HTTP request with double CRLF separating headers and payload)
-u http://localhost/api/myApi/{{{ID}}} -f values.txt (values file is CSV and has a column for ID)
-u http://localhost/api/myApi/{{{ID}}} -f values.txt -m POST -t template.xtx (values file is CSV and has a column for ID, also for all placeholders within the template file)
-u http://localhost/api/myApi/{{{ID}}} -p myplugin.dll (has a public class implementing IValueProvider defined in this exe)
-u http://localhost/api/myApi/{{{ID:RAND_INTEGER:[1000:2000]}}}  generates random integer for the field ID with the raneg 1000-2000
-u http://google.com -h (shows headers)
-u http://google.com -h -q (shows cookies) 
-u http://google.com -v (shows some verbose information including URL to target - especially useful if parameterised) 

";
        }

    }
}
