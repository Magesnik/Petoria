using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Petoria.Core.Contracts;
using Petoria.Core.Models.Email;

namespace Petoria.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var apiKey = _smtpSettings.ApiKey;
                if (string.IsNullOrEmpty(apiKey))
                {
                    // Fallback to Password if ApiKey is not set explicitly (user might put it in Password field)
                    apiKey = _smtpSettings.Password;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlBody);

                _logger.LogInformation("Attempting to send email via SendGrid API to {ToEmail}", toEmail);
                
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {ToEmail} via SendGrid.", toEmail);
                }
                else
                {
                    var errorContent = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send email via SendGrid. Status: {Status}, Error: {Error}", response.StatusCode, errorContent);
                    throw new Exception($"SendGrid Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} using SendGrid API.", toEmail);
                throw;
            }
        }
    }
}
