namespace Contoso.Grpc.UnitTests.MockServer
{
    using System;

    class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Command Input")]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;

            bool isRetryEnabled = false;

            int port = 82;
            if (args.Length > 0 && int.TryParse(args[0], out int portArg))
            {
                port = portArg;
            }

            if (args.Length > 1 && args[1].Equals("RetryEnabled", StringComparison.OrdinalIgnoreCase))
            {
                isRetryEnabled = true;
            }

            // max 3 times attemps, including the original request.
            RetryPolicy retryPolicy = isRetryEnabled ?
                                            new RetryPolicy(3, (float)0.1, 5, 2) :
                                            null;

            GrpcServerLegacyQueryHandlerBase mockLegacyQueryHandler = new MockRetryableLegacyQueryHandler();
            //GrpcServerLegacyQueryHandlerBase mockLegacyQueryHandler = 
            //                                    isRetryEnabled ? 
            //                                        (GrpcServerLegacyQueryHandlerBase)new MockRetryableLegacyQueryHandler() : 
            //                                        (GrpcServerLegacyQueryHandlerBase)new MockLegacyQueryHandler();
            LegacyQueryService legacyQueryService = GrpcServer.CreateLegacyQueryService(mockLegacyQueryHandler);

            GrpcServer grpcServer = new GrpcServer(
                port, legacyQueryService, retryPolicy: retryPolicy);
            grpcServer.Start();

            Console.WriteLine($"gRPC server is started on port {port}.");
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            grpcServer.Shutdown();
        }

        private static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ToString());
            Console.WriteLine(e.ExceptionObject.ToString());
        }
    }
}
