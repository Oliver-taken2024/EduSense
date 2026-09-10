using Microsoft.Extensions.Logging;

namespace EduSense.BLL.Services
{
    public class NullEmailSender : IEmailSender
    {
        private readonly ILogger<NullEmailSender> _logger;

        public NullEmailSender(ILogger<NullEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            _logger.LogInformation("E-post (ej skickad, NullEmailSender) till {Email}: {Subject}\n{Body}", toEmail, subject, htmlBody);
            return Task.CompletedTask;
        }
    }
}