// Copyright (c) Peter Nylander.  All rights reserved.

using System;
using System.IO;
using System.Net;

namespace HttpServer
{
    /// <summary>
    /// HTTP response
    /// </summary>
    public class HttpResponse : HttpBase, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponse"/> class.
        /// </summary>
        public HttpResponse()
            : base()
        {
            // set default status code and content type
            this.StatusCode = HttpStatusCode.OK;
            this.ContentType = "text/html";
        }

        /// <summary>
        /// Gets or sets the status code of the HTTP response
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the HTTP response content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the response stream
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// Close response stream
        /// </summary>
        public void CloseStream()
        {
            this.Stream?.Dispose();
        }

        public void Dispose()
        {
            this.CloseStream();
        }
    }
}
