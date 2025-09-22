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
            this.PartitionKey = serviceName;

            this.RowKey = string.Format("{0:D19}-{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid());

            this.ServiceName = serviceName;
            this.Status = status;
        }

        public HealthCheckLogEntity() { }
    }

}
