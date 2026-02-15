using System.ComponentModel.DataAnnotations;

namespace Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Съобщения от потребители към администратори на хотели.
/// Използва се за въпроси и поддръжка.
/// </summary>
public class HotelMessage
{
    public int Id { get; set; }

    [Required]
    public int HotelId { get; set; }

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

    // Navigation properties
    public Hotel Hotel { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
