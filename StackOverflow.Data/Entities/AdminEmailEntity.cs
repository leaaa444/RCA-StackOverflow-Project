using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace StackOverflow.Data.Entities
{
    public class AdminEmailEntity : TableEntity
    {
        public string Email { get; set; }

        public AdminEmailEntity(string email)
        {
            PartitionKey = "Admin";
            RowKey = email;
            Email = email;
        }

        public AdminEmailEntity() { }
    }
}
