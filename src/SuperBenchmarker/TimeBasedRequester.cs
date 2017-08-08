using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public class TimeBasedRequester : AsyncRequesterBase
    {
        private DateTime _startTime = DateTime.MinValue;
        private TimeSpan _diff;

        public TimeBasedRequester(CommandLineOptions options) : base(options)
        {
        }

        internal override HttpRequestMessage BuildRequest(int i, out IDictionary<string, object> parameters)
        {

            var req = base.BuildRequest(i, out parameters);

            var timeField = _options.TimeField;
            var time = GetTime(timeField, parameters);

            if (DateTime.MinValue == _startTime)
            {
                _startTime = DateTime.Now;
                _diff = DateTime.Now.Subtract(time);
            }

            var diff = time.Add(_diff).Subtract(DateTime.Now);
            if (diff.TotalMilliseconds > 30) // thread.sleep precision is roughly 15-30 milli
                Thread.Sleep(diff);

            return req;
        }

        private DateTime GetTime(string timeField, IDictionary<string, object> parameter)
        {
            var kv = parameter.FirstOrDefault(x => x.Key == timeField);
            if(kv.Value == null) // it is a struct - sadly. So cannot say kv == null
            {
                throw new InvalidOperationException("Could not find time field: " + timeField);
            }

            var t = kv.Value.GetType();
            if (t == typeof(string))
                return DateTime.Parse((string)kv.Value);
            else if (t == typeof(DateTime))
                return (DateTime)kv.Value;
            else if (t == typeof(DateTimeOffset))
                return ((DateTimeOffset)kv.Value).DateTime;

            throw new InvalidOperationException("This type is not supported for conversion to DateTime: " + t.Name);
        }
    }
}
