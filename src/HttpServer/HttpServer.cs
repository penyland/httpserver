// Copyright (c) Peter Nylander.  All rights reserved.

using HttpServer.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        // request queue
        private readonly ProducerConsumerQueue requestQueue;

        // Web Server running status
        private bool isRunning;

        private CancellationTokenSource cancellationTokenSource;

        // handlers for incoming request
        private readonly Dictionary<string, IHttpHandler> handlers = new Dictionary<string, IHttpHandler>();

        // handler for access to Web Server file system
        private readonly FileSystemHandler fileSystemHandler;

        #endregion

        public HttpServer(int port, ISocketListener socketListener)
        {
            this.Port = port;
            this.socketListener = socketListener;
            this.socketListener.ConnectionReceived += this.ConnectionReceivedAsync;

            this.requestQueue = new ProducerConsumerQueue(WORKER_COUNT);
            this.fileSystemHandler = new FileSystemHandler();
        }

        public int Port { get; }

        public void Dispose()
        {
        }

        public void Start()
        {
            try
            {
                this.cancellationTokenSource = new CancellationTokenSource();

                var listenerTask = Task.Run(this.ListenToConnectionsAsync, this.cancellationTokenSource.Token);

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
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource = null;

                this.requestQueue.Dispose();
                this.socketListener.Stop();

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

        private async Task ListenToConnectionsAsync()
        {
            this.socketListener.Start();

            while (this.isRunning && !this.cancellationTokenSource.IsCancellationRequested)
            {
                await this.socketListener.BindServiceNameAsync(this.Port.ToString());
            }
        }

        private async void ConnectionReceivedAsync(object sender, TcpClient args)
        {
            await this.requestQueue.Enqueue(
                () => this.ProcessRequestAsync(args),
                default(System.Threading.CancellationToken));
        }

        private async void ProcessRequestAsync(TcpClient requestClient)
        {
            HttpRequest httpRequest = null;
            bool badRequest = false;

            try
            {
                // parse incoming request
                httpRequest = await HttpRequest.ParseAsync(requestClient);
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
            var responseHeaderBuilder = new StringBuilder("HTTP/1.1 ");
            responseHeaderBuilder.Append(((int)httpContext.Response.StatusCode).ToString());
            responseHeaderBuilder.Append(" ");
            responseHeaderBuilder.AppendLine(Utils.MapStatusCodeToReason(httpContext.Response.StatusCode));

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
                responseHeaderBuilder.Append(responseHeaderKey);
                responseHeaderBuilder.Append(": ");
                responseHeaderBuilder.AppendLine(httpContext.Response.Headers[responseHeaderKey]);
            }

            // line blank seperation header-body
            responseHeaderBuilder.AppendLine();

            // start sending status line and headers
            byte[] buffer = Encoding.UTF8.GetBytes(responseHeaderBuilder.ToString());

            try
            {
                using (var streamWriter = new StreamWriter(requestClient.GetStream()))
                {
                    await streamWriter.WriteAsync(responseHeaderBuilder.ToString());
                    await streamWriter.FlushAsync();

                    //// send body, if it exists
                    if (bodyLength > 0)
                    {
                        await streamWriter.WriteAsync(httpContext.Response.Body);
                        await streamWriter.FlushAsync();
                    }
                    else // no body, streamed response
                    {
                        if (httpContext.Response.Stream != null)
                        {
                            byte[] sendBuffer = new byte[512];
                            int sendBytes = 0;
                            while ((sendBytes = httpContext.Response.Stream.Read(sendBuffer, 0, sendBuffer.Length)) > 0)
                            {
                                byte[] outBuffer = new byte[sendBytes];

                                using (var binaryWriter = new BinaryWriter(requestClient.GetStream()))
                                {
                                    binaryWriter.Write(sendBuffer);
                                    binaryWriter.Flush();
                                }
                            }

                            httpContext.Response.CloseStream();
                        }
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

        private void LogMessage(string message)
        {
            this.OnLogMsg?.Invoke(message);
        }
    }
}
