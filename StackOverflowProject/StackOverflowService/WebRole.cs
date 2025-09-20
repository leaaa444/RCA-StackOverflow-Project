using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics; 
using System.ServiceModel;
using Contracts;

namespace StackOverflowService
{
    public class HealthMonitoringProvider : IHealthMonitoring
    {
        public void IAmAlive()
        {
            // Prazna metoda, njen uspesan poziv je dovoljan dokaz da je servis ziv.
        }
    }

    public class WebRole : RoleEntryPoint
    {
        private ServiceHost serviceHost;
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

            return base.OnStart();
        }
    }
}
