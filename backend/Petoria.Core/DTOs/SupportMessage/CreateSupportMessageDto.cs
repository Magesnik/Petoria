using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.SupportMessage
{
    public class CreateSupportMessageDto
    {
        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        public string Message { get; set; } = string.Empty;
    }
}
