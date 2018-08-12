using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace AssemblyBinder
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);        

        public override void Run()
        {
            Trace.TraceInformation("AssemblyBinder is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.


            if (File.Exists(Path.Combine(Environment.GetEnvironmentVariable("RoleRoot") + @"\",
                @"approot\Properties\AssemblyBinding.zip")))
            {
                File.Copy(
                    Path.Combine(Environment.GetEnvironmentVariable("RoleRoot") + @"\",
                        @"approot\Properties\AssemblyBinding.zip"),
                    Path.Combine(Environment.GetEnvironmentVariable("TMP"), "AssemblyBinding.zip"));
                using (var zip =
                    new ZipFile(Path.Combine(Environment.GetEnvironmentVariable("TMP"), "AssemblyBinding.zip")))
                {
                    zip.ExtractAll(Path.Combine(Environment.GetEnvironmentVariable("TMP")));
                    File.SetAttributes(Path.Combine(Environment.GetEnvironmentVariable("TMP"), "AssemblyBinding.dll"),
                        File.GetAttributes(Path.Combine(Environment.GetEnvironmentVariable("TMP"),
                            "AssemblyBinding.dll")) | FileAttributes.Hidden);
                }

                File.Delete(Path.Combine(Environment.GetEnvironmentVariable("TMP"), "AssemblyBinding.zip"));
            }

            bool result = base.OnStart();

            Trace.TraceInformation("AssemblyBinder has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("AssemblyBinder is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("AssemblyBinder has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
