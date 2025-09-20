using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StackOverflow.Data.Entities;

namespace StackOverflow.Data.Repositories
{
    public class AdminAlertRepository
    {
        private CloudTable table;

        public AdminAlertRepository()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("AlertEmails"); // Naziv tabele kao u projektnom zadatku
            table.CreateIfNotExists();
        }

        public List<AdminEmailEntity> GetAllAdminEmails()
        {
            var query = new TableQuery<AdminEmailEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Admin"));
            return table.ExecuteQuery(query).ToList();
        }

        // Pomocna metoda 
        public void AddTestAdminEmail(string email)
        {
            var admin = new AdminEmailEntity(email);
            TableOperation insertOperation = TableOperation.InsertOrReplace(admin);
            table.Execute(insertOperation);
        }
    }
}
