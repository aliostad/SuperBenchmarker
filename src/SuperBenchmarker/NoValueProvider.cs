using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    class NoValueProvider : IValueProvider
    {
        private Dictionary<string, object> _noValues = new Dictionary<string, object>();
        public IDictionary<string, object> GetValues(int i)
        {
            return _noValues;
        }
    }
}
