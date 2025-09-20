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
    public class VoteRepository
    {
        private CloudTable table;
        public VoteRepository()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("Votes");
            table.CreateIfNotExists();
        }

        public void AddVote(VoteEntity vote)
        {
            TableOperation insertOperation = TableOperation.Insert(vote);
            table.Execute(insertOperation);
        }

        public bool HasVoted(string idOdgovora, string userEmail)
        {
            var query = new TableQuery<VoteEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, idOdgovora),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, userEmail)
                )
            );
            return table.ExecuteQuery(query).FirstOrDefault() != null;
        }

        public List<VoteEntity> GetVotesForQuestionByUser(List<string> answerIds, string userEmail)
        {
            string finalFilter = "";
            foreach (var answerId in answerIds)
            {
                string singleFilter = TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, answerId),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, userEmail)
                );

                if (string.IsNullOrEmpty(finalFilter))
                    finalFilter = singleFilter;
                else
                    finalFilter = TableQuery.CombineFilters(finalFilter, TableOperators.Or, singleFilter);
            }

            if (string.IsNullOrEmpty(finalFilter))
                return new List<VoteEntity>();

            var query = new TableQuery<VoteEntity>().Where(finalFilter);
            return table.ExecuteQuery(query).ToList();
        }

        public void DeleteVotesForAnswers(List<string> answerIds)
        {
            if (!answerIds.Any()) return;

            var allVotesToDelete = new List<VoteEntity>();
            foreach (var answerId in answerIds)
            {
                var query = new TableQuery<VoteEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, answerId)
                );
                allVotesToDelete.AddRange(table.ExecuteQuery(query));
            }

            if (allVotesToDelete.Any())
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (var vote in allVotesToDelete)
                {
                    batchOperation.Delete(vote);
                    if (batchOperation.Count == 100)
                    {
                        table.ExecuteBatch(batchOperation);
                        batchOperation = new TableBatchOperation();
                    }
                }

                if (batchOperation.Count > 0)
                {
                    table.ExecuteBatch(batchOperation);
                }
            }
        }





    }
}
