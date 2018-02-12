using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker
{
    public interface IAsyncRequester
    {
        Task<Tuple<IDictionary<string, object>, HttpStatusCode>> NextAsync(int i);
    }
}
