using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflow.Data.Entities
{
    public class QuestionEntity : TableEntity
    {
        public string Naslov { get; set; }
        public string OpisProblema { get; set; }
        public string SlikaGreskeUrl { get; set; }
        public string AutorEmail { get; set; } // Da znamo ko je postavio pitanje

        public QuestionEntity(string autorEmail)
        {
            PartitionKey = "Question"; // Sva pitanja su u istoj particiji
            RowKey = Guid.NewGuid().ToString(); // Jedinstveni ID za svako pitanje
            AutorEmail = autorEmail;
            Timestamp = DateTime.UtcNow;
        }

        public QuestionEntity() { }
    }
}
