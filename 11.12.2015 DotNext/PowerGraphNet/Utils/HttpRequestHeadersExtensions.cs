using System;
using System.Linq;
using System.Net.Http.Headers;

namespace PowerGraphNet.Utils
{
    public static class HttpRequestHeadersExtensions
    {
        public static string GetAppRequestId(this HttpRequestHeaders header)
        {
            if (header == null) throw new ArgumentNullException("header");

            var appRequestIds = header.GetValues("AppRequestId");
            var appRequestId = appRequestIds.FirstOrDefault();

            if (appRequestId == null)
            {
                throw new ApplicationException("AppRequestId not found");
            }

            return appRequestId;
        }
    }
}