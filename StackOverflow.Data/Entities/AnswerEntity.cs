using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflow.Data.Entities
{
    public class AnswerEntity : TableEntity
    {
        public string TekstOdgovora { get; set; }
        public string AutorEmail { get; set; }
        public int BrojGlasova { get; set; }
        public bool JeNajboljiOdgovor { get; set; }

        public AnswerEntity(string idPitanja, string autorEmail)
        {
            PartitionKey = idPitanja;
            RowKey = Guid.NewGuid().ToString();
            AutorEmail = autorEmail;
            Timestamp = DateTime.UtcNow;
            BrojGlasova = 0;
            JeNajboljiOdgovor = false;
        }

        public AnswerEntity() { }
    }
}
