using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public static class JsonCounter
    {
        public static int? Count(string text, string path)
        {
            var segments = path.Split('/');
            var j = JObject.Parse(text);
            return FindRecursively(j, segments, 0);
        }

        private static int? FindRecursively(JToken j, string[] segments, int index)
        {

            if (segments.Length == index)
            {
                return j.Children().Where(x => x.Type == JTokenType.Object).Count();
            }
            else
            {
                if (j.Type != JTokenType.Object)
                    return null;

                var nextJ = j[segments[index]];
                if (nextJ == null)
                    return null;
                return nextJ.Type == JTokenType.Object || nextJ.Type == JTokenType.Array
                    ? FindRecursively(nextJ, segments, index + 1)
                    : null;
            }
        }
    }
}
