using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
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
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            // Increase timeout for cloud environments
            smtp.Timeout = 15000; 

            try
            {
                _logger.LogInformation("Attempting to send email to {ToEmail} via {Host}:{Port} (SSL: {EnableSsl})", 
                    toEmail, _smtpSettings.Host, _smtpSettings.Port, _smtpSettings.EnableSsl);

                // Use SecureSocketOptions.Auto for better compatibility with different providers/ports
                var socketOptions = _smtpSettings.EnableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None;
                
                await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, socketOptions);
                await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                await smtp.SendAsync(email);
                
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}. Error: {Message}", toEmail, ex.Message);
                throw;
            }
            finally
            {
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(true);
                }
            }
        }
    }
}
