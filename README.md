SuperBenchmarker
================

Superbenchmarker is a load generator command-line tool for testing websites and HTTP APIsmeant to become Apache Benchmark (ab.exe) on steriod.

Features include:

* ability to do POST, PUT and DELETE as well as GET
* ability to provide request template (both headers and payload) using a text file
* ability to parameterise URL and template placeholders (doneted by {{{name}}}) using a CSV file or a plugin DLL
* tracing and troubleshooting output to see request, headers, cookies, URL generated, etc


Usage:
sb.exe -u url [-c concurrency] [-n numberOfRequests] [-m method] [-t template] [-p plu
gin] [-f file] [-d]  [-v]  [-k]  [-x]  [-q]  [-h]  [-?]
Parameters:
 -u     Required. Target URL to call. Can include placeholders.
 -c     Optional. Number of concurrent requests (default=1)
 -n     Optional. Total number of requests (default=100)
 -m     Optional. HTTP Method to use (default=GET)
 -t     Optional. Path to request template to use
 -p     Optional. Name of the plugin (DLL) to replace placeholders. Should contain one class which implements IValueProvider. Must reside in the same folder.
 -f     Optional. Path to CSV file providing replacement values for the test
 -d     Optional. Runs a single dry run request to make sure all is good (boolean switch)
 -v     Optional. Provides verbose tracing information (boolean switch)
 -k     Optional. Outputs cookies (boolean switch)
 -x     Optional. Whether to use default browser proxy. Useful for seeing request/response in Fiddler. (boolean switch)
 -q     Optional. In a dry-run (debug) mode shows only the request. (boolean switch)
 -h     Optional. Displays headers for request and response. (boolean switch)
 -?     Optional. Displays this help. (boolean switch)


Examples:

-u http://google.com
-u http://google.com -n 1000 -c 10
-u http://google.com -n 1000 -c 10 -d (runs only once)
-u http://localhost/api/myApi/ -t template text (file contains headers to be sent for GET. format is same as HTTP request)
-u http://localhost/api/myApi/ -m POST -t template.txt (file contains headers to be sent for GET. format is same as HTTP request with double CRLF separating headers and payload)
-u http://localhost/api/myApi/{{{ID}}} -f values.txt (values file is CSV and has a column for ID)-u http://localhost/api/myApi/{{{ID}}} -p myplugin.dll (has a public class implementing IValueProvider defined in this exe)
-u http://google.com -h (shows headers)
-u http://google.com -h -q (shows cookies)
-u http://google.com -v (shows some verbose information including URL to target - especially useful if parameterised) 

