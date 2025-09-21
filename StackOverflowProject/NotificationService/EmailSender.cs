using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService
{
    public class EmailSender
    {
        private string username;
        private string password;

        public EmailSender()
        {
            username = CloudConfigurationManager.GetSetting("EmailUsername");
            password = CloudConfigurationManager.GetSetting("EmailPassword");
        }

        public Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password)
            };

            var mailMessage = new MailMessage(from: username, to: toEmail)
            {
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };

            return client.SendMailAsync(mailMessage);
        }
    }
}
