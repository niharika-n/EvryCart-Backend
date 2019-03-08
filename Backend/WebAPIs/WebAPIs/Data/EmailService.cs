using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using WebAPIs.Models;

namespace WebAPIs.Data
{
    public class EmailService
    {
        private IConfiguration config;

        public EmailService(IConfiguration _config)
        {
            config = _config;
        }        

        public string SendEmail(EmailViewModel emailModel)
        {
            MailUser user = new MailUser();
            using (var message = new MailMessage())
            {
                foreach (var email in emailModel.ToEmailList)
                {
                    user.Name = email.Name;
                    user.Email = email.Email;
                    message.To.Add(new MailAddress(user.Email, "To" + user.Name));
                }
                message.From = new MailAddress("test.demo@app.com", "App Test");
                message.Subject = emailModel.Subject;
                message.Body = emailModel.Body;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient(config["Email:Host"]))
                {
                    client.Host = config["Email:Host"];
                    client.Port = Convert.ToInt32(config["Email:Port"]);
                    client.EnableSsl = Convert.ToBoolean(config["Email:EnableSsl"]);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = Convert.ToBoolean(config["Email:UseDefaultCredentials"]);
                    client.Credentials = new NetworkCredential(config["Email:UserEmail"], config["Email:UserPassword"]);
                    client.Send(message);
                    message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;

                    return message.DeliveryNotificationOptions.ToString();
                }
            }
        }

    }
}
