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
            
            try
            {
                // Diagnostic: Ignore certificate validation errors (common in cloud environments)
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                _logger.LogInformation("SMTP Step 1: Connecting to {Host}:{Port} with {Options}", 
                    _smtpSettings.Host, _smtpSettings.Port, _smtpSettings.EnableSsl ? "SSL/TLS" : "No SSL");

                var socketOptions = _smtpSettings.Port == 465 
                    ? SecureSocketOptions.SslOnConnect 
                    : SecureSocketOptions.StartTls;

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                
                await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, socketOptions, cts.Token);
                _logger.LogInformation("SMTP Step 2: Connected successfully.");

                await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password, cts.Token);
                _logger.LogInformation("SMTP Step 3: Authenticated successfully.");

                await smtp.SendAsync(email, cts.Token);
                _logger.LogInformation("SMTP Step 4: Email sent successfully to {ToEmail}", toEmail);
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("SMTP Error: Operation timed out during email sending to {ToEmail}", toEmail);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP Error: Failed at some step. Message: {Message}", ex.Message);
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
