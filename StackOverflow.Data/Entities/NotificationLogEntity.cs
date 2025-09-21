using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflow.Data.Entities
{
    public class NotificationLogEntity : TableEntity
    {
        public string IdOdgovora { get; set; }
        public int BrojPoslatihMejlova { get; set; }

        public NotificationLogEntity(string idOdgovora, int brojPoslatihMejlova)
        {
            PartitionKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
            RowKey = DateTime.UtcNow.ToString("HH-mm-ss-fffffff");
            IdOdgovora = idOdgovora;
            BrojPoslatihMejlova = brojPoslatihMejlova;
        }
        public NotificationLogEntity() { }
    }
}
