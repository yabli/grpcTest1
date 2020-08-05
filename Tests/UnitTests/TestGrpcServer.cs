namespace Contoso.Grpc.UnitTests
{
    using Contoso.Grpc;
    using global::Grpc.Core;
    using Google.Protobuf;
    using NUnit.Framework;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    [TestFixture]
    public class TestGrpcServer
    {
        private static Process MockGrpcServerProcess = null;
        private static int Port = UnitTestsHelper.FindOpenPort();
        private const string LocalHost = "127.0.0.1";

        [OneTimeSetUp]
        public static void TestFixtureSetup()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;

            // collects gRpc traces for tests https://github.com/grpc/grpc/blob/master/TROUBLESHOOTING.md
            System.Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            System.Environment.SetEnvironmentVariable("GRPC_TRACE", "all");

            MockGrpcServerProcess = UnitTestsHelper.StartsMockGrpcServer(Port, true);
        }

        [Test]
        public async Task TestsRetry()
        {
            ChannelOption[] channelOptions = new ChannelOption[]
            {
                new ChannelOption(
                    "grpc.service_config",
                    "{\"methodConfig\":[{\"name\":[{\"service\":\"Contoso.Grpc.LegacyQuery\",\"method\":\"LegacyQuery\"}],\"retryPolicy\":{\"maxAttempts\":3,\"initialBackoff\":\"0.5s\",\"maxBackoff\":\"5s\",\"backoffMultiplier\":2.0,\"retryableStatusCodes\":[\"UNAVAILABLE\"]}}]}")
            };

            Channel channel = new Channel($"{LocalHost}:{Port}", ChannelCredentials.Insecure, channelOptions);
            //Channel channel = new Channel($"{LocalHost}:{Port}", ChannelCredentials.Insecure);

            var client = new LegacyQuery.LegacyQueryClient(channel);
            
            byte[] bytes = new byte[] { 55, 66 };
            LegacyQueryRequest legacyQueryRequest = new LegacyQueryRequest()
            {
                LegacyAqm = ByteString.CopyFrom(bytes)
            };

            LegacyQueryResponse response = null;

            try
            {
                var call = client.LegacyQuery(legacyQueryRequest);
                while (await call.ResponseStream.MoveNext().ConfigureAwait(false))
                {
                    response = call.ResponseStream.Current;
                }

                Assert.IsNotNull(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
#pragma warning disable CS1058 // A previous catch clause already catches all exceptions
            catch
#pragma warning restore CS1058 // A previous catch clause already catches all exceptions
            {
                // Remove the same permission as above.
                Console.WriteLine("Non-CLS compliant exception caught.");
            }
        }

        [OneTimeTearDown]
        public static void TestFixtureTearDown()
        {
            if (UnitTestsHelper.IsProcessRunning(MockGrpcServerProcess))
            {
                UnitTestsHelper.StopsProcess(MockGrpcServerProcess, 10000);
            }
        }

        private static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
