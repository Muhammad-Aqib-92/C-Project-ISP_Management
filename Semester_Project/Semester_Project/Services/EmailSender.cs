using Microsoft.AspNetCore.Identity.UI.Services;

namespace Semester_Project.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("Sending email to {Email} with subject {Subject}", email, subject);
            return Task.CompletedTask;
        }
    }
}
