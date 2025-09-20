using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace StackOverflow.Data.Entities
{
    public class HealthCheckLogEntity : TableEntity
    {
        public string Status { get; set; }
        public string ServiceName { get; set; }

        public HealthCheckLogEntity(string serviceName, string status)
        {
            // PartitionKey grupiše logove po nazivu servisa
            this.PartitionKey = serviceName;

            // RowKey mora biti jedinstven za svaki PartitionKey,
            // koristimo obrnute otkucaje sata da bi najnoviji logovi bili na vrhu.
            this.RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19");

            this.ServiceName = serviceName;
            this.Status = status;
        }

        public HealthCheckLogEntity() { }
    }

}
