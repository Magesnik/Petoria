using System.ComponentModel.DataAnnotations;
using Petoria.Constants;
namespace Petoria.Core.DTOs.SupportMessage
{
    public class CreateSupportMessageDto
    {
        [Required]
        [MaxLength(ValidationConstants.SupportMessage.SubjectMaxLength)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MinLength(ValidationConstants.SupportMessage.MessageMinLength)]
        public string Message { get; set; } = string.Empty;
    }
}
