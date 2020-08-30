namespace Contoso.Grpc.UnitTests
{
    using System;

    class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Command Input")]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;

            TestGrpcServer.TestFixtureSetup();
            TestGrpcServer test = new TestGrpcServer();

            try
            {
                test.TestsRetry().GetAwaiter().GetResult(); ;
            }
            finally
            {
                TestGrpcServer.TestFixtureTearDown();
            }
        }

        private static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ToString());
            Console.WriteLine(e.ExceptionObject.ToString());
        }
    }
}
