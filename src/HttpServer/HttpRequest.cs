using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace HttpServer
{
    /// <summary>
    /// Class for HTTP request
    /// </summary>
    public class HttpRequest : HttpBase
    {
        // size of request buffer
        private const int BUFFER_REQUEST_SIZE = 1024;

        /// <summary>
        /// HTTP request method
        /// </summary>
        public HttpMethod HttpMethod { get; internal set; }

        /// <summary>
        /// URL request
        /// </summary>
        // public string URL { get; internal set; }
        public string URL { get; set; }

        /// <summary>
        /// HTTP request protocol
        /// </summary>
        public string HttpProtocol { get; internal set; }

        /// <summary>
        /// HTTP query string
        /// </summary>
        public NameValueCollection QueryString { get; internal set; }

        /// <summary>
        /// HTTP form data
        /// </summary>
        public NameValueCollection Form { get; internal set; }

        /// <summary>
        /// Request Content type
        /// </summary>
        public string ContentType { get; internal set; }

        /// <summary>
        /// Request content length
        /// </summary>
        public long ContentLength { get; internal set; }

        public string UserHostAddress { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpRequest()
            : base()
        {
            this.QueryString = new NameValueCollection();
            this.Form = new NameValueCollection();
        }

        /// <summary>
        /// Parse the request 
        /// </summary>
        /// <param name="requestSocket">Socket</param>
        internal async static Task<HttpRequest> Parse(StreamSocket requestSocket)
        {
            string request = string.Empty;

            // TODO: Read only header at first to avoid OOM exceptions
            //       Read one byte at the time and check if we have found the ending of the header. Ending of the header is two \r\n in a row
            //       We can then extract content length and check if it's too large.
            // TODO: Expose body as (memory?) stream
            using (DataReader dataReader = new DataReader(requestSocket.InputStream))
            {
                dataReader.InputStreamOptions = InputStreamOptions.Partial;

                uint sizeFieldCount = await dataReader.LoadAsync(BUFFER_REQUEST_SIZE);

                // Read HTTP Header
                request = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                request = request.TrimEnd('\0');
            }

            Debug.WriteLine(request);

            HttpRequest httpRequest = new HttpRequest();

            httpRequest.UserHostAddress = requestSocket.Information.RemoteHostName.ToString();

            // split request lines on line feed
            string[] lines = request.Split(LF);

            int i = 0;
            // trim request line on carriage return
            lines[i] = lines[i].TrimEnd(CR);

            // process request line (method, url, protocol version)
            string[] requestLineTokens = lines[i].Split(REQUEST_LINE_SEPARATOR);

            // method
            httpRequest.HttpMethod = HttpMethodParser.Parse(requestLineTokens[0]);

            // url, find start of query string (if exists)
            int idxQueryString = requestLineTokens[1].IndexOf(QUERY_STRING_SEPARATOR);
            httpRequest.URL = (idxQueryString != -1) ?
                requestLineTokens[1].Substring(0, idxQueryString).Trim('/') : requestLineTokens[1].Trim('/');

            // protocol
            httpRequest.HttpProtocol = requestLineTokens[2];

            // parsing query string
            if (idxQueryString != -1)
            {
                string queryString = requestLineTokens[1].Substring(idxQueryString + 1);
                if (queryString != String.Empty)
                {
                    string[] queryStringParams = queryString.Split(QUERY_STRING_PARAMS_SEPARATOR);
                    foreach (string queryStringParam in queryStringParams)
                    {
                        string[] queryStringParamTokens = queryStringParam.Split(QUERY_STRING_VALUE_SEPARATOR);
                        string queryStringParamValue = null;
                        // if there is key-value pair
                        if (queryStringParamTokens.Length == 2)
                            queryStringParamValue = queryStringParamTokens[1];

            // httpRequest.QueryString.Add(queryStringParamTokens[0], HttpServerUtility.HtmlDecode(queryStringParamValue));
                        httpRequest.QueryString.Add(queryStringParamTokens[0], Uri.UnescapeDataString(queryStringParamValue));
                    }
                }
            }

            // next line (header start)
            i++;
            // trim end carriage return of each line
            lines[i] = lines[i].TrimEnd(CR);

            // headers end with empty string
            while (lines[i] != String.Empty)
            {
                int separatorIndex = lines[i].IndexOf(HEADER_VALUE_SEPARATOR);

                if (separatorIndex != -1)
                    httpRequest.Headers.Add(lines[i].Substring(0, separatorIndex), lines[i].Substring(separatorIndex + 1).Trim());

                i++;
                // trim end carriage return of each line
                lines[i] = lines[i].TrimEnd(CR);
            }

            // next line (body start)
            i++;

            // content length specified
            if (httpRequest.Headers.ContainsKey("Content-Length"))
            {
                httpRequest.ContentLength = Convert.ToInt64(httpRequest.Headers["Content-Length"]);
                httpRequest.Body = lines[i].TrimEnd(CR).Substring(0, Convert.ToInt32(httpRequest.Headers["Content-Length"].ToString()));
            }

            if (httpRequest.Headers.ContainsKey("Content-Type"))
            {
                httpRequest.ContentType = httpRequest.Headers["Content-Type"];
            }

            // fill form parameters collection
            if (httpRequest.Headers.ContainsKey("Content-Type") &&
                (httpRequest.Headers["Content-Type"].StartsWith("application/x-www-form-urlencoded")))
            {
                string[] formParams = httpRequest.Body.Split(FORM_PARAMS_SEPARATOR);
                foreach (string formParam in formParams)
                {
                    string[] formParamTokens = formParam.Split(FORM_VALUE_SEPARATOR);
                    string formParamValue = null;
                    // if there is key-value pair
                    if (formParamTokens.Length == 2)
                        formParamValue = formParamTokens[1];

                    // httpRequest.Form.Add(formParamTokens[0], HttpServerUtility.HtmlDecode(formParamValue));
                    httpRequest.Form.Add(formParamTokens[0], Uri.UnescapeDataString(formParamValue));
                }
            }

            return httpRequest;
        }
    }
}
