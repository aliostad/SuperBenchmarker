using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public interface IValueProvider
    {
        IDictionary<string, object> GetValues(int index);
    }
}
