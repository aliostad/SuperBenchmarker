using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public class TemplateParser
    {
        private readonly Encoding _encoding;
        private byte[] _payload;
        private List<KeyValuePair<string, TokenisedString>> _headers = new List<KeyValuePair<string, TokenisedString>>();

        public TemplateParser(string templateText)
            : this(templateText, Encoding.UTF8)
        {
            
        }

        public TemplateParser(string templateText, Encoding encoding)
        {
            _encoding = encoding;
            var indexOfHeadersEnd = templateText.IndexOf("\r\n\r\n");
            if(indexOfHeadersEnd<0)
                Init(templateText, null);
            else
                Init(templateText.Substring(0, indexOfHeadersEnd), templateText.Substring(indexOfHeadersEnd + 4));
        }

        public TemplateParser(string headers, byte[] payload)
        {
            _payload = payload;
            Init(headers, null);
        }

        private void Init(string headers, string payload)
        {
            _headers = new List<KeyValuePair<string, TokenisedString>>();
            var reader = new StringReader(headers);
            string line = null;
            while((line = reader.ReadLine()) != null)
            {
                if (line.Length > 0)
                {
                    var indexOfColon = line.IndexOf(':');
                    _headers.Add(new KeyValuePair<string, TokenisedString>(
                        line.Substring(0, indexOfColon),
                        new TokenisedString(line.Substring(indexOfColon+1).Trim())));
                }
            }
        }

        public IEnumerable<KeyValuePair<string, TokenisedString>> Headers 
        { 
            get { return _headers; } 
        }

      
    }
}
