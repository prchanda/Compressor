using System;
using LoadGenerator;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace FileUploader
{
    public class WebRole : RoleEntryPoint
    {
        LoadManager manager;
        int processorCount = Environment.ProcessorCount;
        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            manager = new LoadManager();
            if (Convert.ToInt32(RoleEnvironment.CurrentRoleInstance.Id.Substring(RoleEnvironment.CurrentRoleInstance.Id.LastIndexOf("_") + 1))%2!=0)
            {                
                for (int i = 0; i < processorCount; i++)
                    manager.SetLoad(i, 100);
            }
            else
            {
                for (int i = 0; i < processorCount; i++)
                    manager.SetLoad(i, 0);
            }

            return base.OnStart();
        }
    }
}