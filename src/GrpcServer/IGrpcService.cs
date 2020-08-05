namespace Contoso.Grpc
{
    internal interface IGrpcService
    {
        string ServiceName { get; }

        string[] MethodsName { get; }
    }
}