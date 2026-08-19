using Microsoft.AspNetCore.Identity.UI.Services;

namespace ResearchProjectManager.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // TODO: Implement actual email sending logic (SMTP, SendGrid, etc.)
            _logger.LogInformation($"Email to {email} with subject '{subject}' would be sent here.");
            await Task.CompletedTask;
        }
    }
}
