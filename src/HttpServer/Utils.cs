// Copyright (c) Peter Nylander.  All rights reserved.

using System.Net;

namespace HttpServer
{
    public static class Utils
    {
        /// <summary>
        /// Map an HTTP status code to reason string
        /// </summary>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>Reason mapped</returns>
        public static string MapStatusCodeToReason(HttpStatusCode statusCode)
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
    }
}
