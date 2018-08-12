using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace FileUploader.Controllers
{
    public class HomeController : Controller
    {
        private static CloudBlobContainer inputDocumentsContainer;
        private string inputContainerName = "inputfiles";

        public HomeController()
        {
            var storageAccount =
                CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            // Get context object for working with blobs, and 
            // set a default retry policy appropriate for a web user interface.
            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the blob container.
            inputDocumentsContainer = blobClient.GetContainerReference(inputContainerName);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    await UploadAndSaveBlobAsync(file);
                    ViewBag.Message = "File Uploaded Successfully!!";
                }

                return View();
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }

        private async Task<CloudBlockBlob> UploadAndSaveBlobAsync(HttpPostedFileBase file)
        {
            var blobName = Path.GetFileName(file.FileName);
            var fileBlob = inputDocumentsContainer.GetBlockBlobReference(blobName);
            using (var fileStream = file.InputStream)
            {
                await fileBlob.UploadFromStreamAsync(fileStream);
            }

            return fileBlob;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            //string instanceId = RoleEnvironment.CurrentRoleInstance.Id;
            //ViewBag.Message = instanceId.Substring(instanceId.LastIndexOf("_") + 1);            

            return View();
        }
    }
}