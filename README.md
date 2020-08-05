# grpcTest1

Projects:
- src/Protos: Protobuf schemas
- src/GrpcServer: the gRpc server which can response to /Contoso.Grpc.LegacyQuery/LegacyQuery gRpc request. GrpcServerLegacyQueryHandlerBase is the abstract handler, the implementation is in MockServer
- Tests/UnitTests.MockServer: the Mock server which contains an implementation of GrpcServerLegacyQueryHandlerBase with Retry logic. it fails the first and second attempts, will send response on the 3rd attempt. The mock server also starts GrpcServer in an exe (Grpc.UnitTests.exe).
- Tests/UnitTests: Test case TestGrpcServer.TestsRetry() is in this UT project. It starts Grpc.UnitTests.exe in a process, then sends retriable request to Grpc.UnitTests.exe.

The test crashes after client received the first failure response (on the first attempt of request).
