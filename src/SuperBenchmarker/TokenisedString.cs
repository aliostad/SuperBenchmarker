using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public class TokenisedString
    {
        private const string TokenPattern = @"\{\{\{([^\}]+?)\}\}\}";
        private readonly string _content;
        private List<Token> _tokens; 
        

        public TokenisedString(string content)
        {
            _content = content;
            _tokens = FindTokens(content);
        }

        public string ToString(IDictionary<string, object> tokens)
        {
            var s = _content;
            _tokens.ForEach((t) => s = t.Replace(s, tokens[t.Name].ToString()));
            return s;
        }

        internal static List<Token> FindTokens(string s)
        {
            var matches = Regex.Matches(s, TokenPattern);
            var tokens = new List<Token>();
            foreach (Match match in matches)
            {
                tokens.Add(new Token()
                    {
                        Name = match.Groups[1].Value,
                        Start = match.Groups[1].Index,
                        Length = match.Groups[1].Length
                    });
            }

            return tokens;
        }
    }
}
