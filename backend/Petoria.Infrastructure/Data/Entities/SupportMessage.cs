using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Petoria.Infrastructure.Data.Entities
{
    public class SupportMessage
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsAnswered { get; set; } = false;

        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AnsweredAt { get; set; }

        public bool IsReadByUser { get; set; } = false;
        
        public bool IsReadByAdmin { get; set; } = false;

        // Navigation property
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}
