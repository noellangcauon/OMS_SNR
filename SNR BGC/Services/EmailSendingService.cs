using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SNR_BGC.DataAccess;
using SNR_BGC.Interface;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace SNR_BGC.Services
{
    public class EmailSendingService: IEmailSendingService
    {
        private readonly IConfiguration _configuration;

        public EmailSendingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmailForTempPassword(string toAddress, string toDisplayName, string subject, string userName, string tempPassword)
        {
            try
            {
                WriteToFile.Log($"[{DateTime.Now}]: Sending...");

                var templatePath = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "TemporaryPassword.html");
                string bodyTemplate = File.ReadAllText(templatePath);

                string safePassword = WebUtility.HtmlEncode(tempPassword);
                string body = bodyTemplate
                    .Replace("{{FullName}}", toDisplayName)
                    .Replace("{{TempPassword}}", safePassword);

                var fromAddress = new MailAddress(_configuration["Email:FromAddress"], _configuration["Email:FromDisplayName"]);
                var toEmailAddress = new MailAddress(toAddress, toDisplayName);
                //var ccAddress = new MailAddress(
                //    _configuration["Email:CcAddress"],
                //    _configuration["Email:CcDisplayName"]
                //);

                var smtp = new SmtpClient
                {
                    Host = "smtp.office365.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, _configuration["Email:Password"])
                };

                using (var message = new MailMessage(fromAddress, toEmailAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // ✅ add this
                })
                {
                    // Add CC
                    //message.CC.Add(ccAddress);

                    smtp.Send(message);
                    WriteToFile.Log($"[{DateTime.Now}]: Message Sent to {toAddress}.");
                }
            }
            catch (Exception ex)
            {
                WriteToFile.Log($"[{DateTime.Now}]: {ex.Message}");
            }
        }
    }
}
