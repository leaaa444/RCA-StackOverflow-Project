using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflow.Data.Entities
{
    public class VoteEntity : TableEntity
    {
        public VoteEntity(string idOdgovora, string userEmail)
        {
            PartitionKey = idOdgovora;
            RowKey = userEmail;
        }

        public VoteEntity() { }
    }
}
