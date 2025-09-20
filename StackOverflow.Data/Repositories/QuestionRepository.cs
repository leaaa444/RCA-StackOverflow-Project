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
    public class QuestionRepository
    {
        private CloudTable table;
        public QuestionRepository()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("Questions");
            table.CreateIfNotExists();
        }

        public void AddQuestion(QuestionEntity question)
        {
            TableOperation insertOperation = TableOperation.Insert(question);
            table.Execute(insertOperation);
        }

        public List<QuestionEntity> GetAllQuestions()
        {
            var query = new TableQuery<QuestionEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Question")
            );
            return table.ExecuteQuery(query).OrderByDescending(q => q.Timestamp).ToList();
        }

        public QuestionEntity GetQuestion(string id)
        {
            var query = new TableQuery<QuestionEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Question"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)
                )
            );
            return table.ExecuteQuery(query).FirstOrDefault();
        }
    }
}
