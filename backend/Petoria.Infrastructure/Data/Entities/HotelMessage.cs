using System.ComponentModel.DataAnnotations;

using Petoria.Constants;
namespace Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Съобщение до хотел от гост: тема, текст, отговор, прочетено ли е.
/// </summary>
public class HotelMessage
{
    public int Id { get; set; }

    [Required]
    public int HotelId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(ValidationConstants.HotelMessage.SubjectMaxLength)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public bool IsAnswered { get; set; } = false;

    /// <summary>
    /// Отговор от администратор на хотела.
    /// </summary>
    public string? AdminResponse { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? AnsweredAt { get; set; }

    /// <summary>
    /// Дали потребителят е прочел отговора.
    /// </summary>
    public bool IsReadByUser { get; set; } = false;

    // Навигационни свойства
    public Hotel Hotel { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
