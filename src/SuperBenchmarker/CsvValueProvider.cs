using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public class CsvValueProvider: IValueProvider
    {
        private string _fileName;
        private string[] _lines;
        private const string CsvPattern = "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)";
        private string[] _headers;
        private const string RandomIdentifier = "###random###";
        private Type[] _typesForRandom = null;
        private Random _random = new Random();

        public CsvValueProvider(string fileName)
        {
            _fileName = fileName;
            _lines = File.ReadAllLines(fileName);
            var matches = Regex.Matches(_lines[0], CsvPattern);
            _headers = new string[matches.Count];
            int i = 0;
            foreach (Match match in matches)
            {
                _headers[i] = match.Value;
                i++;
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
            var matches = Regex.Matches(_lines[lineNumber], CsvPattern);
            int i = 0;
            var values = new string[matches.Count];

            foreach (Match match in matches)
            {
                values[i] = match.Value;
                i++;
            }

            var result = new Dictionary<string, object>();
            for (int j = 0; j < values.Length; j++)
            {
                result.Add(_headers[j], values[j]);
            }

            return result;
        }
    }
}
