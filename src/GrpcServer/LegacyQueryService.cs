namespace Contoso.Grpc
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Grpc.Core;

    public class LegacyQueryService : LegacyQuery.LegacyQueryBase, IGrpcService
    {
        public const string RequestSizeKey = "requestSize";

        private readonly GrpcServerLegacyQueryHandlerBase legacyQueryHandler;

        public string ServiceName
        {
            get { return Contoso.Grpc.LegacyQuery.Descriptor.FullName; }
        }

        public string[] MethodsName
        {
            get { return Contoso.Grpc.LegacyQuery.Descriptor.Methods.Select(m => m.Name).ToArray(); }
        }

        internal LegacyQueryService(GrpcServerLegacyQueryHandlerBase legacyQueryHandler)
        {
            this.legacyQueryHandler = legacyQueryHandler;
        }

        public override async Task LegacyQuery(
            LegacyQueryRequest request, 
            IServerStreamWriter<LegacyQueryResponse> responseStream, 
            ServerCallContext context)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            int requestSize = request.CalculateSize();
            context.UserState.Add(RequestSizeKey, requestSize);

            await this.legacyQueryHandler.LegacyQuery(request, responseStream, context)
                                         .ConfigureAwait(false);
        }
    }
}
