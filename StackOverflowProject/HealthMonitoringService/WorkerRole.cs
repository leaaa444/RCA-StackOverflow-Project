using Contracts;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using StackOverflow.Data;
using StackOverflow.Data.Entities;
using StackOverflow.Data.Helpers;
using StackOverflow.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoringService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private bool isStopped = false;
        private HealthMonitoringRepository repo = new HealthMonitoringRepository();
        private QueueHelper queueHelper = new QueueHelper();

        public override void Run()
        {
            Trace.TraceInformation("HealthMonitoringService is running");
            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.isStopped = true;
            }
        }

        public override bool OnStart()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            // Use TLS 1.2 for Service Bus connections
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("HealthMonitoringService has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("HealthMonitoringService is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("HealthMonitoringService has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var webRoleName = "StackOverflowService";
                    var workerRoleName = "NotificationService";

                    var webRoleInstances = RoleEnvironment.Roles[webRoleName].Instances;
                    var workerRoleInstances = RoleEnvironment.Roles[workerRoleName].Instances;

                    var tasks = new List<Task>();

                    foreach (var instance in webRoleInstances)
                    {
                        tasks.Add(CheckServiceHealth(instance, webRoleName));
                    }

                    foreach (var instance in workerRoleInstances)
                    {
                        tasks.Add(CheckServiceHealth(instance, workerRoleName));
                    }

                    await Task.WhenAll(tasks);

                    Trace.TraceInformation("[HealthCheck] Ciklus provere zavrsen.");
                }
                catch (Exception ex)
                {
                    Trace.TraceError("[HealthCheck] Greska u glavnoj petlji: " + ex.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken);
            }
        }

        private Task CheckServiceHealth(RoleInstance instance, string roleName)
        {
            return Task.Run(() =>
            {
                string status = "OK";
                try
                {
                    var endpoint = instance.InstanceEndpoints["HealthCheckEndpoint"];
                    var endpointAddress = $"net.tcp://{endpoint.IPEndpoint}/HealthCheckEndpoint";

                    var factory = new ChannelFactory<IHealthMonitoring>(new NetTcpBinding(), new EndpointAddress(endpointAddress));
                    var proxy = factory.CreateChannel();

                    proxy.IAmAlive();

                    ((IClientChannel)proxy).Close();
                }
                catch (Exception ex)
                {
                    status = "NOT_OK";
                    Trace.TraceError($"[HealthCheck] Greska pri proveri instance {instance.Id}: {ex.Message}");
                    var queue = queueHelper.GetQueueReference("alerts");
                    string message = $"Service '{roleName}' instance '{instance.Id}' is down.";
                    queue.AddMessage(new CloudQueueMessage(message));
                    Trace.TraceInformation($"[HealthCheck] Poslata poruka u 'alerts' red.");

                }

                Trace.TraceInformation($"[HealthCheck] Provera statusa: Rola={roleName}, Instanca={instance.Id}, Status={status}");
                var logEntry = new HealthCheckLogEntity(roleName, status);
                repo.LogHealthStatus(logEntry);
            });
        }
    }
}
