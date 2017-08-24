using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public class Requester : AsyncRequesterBase, IRequester
    {
        private CommandLineOptions _options;
        private TokenisedString _url;
        private HttpClient _client;
        private TemplateParser _templateParser;
        private IValueProvider _valueProvider;
        private RandomValueProvider _randomValueProvider;

        public Requester(CommandLineOptions options) : base(options)
        {
        }

        public HttpStatusCode Next(int i, out IDictionary<string, object> parameters)
        {
            var result = NextAsync(i).Result;
            parameters = result.Item1;
            return result.Item2;
        }

    }
}
