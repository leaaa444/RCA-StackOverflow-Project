using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace StackOverflow.Data.Helpers
{
    public class QueueHelper
    {
        private CloudQueueClient queueClient;

        public QueueHelper()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            queueClient = storageAccount.CreateCloudQueueClient();
        }

        public CloudQueue GetQueueReference(string queueName)
        {
            CloudQueue queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExists();
            return queue;
        }
    }
}
