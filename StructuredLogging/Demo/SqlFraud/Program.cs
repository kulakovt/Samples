using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using Dapper;
using Serilog;
using Serilog.Context;

namespace SqlFraud
{
    public sealed class Program
    {
        private const string ConnectionString = "Server=(local);Database=DotNext;Trusted_Connection=True;";
#pragma warning disable 618
        private static readonly long MaxAddress = IPAddress.Parse("255.255.255.255").Address;
#pragma warning restore 618

        private readonly Random random = new Random();
        private readonly ILogger baseLogger;
        private readonly ILogger purchaseLogger;

        private Program()
        {
            baseLogger = InitializeLogger();
            purchaseLogger = baseLogger.ForContext("EventType", "PurchaseCommited");
        }

        public static void Main()
        {
            Console.Title = "Fraud Generator";

            var program = new Program();
            program.GenerateLogs();
        }

        private void GenerateLogs()
        {
            var orders = LoadOrders();
            baseLogger.Debug("Start generating logs for {OrderCount} orders", orders.Count);

            var users = orders.GroupBy(o => o.UserId);
            foreach (var user in users)
            {
                var userId = user.Key;
                var userOrders = user.ToList();
                var fraudCount = random.Next(1, 4);

                var homeAddress = NewAddress();
                GenerateUserLog(userId, homeAddress, userOrders.Skip(fraudCount));

                var guestAddress = NewAddress();
                GenerateUserLog(userId, guestAddress, userOrders.Take(fraudCount));
            }

            baseLogger.Debug("Logs generated");
        }

        private void GenerateUserLog(int userId, IPAddress userAddress, IEnumerable<Order> orders)
        {
            using (LogContext.PushProperty("UserAddress", userAddress))
            {
                foreach (var order in orders)
                {
                    purchaseLogger.Information(
                        "User {UserId} made a purchase {Product} x{Quantity} (#{OrderId})",
                        userId,
                        order.Product,
                        order.Quantity,
                        order.Id);
                }
            }
        }

        private static ILogger InitializeLogger()
        {
            var configuration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.LiterateConsole(
                    outputTemplate: "[{Timestamp:HH:mm:ss.FFFF}] {Message}{NewLine}{Exception}")
                .WriteTo.MSSqlServer(ConnectionString, "Logs",
                    additionalDataColumns: new[] { new DataColumn { ColumnName = "EventType", DataType = typeof(string) } });

            return configuration.CreateLogger();
        }

        private static IReadOnlyList<Order> LoadOrders()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var orders = connection.Query<Order>("select * from Orders").ToList();
                return orders;
            }
        }

        private IPAddress NewAddress()
        {
            var buff = new byte[8];
            random.NextBytes(buff);
            long longRand = BitConverter.ToInt64(buff, 0);

            return new IPAddress(Math.Abs(longRand % MaxAddress));
        }
    }
}
