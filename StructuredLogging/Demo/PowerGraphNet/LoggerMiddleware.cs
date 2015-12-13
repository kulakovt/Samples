using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Owin;
using PowerGraphNet.Utils;
using Serilog;
using Serilog.Context;

namespace PowerGraphNet
{
    public sealed class LoggerMiddleware : OwinMiddleware
    {
        private static readonly ILogger RequestLogger = LogFactory
            .CreateLogger()
            .ForEventType("HttpRequest");

        private static readonly ILogger ResponseLogger = LogFactory
            .CreateLogger()
            .ForEventType("HttpResponse");

        public LoggerMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            var requestId = EnsureRequestId(request);
            var referer = GetReferer(request);

            var requestLogger = RequestLogger.ForContext("Referer", referer);

            using (LogContext.PushProperty("AppRequestId", requestId))
            {
                requestLogger.Information(
                    "Processing {RequestMethod} {Path}",
                    request.Method,
                    request.Path.Value);

                if (request.QueryString.HasValue)
                {
                    requestLogger.Information("Query {RequestQuery}", request.QueryString.Value);
                }

                var timer = Stopwatch.StartNew();

                await Next.Invoke(context);

                timer.Stop();
                ResponseLogger.Information(
                    "Responce {ResponseStatus} {ResponseStatusName} ({RequestDuration})",
                    context.Response.StatusCode,
                    context.Response.ReasonPhrase,
                    timer.Elapsed);
            }
        }

        private static string EnsureRequestId(IOwinRequest request)
        {
            string appRequestId = request.Headers["AppRequestId"];

            if (appRequestId == null)
            {
                appRequestId = Guid.NewGuid().ToString("N");
                request.Headers["AppRequestId"] = appRequestId;
            }

            return appRequestId;
        }

        private static string GetReferer(IOwinRequest request)
        {
            return request.Headers["Referer"];
        }
    }
}