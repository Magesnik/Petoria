using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Petoria.Constants;
namespace Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Ревю за хотел: рейтинг (1-5) и текст.
/// </summary>
public class HotelReview
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int HotelId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Range(ValidationConstants.HotelReview.RatingMin, ValidationConstants.HotelReview.RatingMax)]
    public int Rating { get; set; }

    [MaxLength(ValidationConstants.HotelReview.CommentMaxLength)]
    public string ReviewText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Навигационни свойства
    [ForeignKey("HotelId")]
    public Hotel Hotel { get; set; } = null!;

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;
}
