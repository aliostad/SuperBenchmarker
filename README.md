SuperBenchmarker
================

Superbenchmarker is a load generator command-line tool for performance testing HTTP APIs and websites. Inspired by Apache Benchmark, it is meant to become Apache Benchmark (ab.exe) on steriods. It displays final results at the end of the test in the terminal window but it also constantly reports back in a web interface.

![screenshot](https://raw.github.com/aliostad/SuperBenchmarker/master/sb-reports.png)

# Gettting Started

## What you need
Superbenchmarker (sb) runs on Windows or Mac (not tested yet on Linux) and requires .NET 4.52+ or .NET Core 2.0+ installed on the box.

## Installation - Windows
Easiest way to install sb is to use [chocolatey](https://github.com/chocolatey/chocolatey/wiki/Installation#command-line). Once you have installed chocolatey, simply run:

``` bash
> cinst SuperBenchmarker
```
and to update your version of sb:

``` bash
> cup SuperBenchmarker
```

You can also download the lastest version from the `Download` [folder](https://github.com/aliostad/SuperBenchmarker/tree/master/download) of this github repository. This is a single exe with all dependencies IL-merged.

## Installation - Mac
Currently, until `brew` is sorted out, you need to build from the source:

``` bash
git clone https://github.com/aliostad/SuperBenchmarker
cd SuperBenchmarker
./build.sh
```
And then you can run using `dotnet` command:

``` bash
dotnet ./src/SuperBenchmarker/bin/Debug/netcoreapp2.0/SuperBenchmarker.dll -u https://google.com -N 10
```

## Running it and basic command parameters
To run it, just point it to a website or API using `-u (--url)` parameter:

``` bash
sb -u "http://example.com"
```
This command fires 100 GET requests using a single thread, which is equivalent to running command below:

``` bash
sb -u "http://example.com" -c 1 -n 100
```

So you can use `-c` and `-n` to change concurrency or total number of requests. Be careful not to use a very high `-c` on a single box since it will affect the result of the test. Watchout for CPU consumption on the box and make sure it is < 70%.

In order to run your test for a period of time rather than total number of requests, use `-N` to specify number of seconds. Command below runs the test for half an hour:

``` bash
sb -u "http://example.com" -c 4 -N 1800
```

You can use `-c` to increase and `-y (delayInMilliseconds)` to decrease in order to control the load on your server and achieve the desired throughput.

To use a different HTTP verb, use `-m (--method)` parameter:

``` bash
sb -u "http://example.com/api/car/123" -n 1000 -m DELETE
```

To dry-run a particular set-up you can use `-d (--dryRun)` which fires a single requests and outputs the result.

# Taking it to the next level
## Send custom headers/payload to the API
In order to send custom headers (or payload), you need a template file. A template file is a text file which - similar to HTTP - has headers and values in each line, then an empty line, and in the end the payload (assuming you are sending a text payload).

For example, to send an Authorization header to the server, create a text file called `template.txt` with this content:

``` 
Authorization: Basic GV5ITpub3Bhc3N3b3JkaGVyZQ==
```

And then run it using `-t (--template)`:

``` bash
sb -u "http://example.com/api/car/123" -n 1000 -t template.txt
```

Another example, to make a POST request sending a JSON payload, create this file (**NOTE** the empty line between headers and body):

```
Content-Type: application/json

{
    "foo": "bar"
}
```

## Parameterise the requests
It is all well and good to keep sending the same request but in order to truly test your application, you would need to make different requests. You can parameterise the URL and the template. Parameters are defined by triple curly braces:

``` bash
sb -u "http://example.com/api/car/{{{carId}}}"
```

in the above, we defined a parameter named *carId*. sb has built-in support for sending random STRING, DATE, DATETIME, DATETIMEOFFSET, INTEGER, DOUBLE, NAME and GUID - and for some data type it can do ranges. For example:

``` bash
sb -u "http://example.com/api/car/{{{carId:RAND_INTEGER:[1:1000000]}}}"
```

sends random integers in the range of 1 and 1,000,000.

Of course, random values do not always work and data needs to be taken from a pool of known values. In this case you can create a data file containing headers, matching the parameter names you have. (By default, only URL will be parameterised and if you need to parameterise the body too, you need to add flag `-b` too(. You pass the name of the file using `-f (file)` which assumes the file is a CSV. You can shuffle the records using `-U (--shuffleData)` flag. TSV is also supported using option `-a (--TSV)`, here with shuffling the data:

``` bash
sb -u "http://example.com/api/car/{{{carId}}}" -f carIds.tsv -a -U
```

Let's look an example. So let's say you are testing car update API (which needs a PUT method with a payload) and you need to parameterise car id and their prices. Prepare a data file called testdata.csv:

```
carId,price
123,6000
456,8900
...
```

And create a template file (template.txt):

```
some-header: some-value

{
    "price": {{{price}}}
}
```

And now run your test. **NOTE** that `-b` needs to be supplied to parameterise body:

``` bash
sb -u "http://example.com/api/car/{{{carId}}}" -f testdata.csv -t template.txt -m PUT -b
```

Values from your dataset will be used to populate and send requests. Once it has reached the end of the file, it will start from the beginning.

You can also create a data provider *plugin* to fully control the process. See below for more information.

## Real-time report chart/UI
Soon after running the test, your browser will be directed to a file which gets updated in real-time with the progress of your test. In case you do not this to happen, simple use `-B (--dontBrowseToReports)` flag. If you have used `-B` flag and but then changed your mind and you decide to see the progress, simply browse the file `index.html` in subfolder (inside the execution folder) with a name representing timestamp the test started (e.g. 2018-02-16_10-12-53.898088). If you would like to change the name, use `-F` option and provide an alternative folder name.

This folder, after the test finishes, will contain all information you need about your test. You can simply zip it up and keep it for archiving, and go back it to when you need to. The folder contains an `interim.js` file while the test is running and gets replaced with the "final.js" after the test finishes. These files contain the data powering charts.

Interacting with the chart is straightforward. The chart gets updated every 3 seconds and the thickness of the cyan vertical line shows the precision of mouse movements for navigating the chart. As more data arrives, the vertical line gets thinner, i.e. more precision. You can pause/unpause the chart update by clicking on the chart.  

The data slices are taken every 3 seconds. You can change it by using option `-P (--reportSliceSeconds)`. For short burst tests perhaps using one second is more appropriate (`-P 1`):

``` bash
sb -u "http://example.com/api/cars -N 60 -P 1
```

## Parameters to control terminal output
The terminal output is by no means a second class citizen. For debugging purposes that is the best option, e.g. to be able to see the request, headers, etc. Bear in mind, outputting more content to the terminal can impact the test results hence only useful for debugging.

![screenshot](https://raw.github.com/aliostad/SuperBenchmarker/master/SuperBenchmarker2.png)

Here are options to control the output:

 - `-v (--verbose)`: outputs verbose
 - `-h (--headers)`: output headers
 - `-k (--cookies)`: output cookies
 - `-q (--onlyRequest)`: outputs only request information

## Capturing response information
At a minimum, you will be interested in the response time and status code. sb already provides a detailed breakdown both in the terminal output and in its report. However, sometimes you would like to dig deeper or know which requests failed and in these cases, what were the value of the parameters sent to the server. 

sb logs all such information in its `run.log` file, stored inside the folder where the report files get stored (see above). You may change the name of the file or its location using `-l (--logFile)` option, passing the file name (or full path to change the location). This file is a non-headered tab-separated TSV which contains these columns:

 - datetime - after which the response received and its body consumed
 - index of the request (they can be out of order)
 - numeric value of the HTTP status code
 - time taken in millisecond
 - one column per each parameter in the format of `<name>=<value>`. These parameters include jsonCount or regex extraction (see below). By default, value of the parameter is truncated and capped to 50 characters. To prevent that, use flag `-C (--dontcap)`

Sometimes you might be interested to save the whole response body. In this case, use flag `-z` to store responses in "responses" subfolder. If you would like to store in another folder, use `-w`:

``` bash
sb -u "http://example.com/api/cars" -z -w "anotherFolder"
```

Occasionaly, all you need is a small piece of information out of the response body. For example, you would like to see how many records returned in the JSON response. You can use option `-j (--jsonCount)` with the tree location of element containing array. For example, in this JSON response:

``` json
{
    "foo": {
        "bar": [
            {...},
            {...},
            ...
        ]
    }
}
```
the array is located at path "foo/bar" and you would use this command to capture the count in the `run.log`:

``` bash
sb -u "http://example.com/api/cars" -j "foo/bar"
```

if the result from the server itself is an array at the root, use empty string as the path:

``` bash
sb -u "http://example.com/api/cars" -j ""
```

Sometimes you would like to capture part of the response body. You can use option `-R` to provide a regex to capture the substring of interest. You would use a regex with one group, denoting the value to capture. For example, let's imagine result is an HTML:

``` html
<html>
<body>
Your ID is: 12313554
</body>
</html>
```

you would use the commadn below to capture the ID in the log file:

``` bash
sb -u "http://example.com/api/yourid" -R "Your ID is: (\d+)"
```

# Other options

## Connection options: proxy and TLS
Use `-x` if you need the client to use default proxy. Using option `-g` you can set the TLS version. For example, to use TLS 1.2:

``` bash
sb -u "https://example.com/api/things" -g 2
```
## Warmup
You can use `-W` option to provide number of seconds for warmup where the results are not included in the test.

## Plugin development
In order to build a a plugin to have full control over parameterisation, you can install Superbenchmarker nuget package:

```
Package-Install superbenchmarker
```

This adds a reference to sb and then you implement a public class implementing IValueProvider:

``` csharp
public class MyPlugin: IValueProvider
{
    private IList<string> _ids; // e.g. populated from a database
    
    public IDictionary<string, object> GetValues(int index)
    {
        // return a dictionary with name of the parameters and their corresponding values.
        // index is the 0-based count of the requests sent. For example, if index is 9, it is the 10th request
        ...
        return new Dictionary { {"ID", _ids(index)} };
    }
}
```

Then run this commmand (**NOTE** name of the parameter returned in the dictionary is the same as the one defined below):
``` bash
sb -u "https://example.com/api/car/{{{ID}}}" -p myplugin.dll
```

# Summary

```
  -c, --concurrency            (Default: 1) Number of concurrent requests

  -n, --numberOfRequests       (Default: 100) Total number of requests

  -N, --numberOfSeconds        Number of seconds to run the test. If specified, -n will be ignored.

  -y, --delayInMillisecond     (Default: 0) Delay in millisecond

  -u, --url                    Required. Target URL to call. Can include placeholders.

  -m, --method                 (Default: GET) HTTP Method to use

  -t, --template               Path to request template to use

  -p, --plugin                 Name of the plugin (DLL) to replace placeholders. Should contain one class which
                               implements IValueProvider. Must reside in the same folder.

  -l, --logfile                Path to the log file storing run stats

  -f, --file                   Path to CSV file providing replacement values for the test

  -a, --TSV                    If you provide a tab-separated-file (TSV) with -f option instead of CSV

  -d, --dryRun                 Runs a single dry run request to make sure all is good

  -e, --timedField             Designates a datetime field in data. If set, requests will be sent according to order
                               and timing of records.

  -g, --TlsVersion             Version of TLS used. Accepted values are 0, 1, 2 and 3 for TLS 1.0, TLS 1.1 and TLS 1.2
                               and SSL3, respectively

  -v, --verbose                Provides verbose tracing information

  -b, --tokeniseBody           Tokenise the body

  -k, --cookies                Outputs cookies

  -x, --useProxy               Whether to use default browser proxy. Useful for seeing request/response in Fiddler.

  -q, --onlyRequest            In a dry-run (debug) mode shows only the request.

  -h, --headers                Displays headers for request and response.

  -z, --saveResponses          saves responses in -w parameter or if not provided in\response_<timestamp>

  -w, --responsesFolder        folder to save responses in if and only if -w parameter is set

  -?, --help                   Displays this help.

  -C, --dontcap                Don't Cap to 50 characters when Logging parameters

  -R, --responseregex          Regex to extract from response. If it has groups, it retrieves the last group.

  -j, --jsonCount              Captures number of elements under the path e.g. root/leaf1/leaf2 finds count of leaf2
                               children - stores in the log as another parameter

  -W, --warmUpPeriod           (Default: 0) Number of seconds to gradually increase number of concurrent users. Warm-up
                               calls do not affect stats.

  -P, --reportSliceSeconds     (Default: 3) Number of seconds as interval for reporting slices. E.g. if chosen as 5,
                               report charts have 5 second intervals.

  -F, --reportFolder           Name of the folder where report files get stored. By default it is in
                               yyyy-MM-dd_HH-mm-ss.ffffff of the start time.

  -B, --dontBrowseToReports    By default it, sb opens the browser with the report of the running test. If specified,
                               it wil not browse.

  -U, --shuffleData            If specified, shuffles the dataset provided by -f option.

  --help                       Display this help screen.

```
