namespace Contoso.Grpc
{
    using System.Threading.Tasks;
    using System;
    using global::Grpc.Core;

    /// <summary>
    /// Mock Legacy query handler to test retry scenario.
    /// It will fail on the first or second attemps, and succeed on the 3rd attempt.
    /// </summary>
    public class RetryableLegacyQueryHandler : GrpcServerLegacyQueryHandlerBase
    {
        private const string HeaderMachineName = "X-MachineName";
        private const string HeaderMachineIP = "X-MachineIP";
        private const string HeaderMachineIPV6 = "X-MachineIPV6";

        internal const string PreviousAttemptsHeaderKey = "grpc-previous-rpc-attempts";

        public override Task LegacyQuery(
            LegacyQueryRequest request,
            IServerStreamWriter<LegacyQueryResponse> responseStream,
            ServerCallContext context)
        {
            if (TryGetRetryAttempts(context, out int attempts) && attempts > 1)
            {
                this.SetResponseHeader(context.ResponseTrailers);
                byte[] bytes = new byte[] { 88, 99 };
                return responseStream.WriteAsync(new LegacyQueryResponse());
            }
            else
            {
                throw new RpcException(new Status(StatusCode.Unavailable, "Failing the request to test retry."), "failed for retry");
            }
        }

        private static bool TryGetRetryAttempts(ServerCallContext serverCallContext, out int attemps)
        {
            attemps = 0;
            if (serverCallContext.RequestHeaders == null || serverCallContext.RequestHeaders.Count == 0)
            {
                return false;
            }
            
            for(int i = 0; i < serverCallContext.RequestHeaders.Count; i++)
            {
                var entry = serverCallContext.RequestHeaders[i];
                if (entry.Key.Equals(PreviousAttemptsHeaderKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(entry.Value, out attemps))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private void SetResponseHeader(Metadata trailer)
        {
            this.AddHeader(trailer, HeaderMachineName, "LocalHost" ?? "null");

            this.AddHeader(trailer, HeaderMachineIP, "127.0.0.1");
            
            this.AddHeader(trailer, HeaderMachineIPV6, "::1");
        }

        private void AddHeader(Metadata trailer, string key, string value)
        {
            trailer.Add(key, value);
        }
    }
}
