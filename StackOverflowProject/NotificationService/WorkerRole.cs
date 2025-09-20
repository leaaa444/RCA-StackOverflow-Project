using Contracts;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using StackOverflow.Data;
using Microsoft.WindowsAzure.Storage.Queue;
using StackOverflow.Data.Helpers;
using StackOverflow.Data.Repositories;

namespace NotificationService
{
    public class HealthMonitoringProvider : IHealthMonitoring
    {
        public void IAmAlive()
        {
            // Prazna metoda, njen uspesan poziv je dovoljan dokaz da je servis ziv.
        }
    }
    public class WorkerRole : RoleEntryPoint
    {
        private ServiceHost serviceHost;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private QueueHelper queueHelper = new QueueHelper();
        private AdminAlertRepository adminRepo = new AdminAlertRepository();

        public override void Run()
        {
            Trace.TraceInformation("NotificationService is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            var endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["HealthCheckEndpoint"];
            var endpointAddress = $"net.tcp://{endpoint.IPEndpoint}/HealthCheckEndpoint";

            serviceHost = new ServiceHost(typeof(HealthMonitoringProvider));
            var binding = new NetTcpBinding();
            serviceHost.AddServiceEndpoint(typeof(IHealthMonitoring), binding, endpointAddress);
            serviceHost.Open();

            Trace.TraceInformation("[WCF] HealthCheckEndpoint otvoren na adresi: " + endpointAddress);

            adminRepo.AddTestAdminEmail("admin@test.com");
            Trace.TraceInformation("[INFO] Test admin email dodat u bazu.");

            return base.OnStart();
        }

        public override void OnStop()
        {
            Trace.TraceInformation("NotificationService is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("NotificationService has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var alertsQueue = queueHelper.GetQueueReference("alerts");
            Trace.TraceInformation("[NotificationService] Osluskujem 'alerts' red...");

            while (!cancellationToken.IsCancellationRequested)
            {
                CloudQueueMessage message = alertsQueue.GetMessage(TimeSpan.FromMinutes(1));

                if (message != null)
                {
                    try
                    {
                        string alertContent = message.AsString;
                        Trace.TraceInformation("[ALERT] Primljena poruka o gresci: " + alertContent);

                        var adminEmails = adminRepo.GetAllAdminEmails();
                        foreach (var admin in adminEmails)
                        {
                            // Simulacija slanja emaila
                            Trace.TraceInformation($"[EMAIL] Saljem alert na: {admin.Email} sa porukom: {alertContent}");
                        }

                        alertsQueue.DeleteMessage(message); // Brisanje poruke nakon obrade
                        Trace.TraceInformation($"[NotificationService] Poruka uspesno obradjena i izbrisana.");
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"[NotificationService] Greska pri obradi poruke: {ex.Message}");
                        // U slucaju greske, poruka NECE biti izbrisana i vratice se u red nakon 5 minuta
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}
