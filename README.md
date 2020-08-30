# grpcTest1

Projects:
- src/Protos: Protobuf schemas
- src/GrpcServer: the gRpc server which can response to /Contoso.Grpc.LegacyQuery/LegacyQuery gRpc request. RetryableLegacyQueryHandler is the handler, it fails the first attempt by throwing a RpcExecption. Will send expected response on the 2nd attempt.
- Tests/UnitTests: You can run this exe to repro the reported issue.

The test crashes after client received the first failure response (on the first attempt of request).
