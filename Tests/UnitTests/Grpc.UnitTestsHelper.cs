namespace Contoso.Grpc.UnitTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading;

    public static class UnitTestsHelper
    {
        private static StreamReader StandardErrorStream;
        private static StreamReader StandardOutputStream;

        private static Assembly MockGrpcServerAssembly = typeof(MockServer.MockRetryableLegacyQueryHandler).Assembly;
        private static string MockGrpcServerPath = MockGrpcServerAssembly.Location;
        private static string MockGrpcServerDirectory = Path.GetDirectoryName(MockGrpcServerPath);
        private static string TimeStampTag = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        private static string StandardErrorFileName = Path.Combine(MockGrpcServerDirectory, "MockGrpc_StdOut_" + TimeStampTag + ".log");
        private static string StandardOutputFileName = Path.Combine(MockGrpcServerDirectory, "MockGrpc_StdErr_" + TimeStampTag + ".log");

        private static Thread StandardOutputThread = null;
        private static Thread StandardErrorThread = null;

        internal static Process StartsMockGrpcServer(int port, bool hideWindow = true)
        {
            Process mockGrpcServerProcess = StartsProcess(
                                                MockGrpcServerPath,                 // filePath
                                                port.ToString() + " RetryEnabled",  // argument
                                                //port.ToString(),                  // argument
                                                MockGrpcServerDirectory,            // workingDirectory
                                                hideWindow);

            return mockGrpcServerProcess;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Reliability", 
            "CA2000:Dispose objects before losing scope", 
            Justification = "The caller will take responsibility to dispose.")]
        private static Process StartsProcess(
            string filePath, 
            string argument, 
            string workingDirectory, 
            bool hideWindow)
        {

            Console.WriteLine("Starting Server process");

            ProcessStartInfo processStartInfo = new ProcessStartInfo(filePath, argument) 
            {
                UseShellExecute = hideWindow ? false : true,
                CreateNoWindow = hideWindow,
                WindowStyle = hideWindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                //CreateNoWindow = false,
                //WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = $@"{workingDirectory}",
                RedirectStandardOutput = hideWindow,
                RedirectStandardError = hideWindow,
                Verb = "runas",
            };

            // collects gRpc traces for tests https://github.com/grpc/grpc/blob/master/TROUBLESHOOTING.md
            processStartInfo.EnvironmentVariables["GRPC_VERBOSITY"] = "DEBUG";
            processStartInfo.EnvironmentVariables["GRPC_TRACE"] = "all";

            Process process = new Process();
            //process.OutputDataReceived += new DataReceivedEventHandler
            //(
            //    delegate (object sender, DataReceivedEventArgs e)
            //    {
            //        // append the new data to the data already read-in
            //        Console.WriteLine("Std: " + e.Data);
            //    }
            //);

            //process.ErrorDataReceived += new DataReceivedEventHandler
            //(
            //    delegate (object sender, DataReceivedEventArgs e)
            //    {
            //        // append the new data to the data already read-in
            //        Console.WriteLine("Err: " + e.Data);
            //    }
            //);

            process.StartInfo = processStartInfo;
            process.Exited += new EventHandler((object o, EventArgs ea) => StopsProcess(process, 10000));
            Console.WriteLine(process.StartInfo.WorkingDirectory + ">" + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
            process.Start();

            if (process.StartInfo.RedirectStandardError)
            {
                StandardErrorStream = process.StandardError;
                StandardErrorThread = startThread(new ThreadStart(WriteStandardError), "StandardError");
            }

            if (process.StartInfo.RedirectStandardOutput)
            {
                StandardOutputStream = process.StandardOutput;
                StandardOutputThread = startThread(new ThreadStart(WriteStandardOutput), "StandardOutput");
            }

            //if (hideWindow)
            //{
            //    process.BeginOutputReadLine();
            //    process.BeginErrorReadLine();
            //}

            return process;
        }

        internal static void StopsProcess(Process process, int timeouInMilliseconds)
        {
            if (process == null || process.HasExited)
            {
                return;
            }

            process.Kill();
            process.WaitForExit(timeouInMilliseconds);

            if (StandardOutputThread != null)
            {
                StandardOutputThread.Join();
            }

            if (StandardErrorThread != null)
            {
                StandardErrorThread.Join();
            }
        }

        internal static bool IsProcessRunning(Process process)
        {
            if (process == null || process.HasExited)
            {
                return false;
            }

            return true;
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

        /// <summary>Start a thread.</summary>
        /// <param name="startInfo">start information for this thread</param>
        /// <param name="name">name of the thread</param>
        /// <returns>thread object</returns>
        private static Thread startThread(ThreadStart startInfo, string name)
        {
            Thread t = new Thread(startInfo);
            t.IsBackground = true;
            t.Name = name;
            t.Start();
            return t;
        }

        private static void WriteStandardOutput()
        {
            using (StreamWriter writer = File.CreateText(StandardOutputFileName))
            using (StreamReader reader = StandardOutputStream)
            {
                writer.AutoFlush = true;

                for (; ; )
                {
                    string textLine = reader.ReadLine();

                    if (textLine == null)
                        break;

                    writer.WriteLine(textLine);
                }
            }

            if (File.Exists(StandardOutputFileName))
            {
                FileInfo info = new FileInfo(StandardOutputFileName);

                // if the error info is empty or just contains eof etc.

                if (info.Length < 4)
                    info.Delete();
            }
        }

        /// <summary>Thread which outputs standard error output from the running executable to the appropriate file.</summary>
        private static void WriteStandardError()
        {
            using (StreamWriter writer = File.CreateText(StandardErrorFileName))
            using (StreamReader reader = StandardErrorStream)
            {
                writer.AutoFlush = true;

                for (; ; )
                {
                    string textLine = reader.ReadLine();

                    if (textLine == null)
                        break;

                    writer.WriteLine(textLine);
                }
            }

            if (File.Exists(StandardErrorFileName))
            {
                FileInfo info = new FileInfo(StandardErrorFileName);

                // if the error info is empty or just contains eof etc.

                if (info.Length < 4)
                    info.Delete();
            }
        }
    }
}
