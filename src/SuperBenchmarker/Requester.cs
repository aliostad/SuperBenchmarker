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
        private RandomValueProvider _randomValueProvider;
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
                _randomValueProvider = new RandomValueProvider(
                    new[] { _url }.Concat(_templateParser.Headers.Select(x => x.Value)).ToArray());
            }
            else
            {
                _randomValueProvider = new RandomValueProvider(_url);
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

        public HttpStatusCode Next(int i, out IDictionary<string, object> parameters)
        {
            var result = NextAsync(i).Result;
            parameters = result.Item1;
            return result.Item2;
        }

        public async Task<Tuple<IDictionary<string, object>, HttpStatusCode>> NextAsync(int i)
        {
            HttpStatusCode statusCode = HttpStatusCode.SeeOther;
            string textContent = string.Empty;
            IDictionary<string, object> parameters;
            var request = BuildRequest(i, out parameters);
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
                statusCode = response.StatusCode;
                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    if (_options.SaveResponses)
                    {
                        // fire and forget not to affect time taken or TPS
                        Task.Run(() =>
                            File.WriteAllBytes(Path.Combine(_options.ResponseFolder, (i + 1).ToString() +
                                                                                     GetExtensionFromContentType(
                                                                                         response.Content.Headers
                                                                                             .ContentType.MediaType)),
                                content)
                            );

                    }
                    try
                    {
                        textContent = Encoding.UTF8.GetString(content);
                    }
                    catch
                    {
                        // ignore !!
                    }    
                }

                if (_options.OutputHeaders)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(response.Headers.ToString());
                    Console.ResetColor();
                }

                if (_options.IsDryRun && !_options.OnlyRequest)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(textContent);
                    Console.ResetColor();
                }

                
            }
            catch (Exception e)
            {

                if (_options.Verbose || _options.IsDryRun)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.ToString());
                    Console.ResetColor();                
                }

            }

            return new Tuple<IDictionary<string, object>, HttpStatusCode>(parameters, statusCode);

        }

        private static string GetExtensionFromContentType(string contentType)
        {
            switch (contentType)
            {
                case "application/xml":
                    return ".xml";
                case "application/javascript":
                case "text/javascript":
                    return ".json";
                case "text/plain":
                    return ".txt";
                default:
                    return ".bin";
            }
        }

        internal HttpRequestMessage BuildRequest(int i, out IDictionary<string, object> parameters)
        {
            var dictionary = GetParams(i);
            parameters = dictionary;
            var request = new HttpRequestMessage(new HttpMethod(_options.Method), _url.ToString(dictionary));
            if (_templateParser != null)
            {
                foreach (var h in _templateParser.Headers)
                {
                    request.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString(dictionary));
                }
                
                if (new[] {"post", "put", "delete"}.Any(x => x == _options.Method.ToLower()) &&
                    _templateParser.Payload!=null &&
                    _templateParser.Payload.Length>0)
                {
                    if (_templateParser.TextBody == null)
                        request.Content = new ByteArrayContent(_templateParser.Payload);
                    else
                        request.Content =
                            new ByteArrayContent(Encoding.UTF8.GetBytes(_templateParser.TextBody.ToString(dictionary)));

                    foreach (var h in _templateParser.Headers)
                    {
                        request.Content.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString(dictionary));
                    }
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
            var dictionary = _valueProvider.GetValues(i);
            foreach (var kv in _randomValueProvider.GetValues(i))
            {
                dictionary[kv.Key] = kv.Value; // overwrite
            }
            return dictionary;
        }
    }
}
