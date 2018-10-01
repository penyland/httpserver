using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public abstract class HttpHandlerBase : IHttpHandler
    {
        public virtual String Name { get { return "HttpHandlerBase"; } }

        // handlers for incoming request
        protected Dictionary<string, IHttpHandler> Handlers = new Dictionary<string, IHttpHandler>();

        public bool IsApplication = false;

        protected List<HttpMethod> SupportedMethods = new List<HttpMethod>();

        public virtual bool CanProcessRequest(HttpContext httpContext)
        {
            if (SupportedMethods.Contains(httpContext.Request.HttpMethod))
            {
                return true;
            }
            return false;
        }

        public async virtual Task<bool> ProcessRequest(HttpContext httpContext)
        {
            IHttpHandler handler = ResolveHandler(httpContext);
            // no handler found
            if (handler == null)
            {
                httpContext.Response.StatusCode = HttpStatusCode.NotFound;
                return false;
            }
            else
            {
                // process request by handler
                if (handler.CanProcessRequest(httpContext))
                {
                    return await handler.ProcessRequest(httpContext);
                }
                else
                {
                    httpContext.Response.StatusCode = HttpStatusCode.MethodNotAllowed;
                    httpContext.Response.Body = null;
                }
            }
            return false;
        }

        /// <summary>
        /// Resolve and return the HTTP handler for the HTTP context of request/response
        /// </summary>
        /// <param name="httpContext">HTTP context of request/response</param>
        /// <returns>HTTP handler for processing the request</returns>
        public virtual IHttpHandler ResolveHandler(HttpContext httpContext)
        {
            IHttpHandler handler = null;

            // check URL for handler
            if (this.Handlers.ContainsKey(httpContext.Request.URL))
            {
                return (IHttpHandler)this.Handlers[httpContext.Request.URL];
            }

            // if no handler found then check application handlers
            // meaning the first part of the url defines which application should handle the request
            // for example if an request comes to: app1/device/command/parameter
            // then the application handler should match to app1 and the rest should be handled by the application handler
            if (httpContext.Request.URL.IndexOf('/') > 0)
            {
                string appUrl = httpContext.Request.URL.Substring(0, httpContext.Request.URL.IndexOf("/"));
                if (this.Handlers.ContainsKey(appUrl))
                {
                    handler = (IHttpHandler)this.Handlers[appUrl];
                }
            }
            return handler;
        }

        /// <summary>
        /// Resolve and return the HTTP handler for the HTTP context of request/response
        /// </summary>
        /// <param name="httpContext">HTTP context of request/response</param>
        /// <returns>HTTP handler for processing the request</returns>
        public virtual IHttpHandler ResolveHandler(string url, HttpContext httpContext)
        {
            IHttpHandler handler = null;

            // check URL for handler
            if (this.Handlers.ContainsKey(url))
            {
                return (IHttpHandler)this.Handlers[url];
            }

            return handler;
        }

        /// <summary>
        /// Register an HTTP handler for incoming request
        /// </summary>
        /// <param name="url">URL for mapping the handler execution</param>
        /// <param name="handler">HTTP handler for the incoming request on specified URL</param>
        public virtual void RegisterHandler(string url, IHttpHandler handler)
        {
            if (handler != null && (url != null) && (url != String.Empty))
                this.Handlers.Add(url.Trim('/').ToLower(), handler);
            else
                throw new ArgumentNullException("url parameter cannot be null or empty");
        }
    }
}
