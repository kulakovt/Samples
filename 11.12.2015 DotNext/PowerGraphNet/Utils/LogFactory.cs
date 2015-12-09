using System;
using System.IO;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.IOFile;

namespace PowerGraphNet.Utils
{
    internal static class LogFactory
    {
        private static ILogger BaseLogger;

        public static void Initialize(string serviceName)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", serviceName + ".log");

            var configuration = new LoggerConfiguration()
                .Enrich.WithProperty("Host", serviceName)
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.LiterateConsole(
                    outputTemplate:
                        "[{Timestamp:HH:mm:ss.FFFF}] {Message}{NewLine}{Exception}")
                .WriteTo.Sink(new FileSink(filePath, new JsonFormatter(closingDelimiter: "," + Environment.NewLine, renderMessage: true), null));

            BaseLogger = configuration.CreateLogger();
        }

        public static ILogger CreateLogger()
        {
            return BaseLogger;
        }

        public static ILogger ForEventType(this ILogger baseLogger, string eventType)
        {
            return baseLogger.ForContext("EventType", eventType);
        }
     }
}