using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StackOverflow.Data.Entities;
using StackOverflow.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflow.Data.Repositories
{
    public class UserRepository
    {
        private CloudTable table;
        public UserRepository()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("Users");
            table.CreateIfNotExists();
        }

        public void RegisterUser(UserEntity user)
        {
            user.Lozinka = PasswordHelper.HashPassword(user.Lozinka);
            TableOperation insertOperation = TableOperation.Insert(user);
            table.Execute(insertOperation);
        }

        public UserEntity GetUser(string email)
        {
            var query = new TableQuery<UserEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "User"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, email)
                )
            );
            return table.ExecuteQuery(query).FirstOrDefault();
        }

        public bool LoginUser(string email, string password)
        {
            UserEntity user = GetUser(email);

            if (user == null) return false;

            string hashedPassword = PasswordHelper.HashPassword(password);

            if (user.Lozinka == hashedPassword) return true;

            return false;
        }

        public void UpdateUser(UserEntity user)
        {
            TableOperation updateOperation = TableOperation.Replace(user);
            table.Execute(updateOperation);
        }
    }
}
