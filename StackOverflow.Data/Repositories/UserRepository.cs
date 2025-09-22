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

        public Dictionary<string, UserEntity> GetUsersByEmails(List<string> emails)
        {
            if (emails == null || !emails.Any())
            {
                return new Dictionary<string, UserEntity>();
            }

            string finalFilter = "";
            foreach (var email in emails.Distinct())
            {
                string singleFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, email);
                if (string.IsNullOrEmpty(finalFilter))
                    finalFilter = singleFilter;
                else
                    finalFilter = TableQuery.CombineFilters(finalFilter, TableOperators.Or, singleFilter);
            }

            var query = new TableQuery<UserEntity>().Where(finalFilter);
            return table.ExecuteQuery(query).ToDictionary(u => u.Email, u => u);
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

        public bool ChangePassword(string email, string oldPassword, string newPassword)
        {
            UserEntity user = GetUser(email);
            if (user == null) return false;

            string oldHashedPassword = PasswordHelper.HashPassword(oldPassword);
            if (user.Lozinka != oldHashedPassword)
            {
                return false; 
            }

            user.Lozinka = PasswordHelper.HashPassword(newPassword);
            UpdateUser(user);
            return true;
        }
    }
}
