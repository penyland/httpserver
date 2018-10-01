using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    /// <summary>
    /// HTTP interface for handler request
    /// </summary>
    public interface IHttpHandler
    {
        string Name { get; }

        /// <summary>
        /// Returns true if handler can process the request
        /// </summary>
        /// <param name="httpContext">HTTP context of request/response</param>
        /// <returns>Handler can process or not the request</returns>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        bool CanProcessRequest(HttpContext httpContext);

        /// <summary>
        /// Process an HTTP request
        /// </summary>
        /// <param name="httpContext">HTTP context of request/response</param>
        /// <returns>Handler processed request or not</returns>
        Task<bool> ProcessRequest(HttpContext httpContext);
    }
}
