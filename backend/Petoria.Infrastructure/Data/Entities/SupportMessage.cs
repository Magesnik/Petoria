using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Petoria.Constants;

namespace Petoria.Infrastructure.Data.Entities
{
    /// <summary>
    /// Тикет за поддръжка: тема, съобщение, админ отговор.
    /// </summary>
    public class SupportMessage
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(ValidationConstants.SupportMessage.SubjectMaxLength)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsAnswered { get; set; } = false;

        /// <summary>
        /// Отговор от администратор.
        /// </summary>
        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AnsweredAt { get; set; }

        /// <summary>
        /// Дали потребителят е прочел отговора.
        /// </summary>
        public bool IsReadByUser { get; set; } = false;

        /// <summary>
        /// Дали администраторът е прочел съобщението.
        /// </summary>
        public bool IsReadByAdmin { get; set; } = false;

        // Навигационно свойство
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}
