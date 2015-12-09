using System;

namespace PowerGraphNet
{
    internal static class ClusterConfig
    {
        public static readonly Uri GatewayAddress = Localhost(9000);
        public static readonly Uri AuthAddress = Localhost(9001);
        public static readonly Uri StaticAddress = Localhost(9002);
        public static readonly Uri TextAddress = Localhost(9003);
        public static readonly Uri ImageAddress = Localhost(9004);
        public static readonly Uri AdAddress = Localhost(9005);

        private static Uri Localhost(int port)
        {
            return new UriBuilder("http", Environment.MachineName, port).Uri;
        }
    }
}