using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public class Requester
    {
        private CommandLineOptions _options;
        private TokenisedString _url;
        private HttpClient _client = new HttpClient();
        private TemplateParser _templateParser;

        public Requester(CommandLineOptions options)
        {
            _options = options;
            _url = new TokenisedString(options.Url);
            if (!string.IsNullOrEmpty(options.Template))
            {
                _templateParser = new TemplateParser(File.ReadAllText(options.Template));
            }

        }

        public void Next(int i)
        {
            NextAsync(i).Wait();
        }

        public async Task NextAsync(int i)
        {
            var request = BuildMessage(i);
            try
            {
                var response = await _client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                Console.Write(".");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        internal HttpRequestMessage BuildMessage(int i)
        {
            var dictionary = GetParams(i);
            var request = new HttpRequestMessage(new HttpMethod(_options.Method), _url.ToString(dictionary));
            if (_templateParser != null)
            {
                foreach (var h in _templateParser.Headers)
                {
                    request.Headers.Add(h.Key, h.Value.ToString(dictionary));
                }
                
            }
            return request;
        }

        private IDictionary<string, object> GetParams(int i)
        {
            return new Dictionary<string, object>();
        }

    }
}
