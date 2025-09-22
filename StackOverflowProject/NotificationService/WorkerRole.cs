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
        private AdminAlertRepository adminRepo = new AdminAlertRepository(); private AnswerRepository answerRepo = new AnswerRepository();
        private NotificationLogRepository logRepo = new NotificationLogRepository();
        private EmailSender emailSender = new EmailSender();
        private QuestionRepository questionRepo = new QuestionRepository();

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

            //adminRepo.AddTestAdminEmail("leasusic2002@gmail.com");
            //Trace.TraceInformation("[INFO] Test admin email dodat u bazu.");

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
            var acceptedAnswersQueue = queueHelper.GetQueueReference("acceptedanswersqueue");
            
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
                            await emailSender.SendEmailAsync(admin.Email, "Upozorenje: Servis je nedostupan!", $"<p>{alertContent}</p>");
                        }

                        alertsQueue.DeleteMessage(message);
                        Trace.TraceInformation($"[NotificationService] Poruka uspesno obradjena i izbrisana.");
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"[NotificationService] Greska pri obradi poruke: {ex.Message}");
                    }
                }

                CloudQueueMessage answerMessage = acceptedAnswersQueue.GetMessage();
                if (answerMessage != null)
                {
                    try
                    {
                        string idOdgovora = answerMessage.AsString;
                        Trace.TraceInformation($"[Answers] Primljena poruka za najbolji odgovor, ID: {idOdgovora}");

                        var answer = answerRepo.GetAnswerById(idOdgovora);
                        if (answer != null)
                        {
                            var question = questionRepo.GetQuestion(answer.PartitionKey);
                            if (question == null)
                            {
                                Trace.TraceError($"[Answers] Nije pronadjeno pitanje za odgovor ID: {idOdgovora}");
                                acceptedAnswersQueue.DeleteMessage(answerMessage);
                                continue;
                            }
                            var allAnswersToQuestion = answerRepo.GetAnswersForQuestion(answer.PartitionKey);
                            var ucesnici = allAnswersToQuestion.Select(a => a.AutorEmail).Distinct().ToList();

                            foreach (var email in ucesnici)
                            {
                                string subject = $"Odgovoreno na pitanje: '{question.Naslov}'";
                                string body = $@"
                                    <h1>Tema je zatvorena</h1>
                                    <p>Pitanje '{question.Naslov}' na koje ste odgovorili je zatvoreno.</p>
                                    <p>Autor pitanja je označio sledeći odgovor kao najbolji:</p>
                                    <hr>
                                    <p><strong>Autor odgovora:</strong> {answer.AutorEmail}</p>
                                    <blockquote>{answer.TekstOdgovora}</blockquote>
                                    <hr>";

                                await emailSender.SendEmailAsync(email, subject, body);
                            }

                            logRepo.LogNotification(new NotificationLogEntity(idOdgovora, ucesnici.Count));
                            Trace.TraceInformation($"[Answers] Notifikacija uspesno logirana za {ucesnici.Count} ucesnika.");
                        }

                        acceptedAnswersQueue.DeleteMessage(answerMessage);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"[Answers] Greska pri obradi poruke za odgovor: {ex.Message}");
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}
