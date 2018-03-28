using BenchmarkDotNet.Running;

namespace ArchBenchmark
{
    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<ArchBenchmark>();
        }
    }
}
