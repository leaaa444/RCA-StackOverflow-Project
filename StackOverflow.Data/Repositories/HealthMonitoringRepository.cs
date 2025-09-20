using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StackOverflow.Data;
using StackOverflow.Data.Entities;

namespace StackOverflow.Data.Repositories
{
    public class HealthMonitoringRepository
    {
        private CloudTable table;

        public HealthMonitoringRepository()
        {
            // Preuzimanje connection stringa iz konfiguracije
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));

            // Kreiranje klijenta za tabele
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Dobijanje reference na tabelu 'HealthCheck'
            table = tableClient.GetTableReference("HealthCheck");

            // Kreiranje tabele ako ne postoji
            table.CreateIfNotExists();
        }

        public void LogHealthStatus(HealthCheckLogEntity logEntry)
        {
            // Operacija za unos novog reda u tabelu
            TableOperation insertOperation = TableOperation.Insert(logEntry);

            // Izvrsi operaciju
            table.Execute(insertOperation);
        }
    }
}
