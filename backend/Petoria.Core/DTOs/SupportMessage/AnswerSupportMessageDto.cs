using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.SupportMessage
{
    /// <summary>Заявка: админ отговор на тикет за поддръжка</summary>
    public class AnswerSupportMessageDto
    {
        [Required]
        public string Response { get; set; } = string.Empty;
    }
}
