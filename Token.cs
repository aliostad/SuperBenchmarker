using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    class Token
    {
        public string Name { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }

        public void Replace(string tokenisedString, string value)
        {
            var builder = new StringBuilder();
            builder.Append(tokenisedString.Substring(Start, Length));
            builder.Append(value);
            builder.Append(tokenisedString.Substring(Start+Length));
        }

        public void Replace(string tokenisedString, IDictionary<string, object> tokens)
        {
           Replace(tokenisedString, tokens[Name].ToString());
        }
    }
}
