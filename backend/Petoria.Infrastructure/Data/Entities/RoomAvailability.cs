using System.ComponentModel.DataAnnotations;
using Petoria.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Наличност на стая за конкретна дата: брой свободни, блокирана ли е.
/// </summary>
public class RoomAvailability
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RoomTypeId { get; set; }

    [ForeignKey("RoomTypeId")]
    public RoomType? RoomType { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Required]
    [Range(ValidationConstants.Availability.AvailableCountMin, ValidationConstants.Availability.AvailableCountMax)]
    public int AvailableCount { get; set; }  // Колко стаи са свободни на тази дата

    public bool IsBlocked { get; set; } = false;  // Админът може да блокира дати

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
