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

        public string Replace(string tokenisedString, string value)
        {
            var builder = new StringBuilder();
            builder.Append(tokenisedString.Substring(0, Start-3));
            Console.WriteLine(builder.ToString());
            builder.Append(value);
            Console.WriteLine(builder.ToString());
            builder.Append(tokenisedString.Substring(Start+Length+6));
            Console.WriteLine(builder.ToString());

            return builder.ToString();
        }

        public void Replace(string tokenisedString, IDictionary<string, object> tokens)
        {
           Replace(tokenisedString, tokens[Name].ToString());
        }
    }
}
