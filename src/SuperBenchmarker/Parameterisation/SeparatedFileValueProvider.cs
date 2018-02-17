using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public class SeparatedFileValueProvider: IValueProvider
    {
        private string[] _lines;
        private const string CsvPattern = "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)";
        private string[] _headers;
        private Type[] _typesForRandom = null;
        private Random _random = new Random();
        private char _separator;

        public SeparatedFileValueProvider(string fileName, char separator = ',', bool shuffle = false)
        {
            _separator = separator;
            _lines = File.ReadAllLines(fileName);
            _headers = ParseLine(_lines[0]);

            if (shuffle && _lines.Length > 0)
            {
                _lines.Shuffle(1);
            }

            if (_lines.Length == 1)
            {
                _typesForRandom = new Type[_headers.Length];
                for (int j = 0; j < _headers.Length; j++)
                {
                    _typesForRandom[j] = typeof (int);
                }
            }

        }

        private string[] ParseLine(string line)
        {
            // CSV deals with string qualifiers
            if (_separator == ',')
            {
                var matches = Regex.Matches(line, CsvPattern);
                var strings = new string[matches.Count];
                int i = 0;
                foreach (Match match in matches)
                {
                    strings[i] = match.Value;
                    i++;
                }
                return strings;
            }
            else
            {
                return line.Split(new[] {_separator});
            }
        }

        public IDictionary<string, object> GetValues(int index)
        {
            if (_typesForRandom == null)
            {
                return GetFileValues(index);
            }
            else
            {
                return GetRandomValues(index);
            }
        }

        private IDictionary<string, object> GetRandomValues(int index)
        {
            var dictionary = new Dictionary<string, object>();
            object o = "";
            for (int i = 0; i < _headers.Length; i++)
            {
                if (typeof (int) == _typesForRandom[i])
                    o = _random.Next();

                dictionary.Add(_headers[i],o);
            }

            return dictionary;
        }

        public IDictionary<string, object> GetFileValues(int index)
        {
            var lineNumber = (index % (_lines.Length - 1)) + 1;

            var values = ParseLine(_lines[lineNumber]);
            var result = new Dictionary<string, object>();
            for (int j = 0; j < values.Length; j++)
            {
                result.Add(_headers[j], values[j]);
            }

            return result;
        }
    }
}
