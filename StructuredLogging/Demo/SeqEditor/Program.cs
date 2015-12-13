using Serilog;

namespace SeqEditor
{
    public sealed class Program
    {
        public static readonly ILogger BaseLogger = InitializeLogger();

        public static void Main(string[] args)
        {
            var story = new Story();
            story.Run();
        }

        private static ILogger InitializeLogger()
        {
            
            var configuration = new LoggerConfiguration()
                //.Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .MinimumLevel.Verbose()
                .WriteTo.Seq("http://localhost:5342")
                .WriteTo.LiterateConsole(
                    outputTemplate: "[{Timestamp:HH:mm:ss.FFFF}] {Message}{NewLine}{Exception}");

            return configuration.CreateLogger();
        }
    }
}
