using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    public enum HttpMethod
    {
        CONNECT,
        DELETE,
        GET,
        HEAD,
        OPTIONS,
        POST,
        PUT,
        TRACE
    }

    internal class HttpMethodParser
    {
        public static HttpMethod Parse(String method)
        {
            switch (method)
            {
                case "OPTIONS":
                    return HttpMethod.OPTIONS;
                case "GET":
                    return HttpMethod.GET;
                case "HEAD":
                    return HttpMethod.HEAD;
                case "POST":
                    return HttpMethod.POST;
                case "PUT":
                    return HttpMethod.PUT;
                case "DELETE":
                    return HttpMethod.DELETE;
                case "TRACE":
                    return HttpMethod.TRACE;
                case "CONNECT":
                    return HttpMethod.CONNECT;
                default:
                    return 0;
                // throw new HttpException(HttpErrorCodes.HttpRequestParseError);
            }
        }
    }
}
