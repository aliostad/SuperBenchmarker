SuperBenchmarker
================

Superbenchmarker is a load generator command-line tool for testing websites and HTTP APIsmeant to become Apache Benchmark (ab.exe) on steriod.

Features include:

* ability to do POST, PUT and DELETE as well as GET
* ability to provide request template (both headers and payload) using a text file
* ability to parameterise URL and template placeholders (doneted by {{{name}}}) using a CSV file or a plugin DLL
* tracing and troubleshooting output to see request, headers, cookies, URL generated, etc

<br/>
Usage:<br/>
sb.exe -u url [-c concurrency] [-n numberOfRequests] [-m method] [-t template] [-p plugin] [-f file] [-d]  [-v]  [-k]  [-x]  [-q]  [-h]  [-?]<br/>
Parameters:<br/>
 -u     Required. Target URL to call. Can include placeholders.<bre/>
 -c     Optional. Number of concurrent requests (default=1)<br/>
 -n     Optional. Total number of requests (default=100)<br/>
 -m     Optional. HTTP Method to use (default=GET)<br/><br/>
 -p     Optional. Name of the plugin (DLL) to replace placeholders. Should contain one class which implements IValueProvider. Must reside in the same folder.<br/>
 -f     Optional. Path to CSV file providing replacement values for the test<br/>
 -d     Optional. Runs a single dry run request to make sure all is good (boolean switch)<br/>
 -v     Optional. Provides verbose tracing information (boolean switch)<br/>
 -k     Optional. Outputs cookies (boolean switch)<br/>
 -x     Optional. Whether to use default browser proxy. Useful for seeing request/response in Fiddler. (boolean switch)<br/>
 -q     Optional. In a dry-run (debug) mode shows only the request. (boolean switch)<br/>
 -h     Optional. Displays headers for request and response. (boolean switch)<br/>
 -?     Optional. Displays this help. (boolean switch)<br/>
<br/>

Examples:<br/>
<br/>
-u http://google.com<br/>
-u http://google.com -n 1000 -c 10<br/>
-u http://google.com -n 1000 -c 10 -d (runs only once)<br/>
-u http://localhost/api/myApi/ -t template text (file contains headers to be sent for GET. format is same as HTTP request)<br/>
-u http://localhost/api/myApi/ -m POST -t template.txt (file contains headers to be sent for GET. format is same as HTTP request with double CRLF separating headers and payload)<br/>
-u http://localhost/api/myApi/{{{ID}}} -f values.txt (values file is CSV and has a column for ID)-u http://localhost/api/myApi/{{{ID}}} -p myplugin.dll (has a public class implementing IValueProvider defined in this exe)<br/>
-u http://google.com -h (shows headers)<br/>
-u http://google.com -h -q (shows cookies)<br/>
-u http://google.com -v (shows some verbose information including URL to target - especially useful if parameterised) <br/>

