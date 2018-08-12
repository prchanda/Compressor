# Compressor
This project contains a cloud service solution consisting of 1 WebRole and 2 WorkerRoles, required to setup lab for [Cloud Service troubleshooting series](https://blogs.msdn.microsoft.com/pratyay/2018/07/30/cloud-service-troubleshooting-series/).

As the project name goes, the main functionality of this application is to compress an input file and place the output zip file in a storage blob container. Default name of the input and output blob containers are **inputfiles** and **outputfiles**, but you can modify them in the application code as per your wish. Compression operation is done using an open source CodePlex project called ['DotNetZip'](https://archive.codeplex.com/?p=dotnetzip).

Please follow the below steps to setup the cloud service solution:

1.	Install Git client for windows. You can download the setup file from here : https://git-scm.com/download/win. Git glone the cloud service solution using the command : **git clone https://github.com/prchanda/compressor.git**.

    **Note:** *This repository contains LFS objects and it's not currently possible to include them in ZIP downloads due to the way they                are generated. Please make sure to clone this git repository so that LFS objects are included in the download files.*

2.	Create a classic storage account from Azure Portal. Refer [this](https://docs.microsoft.com/en-us/azure/storage/common/storage-create-storage-account#create-a-storage-account) article for guidance.

3.  Open up the solution in Visual Studio and [configure the solution to use your Azure storage account when it runs in Azure](https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-dotnet-get-started#configure-the-solution-to-use-your-azure-storage-account-when-it-runs-in-azure) for *FileUploader* and *ZipEngine* role. To keep things simple set the storage account connection string setting name as **StorageConnectionString**, otherwise to update the references of the storage connection string in code.

4. *(Optional)* If you want to change the input/output blob container names, then please modify the below lines of code:

    - **private string inputContainerName = "inputfiles";**     /// Line #16 of HomeController.cs under FileUploader project
    - **private string inputContainerName = "inputfiles", outputContainerName = "outputfiles";**      /// Line #17 of WorkerRole.cs under ZipEngine.cs

5.  Publish the solution to Azure using the Visual Studio Publish Azure Application Wizard. You can refer [this](https://docs.microsoft.com/en-us/azure/vs-azure-tools-publish-azure-application-wizard) article if you are not aware as how to publish your cloud service solution to Azure. You dont have to wait for the whole deployment to get complete, you can proceed to the next step as soon as the cloud service roles are created.

6.  Now you should see a cloud service created under your azure subscription. Navigate to your cloud service resource and configure an autoscale rule based on CPU metric on *FileUploader* role. You can follow [this](https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-how-to-scale-portal) article for guidance, set the autoscale criteria and action as per the below screenshots:

![CPU Metric Criteria](https://github.com/prchanda/compressor/blob/master/Images/CPU%20Metric%20Criteria.PNG) ![Autoscale Operation](https://github.com/prchanda/compressor/blob/master/Images/Autoscale%20Action.PNG) ![Instance Limits](https://github.com/prchanda/compressor/blob/master/Images/Instance%20Limits.PNG)
