using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StackOverflow.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflow.Data.Repositories
{
    public class NotificationLogRepository
    {
        private CloudTable table;
        public NotificationLogRepository()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("NotificationLogs");
            table.CreateIfNotExists();
        }

        public void LogNotification(NotificationLogEntity log)
        {
            TableOperation insertOperation = TableOperation.Insert(log);
            table.Execute(insertOperation);
        }
    }
}
