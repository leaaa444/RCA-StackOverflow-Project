using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using StackOverflow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace StackOverflowService
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public void InitBlobs()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
                CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();

                // Kontejner za slike pitanja 
                CloudBlobContainer container = blobStorage.GetContainerReference("questionimages");
                container.CreateIfNotExists();
                var permissions = container.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(permissions);

                // Kontejner za slike profila
                CloudBlobContainer pContainer = blobStorage.GetContainerReference("profileimages");
                pContainer.CreateIfNotExists();
                var pPermissions = pContainer.GetPermissions();
                pPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
                pContainer.SetPermissions(pPermissions);
            }
            catch (WebException) { }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            InitBlobs();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DataSeeder.SeedInitialData();
        }
    }
}
