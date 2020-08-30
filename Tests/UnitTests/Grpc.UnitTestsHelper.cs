namespace Contoso.Grpc.UnitTests
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public static class UnitTestsHelper
    {
        internal static GrpcServer StartGrpcServer(int port, bool isRetryEnabled)
        {
            // max 3 times attemps, including the original request.
            RetryPolicy retryPolicy = isRetryEnabled ?
                                            new RetryPolicy(3, (float)0.1, 5, 2) :
                                            null;

            GrpcServerLegacyQueryHandlerBase mockLegacyQueryHandler = new RetryableLegacyQueryHandler();
            //GrpcServerLegacyQueryHandlerBase mockLegacyQueryHandler = 
            //                                    isRetryEnabled ? 
            //                                        (GrpcServerLegacyQueryHandlerBase)new MockRetryableLegacyQueryHandler() : 
            //                                        (GrpcServerLegacyQueryHandlerBase)new MockLegacyQueryHandler();
            LegacyQueryService legacyQueryService = GrpcServer.CreateLegacyQueryService(mockLegacyQueryHandler);

            GrpcServer grpcServer = new GrpcServer(
                port, legacyQueryService, retryPolicy: retryPolicy);
            grpcServer.Start();

            return grpcServer;
        }

        internal static void StopGrpcServer(GrpcServer grpcServer)
        {
            grpcServer.Shutdown();
        }

        internal static int FindOpenPort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            Console.WriteLine($"Found available port {port}");
            return port;
        }
    }
}
