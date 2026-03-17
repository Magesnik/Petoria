namespace Petoria.Core.Contracts
{
    /// <summary>
    /// Интерфейс за изпращане на имейли чрез SMTP.
    /// Използва се за потвърждение на регистрация, резервации и анулирания.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>Изпраща HTML имейл до посочен адрес</summary>
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
