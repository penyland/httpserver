using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    /// <summary>
    /// Base class for HTTP object (request/response)
    /// </summary>
    public class HttpBase
    {
        #region Constants ...

        // const string separator
        protected const char CR = '\r';
        protected const char LF = '\n';

        // request line separator
        protected const char REQUEST_LINE_SEPARATOR = ' ';
        // starting of query string
        protected const char QUERY_STRING_SEPARATOR = '?';
        // query string parameters separator
        protected const char QUERY_STRING_PARAMS_SEPARATOR = '&';
        // query string value separator
        protected const char QUERY_STRING_VALUE_SEPARATOR = '=';
        // header-value separator 
        protected const char HEADER_VALUE_SEPARATOR = ':';
        // form parameters separator
        protected const char FORM_PARAMS_SEPARATOR = '&';
        // form value separator
        protected const char FORM_VALUE_SEPARATOR = '=';

        #endregion

        #region Properties ...

        /// <summary>
        /// Headers of the HTTP request/response
        /// </summary>
        public NameValueCollection Headers { get; protected set; }

        /// <summary>
        /// Body of HTTP request/response
        /// </summary>
        public string Body { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpBase()
        {
            this.Headers = new NameValueCollection();
        }
    }
}
