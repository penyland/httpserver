using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    /// <summary>
    /// HTTP context for the current request/response
    /// </summary>
    public class HttpContext
    {
        /// <summary>
        /// HTTP request in the current context
        /// </summary>
        public HttpRequest Request { get; internal set; }

        /// <summary>
        /// HTTP response in the current context
        /// </summary>
        public HttpResponse Response { get; internal set; }

        /// <summary>
        /// HTTP server utility
        /// </summary>
        // public HttpServerUtility Server { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        internal HttpContext(HttpRequest request) // , HttpServerUtility server)
        {
            this.Request = request;
            this.Response = new HttpResponse();
            // this.Server = server;
        }
    }
}
