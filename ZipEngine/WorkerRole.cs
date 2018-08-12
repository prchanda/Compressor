using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Ionic.Zip;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace ZipEngine
{
    public class WorkerRole : RoleEntryPoint
    {
        private string inputContainerName = "inputfiles", outputContainerName = "outputfiles";
        private static CloudBlobContainer inputDocumentsContainer, outputDocumentsContainer;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("ZipEngine is running");

            while (true)
            {
                var inputFiles = inputDocumentsContainer.ListBlobs();
                var inputFileNames = inputFiles.OfType<CloudBlockBlob>().Select(file => file.Name).ToList();
                var outputFiles = outputDocumentsContainer.ListBlobs();
                var outputFileNames = outputFiles.OfType<CloudBlockBlob>().Select(file => file.Name).ToList();

                if (outputFileNames.Count != 0)
                    inputFileNames = inputFileNames
                        .Where(inFile => !outputFileNames.Contains(Path.GetFileNameWithoutExtension(inFile) + ".zip"))
                        .Select(inFile => inFile).ToList();

                foreach (var inputFile in inputFileNames)
                {
                    var fileBlob = inputDocumentsContainer.GetBlockBlobReference(inputFile);

                    fileBlob.DownloadToFile(Path.Combine(Environment.GetEnvironmentVariable("TMP"), inputFile),
                        FileMode.Create);
                    using (var zip = new ZipFile())
                    {
                        zip.AddFile(Path.Combine(Environment.GetEnvironmentVariable("TMP"), inputFile));
                        zip.Save(Path.Combine(Environment.GetEnvironmentVariable("TMP"),
                            Path.GetFileNameWithoutExtension(inputFile) + ".zip"));
                        fileBlob = outputDocumentsContainer.GetBlockBlobReference(
                            Path.GetFileNameWithoutExtension(inputFile) + ".zip");
                        fileBlob.UploadFromFile(Path.Combine(Environment.GetEnvironmentVariable("TMP"),
                            Path.GetFileNameWithoutExtension(inputFile) + ".zip"));
                    }
                }
                return;
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            WorkerAssembly.WorkerAssembly workerAssembly = new WorkerAssembly.WorkerAssembly();
            workerAssembly.DoWork();
            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            var storageAccount =
                CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            // Get context object for working with blobs, and 
            // set a default retry policy appropriate for a web user interface.
            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the blob container.
            inputDocumentsContainer = blobClient.GetContainerReference(inputContainerName);
            outputDocumentsContainer = blobClient.GetContainerReference(outputContainerName);
            

            Trace.TraceInformation("ZipEngine has been started");

            var result = base.OnStart();

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("ZipEngine is stopping");

            cancellationTokenSource.Cancel();
            runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("ZipEngine has stopped");
        }
    }
}