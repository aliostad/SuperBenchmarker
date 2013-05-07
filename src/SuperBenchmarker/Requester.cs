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
    public class Requester
    {
        private CommandLineOptions _options;
        private TokenisedString _url;
        private HttpClient _client;
        private TemplateParser _templateParser;
        private IValueProvider _valueProvider;

        public Requester(CommandLineOptions options)
        {

            var requestHandler = new WebRequestHandler()
                                     {
                                         UseCookies = false,
                                         UseDefaultCredentials = true
                                     };
            if (options.UseProxy)
            {
                requestHandler.UseProxy = true;
                requestHandler.Proxy = WebRequest.GetSystemWebProxy();
            }
            _client = new HttpClient(requestHandler);
            _options = options;
            _url = new TokenisedString(options.Url);
            if (!string.IsNullOrEmpty(options.Template))
            {
                _templateParser = new TemplateParser(File.ReadAllText(options.Template));
            }
            
            _valueProvider = new NoValueProvider();

            if (!string.IsNullOrEmpty(_options.Plugin)) // plugin
            {
                var assembly = Assembly.LoadFile(_options.Plugin);
                var valueProviderType = assembly.GetExportedTypes().Where(t => typeof (IValueProvider)
                                                                      .IsAssignableFrom(t)).FirstOrDefault();
                if(valueProviderType==null)
                    throw new ArgumentException("No public type in plugin implements IValueProvider.");

                _valueProvider = (IValueProvider)Activator.CreateInstance(valueProviderType);
            }
            else if (!string.IsNullOrEmpty(options.ValuesFile)) // csv values file
            {
                _valueProvider = new CsvValueProvider(options.ValuesFile);
            }

        }

        public void Next(int i)
        {
            NextAsync(i).Wait();
        }

        public async Task NextAsync(int i)
        {
            var request = BuildRequest(i);
            if (_options.Verbose)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write(request.Method.Method + " ");
                Console.WriteLine(request.RequestUri.ToString());
                Console.ResetColor();
            }
            
            if (_options.OutputHeaders)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(request.Headers.ToString());
                Console.ResetColor();                
            }

            try
            {
                var response = await _client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (_options.OutputHeaders)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(response.Headers.ToString());
                    Console.ResetColor();
                }

                if (_options.IsDryRun && !_options.OnlyRequest)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(content);
                    Console.ResetColor();
                }

                
            }
            catch (Exception e)
            {
                if(_options.Verbose)
                    Console.WriteLine(e.ToString());
            }



        }

        internal HttpRequestMessage BuildRequest(int i)
        {
            var dictionary = GetParams(i);
            var request = new HttpRequestMessage(new HttpMethod(_options.Method), _url.ToString(dictionary));
            if (_templateParser != null)
            {
                foreach (var h in _templateParser.Headers)
                {
                    request.Headers.Add(h.Key, h.Value.ToString(dictionary));
                }
                
                if (new[] {"post", "put", "delete"}.Any(x => x == _options.Method.ToLower()) &&
                    _templateParser.Payload!=null &&
                    _templateParser.Payload.Length>0)
                {
                    request.Content = new ByteArrayContent(_templateParser.Payload);
                }
            }

            if (_options.OutputCookies)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                foreach (var cookie in request.Headers.GetCookies())
                {
                    foreach (var state in cookie.Cookies)
                    {
                        Console.Write(state.Name);
                        Console.Write(": ");
                        Console.WriteLine(state.Value);
                    }
                }
                Console.ResetColor();
            }

            return request;
        }

        private IDictionary<string, object> GetParams(int i)
        {
            return _valueProvider.GetValues(i);
        }

    }
}
