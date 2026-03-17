using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Petoria.Constants;
using Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Резервация: потребител, хотел, тип стая, дати, брой стаи, цена, статус, възстановена сума.
/// </summary>
public class Reservation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    [Required]
    public int HotelId { get; set; }

    [ForeignKey("HotelId")]
    public Hotel? Hotel { get; set; }

    // Избор на тип стая
    public int? RoomTypeId { get; set; }

    [ForeignKey("RoomTypeId")]
    public RoomType? RoomType { get; set; }

    [Range(ValidationConstants.Reservation.RoomsMin, ValidationConstants.Reservation.RoomsMax)]
    public int NumberOfRooms { get; set; } = 1;  // Колко стаи от този тип

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Възстановена сума при анулиране.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal RefundAmount { get; set; } = 0;

    /// <summary>
    /// Задържана сума при анулиране.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal RetainedAmount { get; set; } = 0;

    [MaxLength(ValidationConstants.Reservation.StatusMaxLength)]
    public string Status { get; set; } = "Pending"; // Чакаща, Потвърдена, Отказана, Завършена

    [MaxLength(ValidationConstants.Reservation.NotesMaxLength)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
