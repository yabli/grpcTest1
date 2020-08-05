namespace Contoso.Grpc
{
    using System;
    using System.Collections.Generic;
    using global::Grpc.Core;

    internal class GrpcServer
    {
        private const string GRPC_ARG_SERVICE_CONFIG = "grpc.service_config";

        public static LegacyQueryService CreateLegacyQueryService(GrpcServerLegacyQueryHandlerBase legacyQueryHandler)
        {
            if (legacyQueryHandler == null)
            {
                throw new ArgumentNullException(nameof(legacyQueryHandler));
            }

            return new LegacyQueryService(legacyQueryHandler);
        }

        private readonly Server server;
        private readonly int maxWaitTimeForStopMilliseconds;

        public GrpcServer(
            int port,
            LegacyQueryService legacyQueryService,
            int maxWaitTimeForStopMilliseconds = 30000,
            RetryPolicy retryPolicy = null)
        {
            if (legacyQueryService == null)
            {
                throw new ArgumentNullException(nameof(legacyQueryService));
            }

            ChannelOption[] channelOptions = this.BuildChannelOptions(
                new IGrpcService[] { legacyQueryService }, retryPolicy);
            
            this.server = new Server(channelOptions)
            {
                Services = {
                    LegacyQuery.BindService(legacyQueryService)
                },

                // find more regarding Auth and ServerCredentials here https://grpc.io/docs/guides/auth/
                Ports = {
                    new ServerPort("localhost", port, ServerCredentials.Insecure)
                }
            };

            this.maxWaitTimeForStopMilliseconds = maxWaitTimeForStopMilliseconds;
        }

        public void Start()
        {
            this.server.Start();
        }

        public void Shutdown()
        {
            this.server.ShutdownAsync().Wait(this.maxWaitTimeForStopMilliseconds);
        }

        /// <summary>
        /// To build a gRpc service config includes retry policy for given gRpcServices
        /// The service config will look like this
		///      "{\n"
		///      "  \"methodConfig\": [ {\n"
		///      "    \"name\": [\n"
		///      "      { \"service\": \"service\", \"method\": \"method\" }\n"
		///      "    ],\n"
		///      "    \"retryPolicy\": {\n"
		///      "      \"maxAttempts\": 2,\n"
		///      "      \"initialBackoff\": \"1s\",\n"
		///      "      \"maxBackoff\": \"120s\",\n"
		///      "      \"backoffMultiplier\": 1.6,\n"
		///      "      \"retryableStatusCodes\": [ \"ABORTED\" ]\n"
		///      "    }\n"
		///      "  } ]\n"
        ///      "}");
        /// </summary>
        /// <param name="gRpcServices"></param>
        /// <param name="retryPolicy"></param>
        /// <returns></returns>
        private ChannelOption[] BuildChannelOptions(IGrpcService[] gRpcServices, RetryPolicy retryPolicy)
        {
            if (retryPolicy == null)
            {
                return Array.Empty<ChannelOption>();
            }
            else
            {
                string retryPolicyString = retryPolicy.BuildPolicyString();
                List<string> gRpcServicesNames = new List<string>();
                foreach(var gRpcService in gRpcServices)
                {
                    foreach(var methodName in gRpcService.MethodsName)
                    {
                        gRpcServicesNames.Add($"{{\"service\":\"{gRpcService.ServiceName}\", \"method\":\"{methodName}\"}}");
                    }
                }

                string serviceNames = string.Join(",", gRpcServicesNames);
                string serviceNameString = $"\"name\":[{serviceNames}]";
                string serviceConfig = $"{{\"methodConfig\":[{{{serviceNameString},{retryPolicyString}}}]}}";
                ChannelOption channelOption = new ChannelOption(GRPC_ARG_SERVICE_CONFIG, serviceConfig);

                return new ChannelOption[] { channelOption };
            }
        }
    }
}
