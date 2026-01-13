using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int HotelId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    // For nested replies - null if top-level comment
    public int? ParentCommentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("HotelId")]
    public Hotel Hotel { get; set; } = null!;

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey("ParentCommentId")]
    public Comment? ParentComment { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    public ICollection<CommentRating> Ratings { get; set; } = new List<CommentRating>();
}
