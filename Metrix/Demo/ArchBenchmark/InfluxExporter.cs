using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using InfluxDB.Collector;

namespace ArchBenchmark
{
    public class InfluxExporter : IExporter
    {
        public string Name => nameof(InfluxExporter);

        private readonly Uri influxServer;
        private readonly string measurement;

        public InfluxExporter(Uri influxServer, string measurement)
        {
            this.influxServer = influxServer;
            this.measurement = measurement;
        }

        public IEnumerable<string> ExportToFiles(Summary summary, ILogger consoleLogger)
        {
            using (var writer = CreateInfluxWriter(influxServer))
            {
                foreach (var report in summary.Reports)
                {
                    var tags = new Dictionary<string, string>
                    {
                        { "class", report.Benchmark.Target.Type.Name },
                        { "method", report.Benchmark.Target.MethodDisplayInfo }
                    };

                    var nanoseconds = report.ResultStatistics.Mean;

                    writer.Measure(measurement, nanoseconds, tags);
                }
            }

            return Enumerable.Empty<string>();
        }

        public void ExportToLog(Summary summary, ILogger logger)
        {
            throw new NotSupportedException();
        }

        public static MetricsCollector CreateInfluxWriter(Uri influxServer)
        {
            return new CollectorConfiguration()
                .Tag.With("host", Environment.MachineName.ToLower(CultureInfo.InvariantCulture))
                .Batch.AtInterval(TimeSpan.FromSeconds(1))
                .WriteTo.InfluxDB(influxServer, "benchmarks")
                .CreateCollector();
        }
    }
}