using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.SupportMessage
{
    public class AnswerSupportMessageDto
    {
        [Required]
        public string Response { get; set; } = string.Empty;
    }
}
