using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Petoria.Constants;
namespace Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Коментар в хотел: текст, автор, дата, parent за нишки, рейтинги, отговори.
/// </summary>
public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int HotelId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(ValidationConstants.Comment.ContentMaxLength)]
    public string Text { get; set; } = string.Empty;

    // За вложени отговори - null ако е коментар от най-високо ниво
    public int? ParentCommentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Навигационни свойства
    [ForeignKey("HotelId")]
    public Hotel Hotel { get; set; } = null!;

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey("ParentCommentId")]
    public Comment? ParentComment { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    public ICollection<CommentRating> Ratings { get; set; } = new List<CommentRating>();
}
