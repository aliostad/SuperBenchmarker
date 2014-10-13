using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RandomGen;

namespace SuperBenchmarker
{
    public class RandomValueProvider : IValueProvider
    {
        private const string Prefix = "RAND_";
        internal const string NamePattern = @"^[A-Za-z_0-9]+\:RAND_(STRING|DATE|DATETIME|DATETIMEOFFSET|INTEGER|DOUBLE|NAME)(?:\:\[([^:]+)\:([^]]+)\])?$";
        private class RandomTypes
        {
            public const string RandomString = "STRING";
            public const string RandomDate = "DATE";
            public const string RandomDateTime = "DATETIME";
            public const string RandomDateTimeOffset = "DATETIMEOFFSET";
            public const string RandomInteger = "INTEGER";
            public const string RandomDouble = "DOUBLE";
            public const string RandomName = "NAME";

           
        }

        private Dictionary<string, Func<object>> _gens = new Dictionary<string, Func<object>>();

        public RandomValueProvider(params TokenisedString[] tokenisedStrings)
        {
            foreach (var tokenisedString in tokenisedStrings)
            {
                foreach (var t in tokenisedString.Tokens)
                {
                    var func = AnalyseThis(t.Name);
                    if (func != null)
                        _gens[t.Name] = func;
                }
            }
        }

        internal Func<object> AnalyseThis(string name)
        {
            var match = Regex.Match(name, NamePattern);
            if (!match.Success)
                return null;

            switch (match.Groups[1].Value)
            {
                case RandomTypes.RandomString:
                    return Gen.Random.Text.Words();
                case RandomTypes.RandomName:
                    return Gen.Random.Names.First();
                case RandomTypes.RandomInteger:
                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        var integers = Gen.Random.Numbers.Integers(
                            Convert.ToInt32(match.Groups[2].Value),
                            Convert.ToInt32(match.Groups[3].Value));
                        return () => integers().ToString();
                    }
                    var ints = Gen.Random.Numbers.Integers(max: int.MaxValue);
                    return () => ints().ToString();
                case RandomTypes.RandomDouble:
                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        var doubles = Gen.Random.Numbers.Doubles(
                            Convert.ToDouble(match.Groups[2].Value),
                            Convert.ToDouble(match.Groups[3].Value));
                        return () => doubles().ToString();
                    }
                    var dos = Gen.Random.Numbers.Doubles(0, double.MaxValue);
                    return () => dos().ToString();
                case RandomTypes.RandomDate:
                    var dates = GetRandomDateTime(match.Groups[2].Value, match.Groups[3].Value);
                    return () => dates().ToString("d");
                case RandomTypes.RandomDateTime:
                    var dates2 = GetRandomDateTime(match.Groups[2].Value, match.Groups[3].Value);
                    return () => dates2().ToString("G");
                case RandomTypes.RandomDateTimeOffset:
                    var dates3 = GetRandomDateTime(match.Groups[2].Value, match.Groups[3].Value);
                    return () => dates3().ToString("O");

                default:
                    throw new NotSupportedException(match.Groups[1].Value);
            }
            
        }

        private Func<DateTime> GetRandomDateTime(string min = null, string max = null)
        {
            if (!string.IsNullOrEmpty(min))
            {
                return Gen.Random.Time.Dates(
                    DateTime.Parse(min),
                    DateTime.Parse(max));
            }
            return Gen.Random.Time.Dates(DateTime.Now.AddYears(-25), DateTime.Now);
        }

        public IDictionary<string, object> GetValues(int index)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var gen in _gens)
            {
                dictionary[gen.Key] = gen.Value();
            }
            return dictionary;
        }
    }
}
