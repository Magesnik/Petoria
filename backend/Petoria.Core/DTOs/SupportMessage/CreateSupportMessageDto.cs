using System.ComponentModel.DataAnnotations;
using Petoria.Constants;
namespace Petoria.Core.DTOs.SupportMessage
{
    public class CreateSupportMessageDto
    {
        [Required(ErrorMessage = ValidationConstants.SupportMessage.SubjectRequired)]
        [MaxLength(ValidationConstants.SupportMessage.SubjectMaxLength, ErrorMessage = ValidationConstants.SupportMessage.SubjectMaxLengthError)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationConstants.SupportMessage.MessageRequired)]
        [MinLength(ValidationConstants.SupportMessage.MessageMinLength, ErrorMessage = ValidationConstants.SupportMessage.MessageMinLengthError)]
        public string Message { get; set; } = string.Empty;
    }
}
