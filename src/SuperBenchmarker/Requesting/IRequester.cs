using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public interface IRequester
    {
        HttpStatusCode Next(int i, out IDictionary<string, object> parameters);
    }
}
