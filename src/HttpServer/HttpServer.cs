// Copyright (c) Peter Nylander.  All rights reserved.

using HttpServer.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace HttpServer
{
    public class HttpServer : IDisposable
    {
        public delegate void LogPrintDelegate(string msg);

        public event LogPrintDelegate OnLogMsg;

        // socket instance for listening incoming request
        private readonly ISocketListener socketListener;

        #region Constants ...

        // default HTTP port
        private const int HTTP_PORT_DEFAULT = 80;

        // Default maximum concurrency
        private const int WORKER_COUNT = 2;

        // default webroot directory
        private const string WEBROOT_DEFAULT = @"\webroot\";

        // size of response buffer
        private const int BUFFER_RESPONSE_SIZE = 8192;

        // default start page
        public const string DEFAULT_PAGE = "index.html";

        #endregion

        #region Fields

        // Web Server running status
        private bool isRunning;

        // request queue
        private readonly ProducerConsumerQueue requestQueue;

        // handlers for incoming request
        private readonly Dictionary<string, IHttpHandler> handlers = new Dictionary<string, IHttpHandler>();

        // handler for access to Web Server file system
        private readonly FileSystemHandler fileSystemHandler;

        #endregion

        public HttpServer(int port, ISocketListener socketListener)
        {
            this.Port = port;
            this.socketListener = socketListener;
            this.socketListener.ConnectionReceived += listener_ConnectionReceived;

            this.requestQueue = new ProducerConsumerQueue(WORKER_COUNT);

            //this.listener = new StreamSocketListener();
            //this.listener.ConnectionReceived += listener_ConnectionReceived;
            this.fileSystemHandler = new FileSystemHandler();
        }

        public int Port { get; }

        public void Dispose()
        {
            this.socketListener.Dispose();
        }

        public async void StartAsync()
        {
            try
            {
                await this.socketListener.BindServiceNameAsync(this.Port.ToString());

                this.isRunning = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Stop Web Server
        /// </summary>
        public void Stop()
        {
            try
            {
                this.requestQueue.Dispose();
                this.socketListener.Dispose();

                this.isRunning = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Register an HTTP handler for incoming request
        /// </summary>
        /// <param name="url">URL for mapping the handler execution</param>
        /// <param name="handler">HTTP handler for the incoming request on specified URL</param>
        public void RegisterHandler(string url, IHttpHandler handler)
        {
            if (handler != null && (url != null) && !string.IsNullOrEmpty(url))
            {
                this.handlers.Add(url.Trim('/').ToLower(), handler);
            }
            else
            {
                throw new ArgumentNullException("url parameter cannot be null or empty");
            }
        }

        private async void listener_ConnectionReceived(object sender, ISocket args)
        {
            await this.requestQueue.Enqueue(
                () => this.ProcessRequestAsync(args),
                default(System.Threading.CancellationToken));
        }

        private async void ProcessRequestAsync(ISocket requestSocket)
        {
            HttpRequest httpRequest = null;
            bool badRequest = false;

            try
            {
                // parse incoming request
                httpRequest = await HttpRequest.ParseAsync(requestSocket);
            }
            catch
            {
                // error parsing incoming request (bad request)
                httpRequest = new HttpRequest();
                badRequest = true;
            }

            // create an HTTP context for the current request
            var httpContext = new HttpContext(httpRequest);

            if (badRequest)
            {
                this.LogMessage("Bad request to " + httpContext.Request.URL);
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                httpContext.Response.Body = null;
            }
            else
            {
                this.LogMessage("Request to " + httpContext.Request.URL);
                try
                {
                    // Resolve Request handler for the request
                    IHttpHandler handler = this.ResolveHandler(httpContext);

                    // no handler found
                    if (handler == null)
                    {
                        httpContext.Response.StatusCode = HttpStatusCode.NotFound;
                    }
                    else
                    {
                        // process request by handler
                        if (handler.CanProcessRequest(httpContext))
                        {
                            await handler.ProcessRequest(httpContext);
                        }
                        else
                        {
                            httpContext.Response.StatusCode = HttpStatusCode.MethodNotAllowed;
                            httpContext.Response.Body = null;
                        }
                    }
                }
                catch
                {
                    httpContext.Response.StatusCode = HttpStatusCode.InternalServerError;
                    httpContext.Response.Body = null;
                }
            }

            // Build HttpResponse

            // build the status line
            var responseBuilder = new StringBuilder("HTTP/1.1 ");
            responseBuilder.Append(((int)httpContext.Response.StatusCode).ToString());
            responseBuilder.Append(" ");
            responseBuilder.AppendLine(this.MapStatusCodeToReason(httpContext.Response.StatusCode));

            // build header section
            httpContext.Response.Headers["Content-Type"] = httpContext.Response.ContentType;
            int bodyLength = (!string.IsNullOrEmpty(httpContext.Response.Body)) ? httpContext.Response.Body.Length : 0;
            if (bodyLength > 0)
            {
                httpContext.Response.Headers["Content-Length"] = bodyLength.ToString();
            }

            httpContext.Response.Headers["Connection"] = "close";

            foreach (string responseHeaderKey in httpContext.Response.Headers.Keys)
            {
                responseBuilder.Append(responseHeaderKey);
                responseBuilder.Append(": ");
                responseBuilder.AppendLine(httpContext.Response.Headers[responseHeaderKey]);
            }

            // line blank seperation header-body
            responseBuilder.AppendLine();

            // start sending status line and headers
            byte[] buffer = Encoding.UTF8.GetBytes(responseBuilder.ToString());

            try
            {
                DataWriter dataWriter = new DataWriter(requestSocket.OutputStream);
                dataWriter.WriteBytes(buffer);
                await dataWriter.StoreAsync();

                //// send body, if it exists
                if (bodyLength > 0)
                {
                    buffer = Encoding.UTF8.GetBytes(httpContext.Response.Body);
                    dataWriter.WriteBytes(buffer);
                    await dataWriter.StoreAsync();
                }

                // no body, streamed response
                else
                {
                    if (httpContext.Response.Stream != null)
                    {
                        byte[] sendBuffer = new byte[512];
                        int sendBytes = 0;
                        while ((sendBytes = httpContext.Response.Stream.Read(sendBuffer, 0, sendBuffer.Length)) > 0)
                        {
                            byte[] outBuffer = new byte[sendBytes];
                            dataWriter.WriteBytes(outBuffer);
                            await dataWriter.StoreAsync();
                        }

                        httpContext.Response.CloseStream();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + " " + e.InnerException);
            }
        }

        /// <summary>
        /// Resolve and return the HTTP handler for the HTTP context of request/response
        /// </summary>
        /// <param name="httpContext">HTTP context of request/response</param>
        /// <returns>HTTP handler for processing the request</returns>
        private IHttpHandler ResolveHandler(HttpContext httpContext)
        {
            HttpHandlerBase handler = null;

            // check if it is a request for the file system
            if (this.fileSystemHandler.CanProcessRequest(httpContext))
            {
                return this.fileSystemHandler;
            }
            else
            {
                // check URL for handler
                if (this.handlers.ContainsKey(httpContext.Request.URL))
                {
                    return (IHttpHandler)this.handlers[httpContext.Request.URL];
                }
            }

            return handler;
        }

        /// <summary>
        /// Map an HTTP status code to reason string
        /// </summary>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>Reason mapped</returns>
        private string MapStatusCodeToReason(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    return "OK";
                case HttpStatusCode.Found:
                    return "Found";
                case HttpStatusCode.BadRequest:
                    return "Bad request";
                case HttpStatusCode.Unauthorized:
                    return "Unauthorized";
                case HttpStatusCode.Forbidden:
                    return "Forbidden";
                case HttpStatusCode.NotFound:
                    return "Not found";
                case HttpStatusCode.MethodNotAllowed:
                    return "Method not allowed";
                case HttpStatusCode.InternalServerError:
                    return "Internal server error";
                case HttpStatusCode.ServiceUnavailable:
                    return "Service unavailable";
            }

            return null;
        }

        private void LogMessage(string message)
        {
            this.OnLogMsg?.Invoke(message);
        }
    }
}
