using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SuperBenchmarker
{
    // letters left: e, g, o, ,
    public class CommandLineOptions : CommandOption
    {
        [Option('c', "concurrency" ,Required = false, HelpText = "Number of concurrent requests", Default = 1)]
        public int Concurrency { get; set; }

        [Option('n', "numberOfRequests", Required = false, HelpText = "Total number of requests", Default = 100)]
        public int NumberOfRequests { get; set; }

        [Option('y', "delayInMillisecond", Required = false, HelpText = "Delay in millisecond", Default = 0)]
        public int DelayInMillisecond { get; set; }

        [Option('u', "url", Required = true, HelpText = "Target URL to call. Can include placeholders.")]
        public string Url { get; set; }

        [Option('m', "method", Required = false, HelpText = "HTTP Method to use", Default = "GET")]
        public string Method { get; set; }

        [Option('t', "template", Required = false, HelpText = "Path to request template to use")]
        public string Template { get; set; }

        [Option('p', "plugin", Required = false, HelpText = "Name of the plugin (DLL) to replace placeholders. Should contain one class which implements IValueProvider. Must reside in the same folder.")]
        public string Plugin { get; set; }

        [Option('l', "logfile", Required = false, HelpText = "Path to the log file storing run stats", Default = "run.log")]
        public string LogFile { get; set; }

        [Option('f', "file", Required = false, HelpText = "Path to CSV file providing replacement values for the test")]
        public string ValuesFile { get; set; }

        [Option('a', "TSV", Required = false, HelpText = "If you provide a tab-separated-file (TSV) with -f option instead of CSV")]
        public bool IsTsv { get; set; }

        [Option('d', "dryRun", Required = false, HelpText = "Runs a single dry run request to make sure all is good")]
        public bool IsDryRun { get; set; }

        [Option('e', "timedField", Required = false, HelpText = "Designates a datetime field in data. If set, requests will be sent according to order and timing of records.")]
        public string TimeField { get; set; }

        [Option('g', "timedField", Required = false, HelpText = "Version of TLS used. Accepted values are 0, 1, 2 and 3 for TLS 1.0, TLS 1.1 and TLS 1.2 and SSL3, respectively")]
        public int? TlsVersion { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Provides verbose tracing information")]
        public bool Verbose { get; set; }

        [Option('b', "tokeniseBody", Required = false, HelpText = "Tokenise the body")]
        public bool TokeniseBody { get; set; }

        [Option('k', "cookies", Required = false, HelpText = "Outputs cookies")]
        public bool OutputCookies { get; set; }

        [Option('x', "useProxy", Required = false, HelpText = "Whether to use default browser proxy. Useful for seeing request/response in Fiddler.")]
        public bool UseProxy { get; set; }

        [Option('q', "onlyRequest", Required = false, HelpText = "In a dry-run (debug) mode shows only the request.")]
        public bool OnlyRequest { get; set; }

        [Option('h', "headers", Required = false, HelpText = "Displays headers for request and response.")]
        public bool OutputHeaders { get; set; }

        [Option('z', "saveResponses", Required = false, HelpText = "saves responses in -w parameter or if not provided in\\response_<timestamp>")]
        public bool SaveResponses { get; set; }

        [Option('w', "responsesFolder", Required = false, HelpText = "folder to save responses in if and only if -w parameter is set")]
        public string ResponseFolder { get; set; }

        [Option('?', "help", Required = false, HelpText = "Displays this help.")]
        public bool IsHelp { get; set; }

        [Option('C', "dontcap", Required = false, HelpText = "Don't Cap to 50 characters when Logging parameters")]
        public bool CapLoggingParameters { get; set; }

        [Option('R', "responseregex", Required =false, HelpText ="Regex to extract from response. If it has groups, it retrieves the last group.")]
        public string ResponseExtractionRegex { get; set; }

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
-u http://localhost/api/myApi/{{{ID}}} -f values.txt -m POST -t template.txt (values file is CSV and has a column for ID, also for all placeholders within the template file)
-u http://localhost/api/myApi/{{{ID}}} -f values.txt -m POST -t template.txt -a (using a TSV file instead of CSV)
-u http://localhost/api/myApi/{{{ID}}} -f values.txt -m POST -t templateWithParameterisedBody.txt -b (values file is CSV and has a column for ID, also for all placeholders within the template file. Body is text and has placeholders to be replaced)
-u http://localhost/api/myApi/{{{ID}}} -p myplugin.dll (has a public class implementing IValueProvider defined in this exe)
-u http://localhost/api/myApi/{{{ID:RAND_INTEGER:[1000:2000]}}}  generates random integer for the field ID with the raneg 1000-2000
-u http://google.com -h (shows headers)
-u http://google.com -h -q (shows cookies) 
-u http://google.com -n 1000 -c 1 -y 500 (send requests with a delay of 500ms) 
-u http://google.com -v (shows some verbose information including URL to target - especially useful if parameterised) 
-u http://google.com -z (stores responses under response folder in the working directory. Creates folder if does not exist)
-u http://google.com -z -w c:\temp\perfrun1 (stores responses in c:\temp\perfrun1. Creates folder if does not exist)
-u http://google.com -g 2 (Uses TLS 1.2)

";
        }

    }
}
