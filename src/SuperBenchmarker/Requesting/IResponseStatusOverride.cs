using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SuperBenchmarker
{
    /// <summary>
    /// Interface for plugins that 
    /// </summary>
    public interface IResponseStatusOverride
    {
        /// <summary>
        /// Override the status received
        /// </summary>
        /// <param name="statusReceived">Response status</param>
        /// <param name="responseBuffer">response data as a byte array</param>
        /// <param name="responseHeaders">Response headers</param>
        /// <param name="request">Request</param>
        /// <returns></returns>
        HttpStatusCode OverrideStatus(HttpStatusCode statusReceived, byte[] responseBuffer, 
            HttpResponseHeaders responseHeaders,
            HttpRequestMessage request);
    }
}