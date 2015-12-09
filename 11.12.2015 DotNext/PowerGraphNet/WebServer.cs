using System;
using System.Globalization;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using PowerGraphNet.Utils;
using Serilog;

namespace PowerGraphNet
{
    public sealed class WebServer
    {
        private static ILogger Logger;
        public static string ServiceName;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("Enter service name");
            }

            ServiceName = args[0];
            Console.Title = ServiceName;

            using (Start(GetAddressFor(ServiceName)))
            {
                Console.ReadLine();
            }

            Logger.Information("Web server {ServiceName} shutdown", ServiceName);
        }

        private static IDisposable Start(Uri remoteAddress)
        {
            LogFactory.Initialize(ServiceName);
            Logger = LogFactory.CreateLogger();
            
            var listenAddress = ConvertToListenAddress(remoteAddress);
            var listener = WebApp.Start(listenAddress, InitializeApp);

            Logger.Information("Web server {ServiceName} started at {ServicePort}", ServiceName, remoteAddress.Port);

            return listener;
        }

        private static void InitializeApp(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            app.Use<LoggerMiddleware>();
            app.UseWebApi(config);
        }

        private static string ConvertToListenAddress(Uri remoteAddress)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}://+:{1}/", remoteAddress.Scheme, remoteAddress.Port);
        }

        private static Uri GetAddressFor(string serviceName)
        {
            switch (serviceName)
            {
                case "Gateway":
                    return ClusterConfig.GatewayAddress;
                case "Auth":
                    return ClusterConfig.AuthAddress;
                case "Static":
                    return ClusterConfig.StaticAddress;
                case "Text":
                    return ClusterConfig.TextAddress;
                case "Image":
                    return ClusterConfig.ImageAddress;
                case "Ad":
                    return ClusterConfig.AdAddress;
            }

            throw new ArgumentOutOfRangeException(serviceName);
        }
    }
}
