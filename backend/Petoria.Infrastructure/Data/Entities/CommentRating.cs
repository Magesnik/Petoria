using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Рейтинг на коментар (харесване/нехаресване).
/// </summary>
public class CommentRating
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CommentId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public bool IsLike { get; set; } // true = харесване, false = нехаресване

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигационни свойства
    [ForeignKey("CommentId")]
    public Comment Comment { get; set; } = null!;

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;
}
