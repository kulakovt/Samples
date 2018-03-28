using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;

namespace ArchBenchmark
{
    [InfluxExporter(InfluxServer, Measurement)]
    [Config(typeof(MyConfig))]
    public class ArchBenchmark
    {
        public const string InfluxServer = "http://127.0.0.1:8086";
        public const string Measurement = "arch";

        private readonly byte[] data;

        public ArchBenchmark()
        {
            data = new byte[1000 * 1000];
            new Random().NextBytes(data);
        }

        [Benchmark]
        public byte[] PackFast()
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(data);
            }
        }

        [Benchmark]
        public byte[] PackSlow()
        {
            using (var sha512 = SHA512.Create())
            {
                return sha512.ComputeHash(data);
            }
        }
    }
}
