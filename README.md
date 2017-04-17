SuperBenchmarker
================

Superbenchmarker is a load generator command-line tool for testing websites and HTTP APIs, and is meant to become Apache Benchmark (ab.exe) on steriods. **To run it you need .NET 4.5.**

Visit [Wiki](https://github.com/aliostad/SuperBenchmarker/wiki)

Features include:

* ability to do POST, PUT and DELETE as well as GET
* ability to provide request templates (both headers and payload) using a text file
* ability to parameterise URLs and template placeholders (denoted by {{{name}}}) using a CSV file or a plugin DLL
* tracing and troubleshooting of output to see request, headers, cookies, URLs generated, etc
* colourful commandline output to assist performance bottleneck investigation

![screenshot](https://raw.github.com/aliostad/SuperBenchmarker/master/SuperBenchmarker2.png)

### Install using chocolatey
If you have installed chocolatey (if not [here](https://github.com/chocolatey/chocolatey/wiki/Installation#command-line) are the instructions), you can install this by running

```
cinst SuperBenchmarker 
```

### Download
You may download the latest binary from download [folder](https://github.com/aliostad/SuperBenchmarker/tree/master/download) in GitHub source.


### Usage

sb.exe -u url [-c concurrency] [-n numberOfRequests] [-m method] [-t template] [-p plugin] [-f file] [-d]  [-v]  [-k]  [-x]  [-q]  [-h]  [-?]<br/>
Parameters:<br/>
- -u     Required. Target URL to call. Can include placeholders.<br/>
- -c     Optional. Number of concurrent requests (default=1)<br/>
- -n     Optional. Total number of requests (default=100)<br/>
- -m     Optional. HTTP Method to use (default=GET)<br/>
- -p     Optional. Name of the plugin (DLL) to replace placeholders. Should contain one class which implements IValueProvider. Must reside in the same folder.<br/>
- -f     Optional. Path to CSV file providing replacement values for the test<br/>
- -d     Optional. Runs a single dry run request to make sure all is good (boolean switch)<br/>
- -v     Optional. Provides verbose tracing information (boolean switch)<br/>
- -k     Optional. Outputs cookies (boolean switch)<br/>
- -x     Optional. Whether to use default browser proxy. Useful for seeing request/response in Fiddler. (boolean switch)<br/>
- -q     Optional. In a dry-run (debug) mode shows only the request. (boolean switch)<br/>
- -h     Optional. Displays headers for request and response. (boolean switch)<br/>
- -y     Optional. Number of milliseconds to delay each request with. Will not affetc the results - other than less throughput.<br/>
- -l     Optional. Location of the log file.<br/>
- -?     Optional. Displays this help. (boolean switch)<br/>
<br/>

### Examples

- -u http://google.com
- -u http://google.com -n 1000 -c 10
- -u http://google.com -n 1000 -c 10 -d (runs only once)
- -u http://localhost/api/myApi/ -t template text (file contains headers to be sent for GET. format is same as HTTP request)
- -u http://localhost/api/myApi/ -m POST -t template.txt (file contains headers to be sent for POST. format is same as HTTP request with double CRLF separating headers and payload)
- -u http://localhost/api/myApi/{{{ID}}} -f values.txt (values file is CSV and has a column for ID)
- -u http://localhost/api/myApi/{{{ID}}} -f values.txt -m POST -t template.txt (values file is CSV and has a column for ID, also for all placeholders within the template file)
- -u http://localhost/api/myApi/{{{ID}}} -f values.txt -m POST -t templateWithParameterisedBody.txt -b (values file is CSV and has a column for ID, also for all placeholders within the template file. Body is text and has placeholders to be replaced)
- -u http://localhost/api/myApi/{{{ID}}} -p myplugin.dll (has a public class implementing IValueProvider defined in this exe)
- -u http://localhost/api/myApi/{{{ID:RAND_INTEGER:[1000:2000]}}}  generates random integer for the field ID with the raneg 1000-2000
- -u http://google.com -h (shows headers)
- -u http://google.com -h -q (shows cookies) 
- -u http://google.com -n 1000 -c 1 -y 500 (send requests with a delay of 500ms) 
- -u http://google.com -v (shows some verbose information including URL to target - especially useful if parameterised) 

