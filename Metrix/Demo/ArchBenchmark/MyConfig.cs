using System;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace ArchBenchmark
{
    public class MyConfig : ManualConfig
    {
        public MyConfig()
        {
            var job = new Job(nameof(Job.InProcess), InfrastructureMode.InProcess);
            job.Run.LaunchCount = 1;
            job.Run.WarmupCount = 1;
            job.Run.TargetCount = 3;
            job.Run.RunStrategy = RunStrategy.ColdStart;
            job.Run.UnrollFactor = 1;

            Add(job);
        }
    }

    public class InfluxExporterAttribute : ExporterConfigBaseAttribute
    {
        public InfluxExporterAttribute(string influxServer, string measurement)
            : base(new InfluxExporter(new Uri(influxServer), measurement))
        {
        }
    }
}