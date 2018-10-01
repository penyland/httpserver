using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace HttpServer
{
    /// <summary>
    /// HTTP response
    /// </summary>
    public class HttpResponse : HttpBase, IDisposable
    {
        /// <summary>
        /// Status code of the HTTP response
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// HTTP response content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Response stream
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpResponse()
            : base()
        {
            // set default status code and content type
            this.StatusCode = HttpStatusCode.OK;
            this.ContentType = "text/html";
        }
    
        /// <summary>
        /// Close response stream
        /// </summary>
        public void CloseStream()
        {
            if (this.Stream != null)
            {
                this.Stream.Dispose();
            }
        }

        public void Dispose()
        {
            this.CloseStream();
        }
    }
}
