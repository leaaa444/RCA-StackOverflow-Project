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
    public class AnswerRepository
    {
        private CloudTable table;
        public AnswerRepository()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("Answers");
            table.CreateIfNotExists();
        }

        public void AddAnswer(AnswerEntity answer)
        {
            TableOperation insertOperation = TableOperation.Insert(answer);
            table.Execute(insertOperation);
        }

        public List<AnswerEntity> GetAnswersForQuestion(string idPitanja)
        {
            var query = new TableQuery<AnswerEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, idPitanja)
            );
            return table.ExecuteQuery(query).OrderByDescending(a => a.Timestamp).ToList();
        }

        public AnswerEntity GetAnswer(string idPitanja, string idOdgovora)
        {
            var query = new TableQuery<AnswerEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, idPitanja),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, idOdgovora)
                )
            );
            return table.ExecuteQuery(query).FirstOrDefault();
        }

        public void UpdateAnswer(AnswerEntity answer)
        {
            TableOperation updateOperation = TableOperation.Replace(answer);
            table.Execute(updateOperation);
        }

        public void BatchUpdateAnswers(List<AnswerEntity> answers)
        {
            if (!answers.Any()) return;

            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (var answer in answers)
            {
                batchOperation.Replace(answer);
            }
            table.ExecuteBatch(batchOperation);
        }

        public void DeleteAnswersForQuestion(List<AnswerEntity> answers)
        {
            if (!answers.Any()) return;

            TableBatchOperation batchOperation = new TableBatchOperation();
            foreach (var answer in answers)
            {
                batchOperation.Delete(answer);
            }
            table.ExecuteBatch(batchOperation);
        }



    }
}
