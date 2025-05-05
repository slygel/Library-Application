using LibraryAPI.IServices;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using LibraryAPI.Helpers;

namespace LibraryAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettingsOption _emailSettingsOption;
        public EmailService(IOptions<EmailSettingsOption> emailSettingsOption)
        {
            _emailSettingsOption = emailSettingsOption.Value;
        }

        public Task SendMailAsync(string toEmail, string subject, string body, bool isBodyHtml = true, CancellationToken cancellationToken = default)
        {
            var mailServer = _emailSettingsOption.MailServer;
            var fromEmail = _emailSettingsOption.FromEmail;
            var password = _emailSettingsOption.Password;
            var senderName = _emailSettingsOption.SenderName;
            int port = _emailSettingsOption.MailPort;
            var client = new SmtpClient(mailServer, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
            };

            MailAddress fromAddress = new MailAddress(fromEmail, senderName);
            MailMessage mailMessage = new MailMessage
            {
                From = fromAddress,
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            };

            mailMessage.To.Add(toEmail);

            return client.SendMailAsync(mailMessage, cancellationToken);
        }
    }
}
