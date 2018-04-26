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
            path = path.Trim();
            var segments = path==string.Empty ? new string[0] : path.Split('/');
            var j = JToken.Parse(text);
            return FindRecursively(j, segments, 0);
        }

        private static int? FindRecursively(JToken j, string[] segments, int index)
        {

            if (segments.Length == index)
            {
                return j.Children().Where(x => x.Type.IsIn(JTokenType.Object, JTokenType.Boolean, JTokenType.Bytes,
                        JTokenType.Date, JTokenType.Float, JTokenType.Guid, JTokenType.Integer, JTokenType.String, JTokenType.TimeSpan)).Count();
            }
            else
            {
                if (j.Type != JTokenType.Object)
                    return null;

                var nextJ = j[segments[index]];
                if (nextJ == null)
                    return null;
                return nextJ.Type.IsIn(JTokenType.Object, JTokenType.Array)
                    ? FindRecursively(nextJ, segments, index + 1)
                    : null;
            }
        }

        private static bool IsIn<T>(this T me, params T[] e)
        {
            return e.Any(x => x.Equals(me));
        }
    }
}
