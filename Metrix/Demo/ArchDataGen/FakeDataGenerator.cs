using System;
using System.Collections.Generic;
using System.Diagnostics;
using ArchBenchmark;

namespace ArchDataGen
{
    internal sealed class FakeDataGenerator
    {
        private static readonly IReadOnlyDictionary<int, int> MoonDays = new Dictionary<int, int>
        {
            [01] = 12,
            [02] = 11,
            [03] = 12,
            [04] = 10,
            [05] = 10,
            [06] = 09,
            [07] = 09,
            [08] = 08,
            [09] = 06,
            [10] = 06,
            [11] = 04,
            [12] = 04
        };

        private readonly ArchBenchmark.ArchBenchmark arch;

        private FakeDataGenerator()
        {
            arch = new ArchBenchmark.ArchBenchmark();
        }

        public static void Run()
        {
            new FakeDataGenerator().Generate();
        }

        private void Generate()
        {
            using (var writer = InfluxExporter.CreateInfluxWriter(new Uri(ArchBenchmark.ArchBenchmark.InfluxServer)))
            {
                var now = DateTime.UtcNow.Date;

                for (int past = -360; past <= 0 ; past++)
                {
                    var timestamp = now.AddDays(past);

                    var tags = new Dictionary<string, string>
                    {
                        { "class", nameof(ArchBenchmark.ArchBenchmark) },
                        { "method", nameof(ArchBenchmark.ArchBenchmark.PackFast) }
                    };

                    var value = new Dictionary<string, object> { { "value", Measure(timestamp, () => arch.PackFast()) } };
                    writer.Write(ArchBenchmark.ArchBenchmark.Measurement, value, tags, timestamp);

                    tags["method"] = nameof(ArchBenchmark.ArchBenchmark.PackSlow);
                    value = new Dictionary<string, object> { { "value", Measure(timestamp, () => arch.PackSlow()) } };
                    writer.Write(ArchBenchmark.ArchBenchmark.Measurement, value, tags, timestamp);

                    Console.WriteLine(past);
                }
            }
        }

        private double Measure(DateTime time, Action action)
        {
            const int moonIsComming = 5;
            const int nanosecondsPerMillisecond = 1000 * 1000;

            var moonDay = MoonDays[time.Month];
            var moonPower = Math.Abs(moonDay - time.Day);
            var moonTime = moonPower > moonIsComming ? 0 : moonIsComming - moonPower;

            var timer = Stopwatch.StartNew();
            action();
            return (timer.Elapsed.TotalMilliseconds + moonTime) * nanosecondsPerMillisecond;
        }
    }
}