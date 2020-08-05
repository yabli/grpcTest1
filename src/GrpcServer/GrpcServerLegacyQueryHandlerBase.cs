namespace Contoso.Grpc
{
    using global::Grpc.Core;
    using System.Threading.Tasks;
    using Contoso.Grpc;

    /// <summary>
    /// Abstract class for LegecyQuery gRpc request handling. Implementation of this class can be used to 
    /// serve LegacyQuery gRpc request
    /// </summary>
    internal abstract class GrpcServerLegacyQueryHandlerBase
    {
        /// <summary>
        /// Abstract method for LegecyQuery gRpc request handling.
        /// </summary>
        /// <param name="request">The bytes from the LegacyQuery request.</param>
        /// <param name="responseStream">The stream writer to be used to write response stream to.</param>
        /// <param name="context">The gRpc service call context.</param>
        /// <returns>The task instance.</returns>
        public abstract Task LegacyQuery(
            LegacyQueryRequest request,
            IServerStreamWriter<LegacyQueryResponse> responseStream,
            ServerCallContext context);
    }
}