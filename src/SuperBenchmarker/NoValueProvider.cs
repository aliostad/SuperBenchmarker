using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    class NoValueProvider : IValueProvider
    {
        public IDictionary<string, object> GetValues(int i)
        {
            return new Dictionary<string, object>();
        }
    }
}
