using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EduSense.BLL.Services
{
  
    // Skickar riktiga mail via SMTP (Mailtrap under utveckling,
    // eller en riktig SMTP-server om lösningen någon gång behöver köras live).


    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new BodyBuilder
            {
                HtmlBody = htmlBody
            }.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                var socketOptions = _settings.UseSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

                await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions);

                if (!string.IsNullOrWhiteSpace(_settings.Username))
                {
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);
                }

                await client.SendAsync(message);
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }

            _logger.LogInformation("E-post skickad till {Email}: {Subject}", toEmail, subject);
        }
    }
}