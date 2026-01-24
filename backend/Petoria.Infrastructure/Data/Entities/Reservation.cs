using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

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

    // Room type selection
    public int? RoomTypeId { get; set; }

    [ForeignKey("RoomTypeId")]
    public RoomType? RoomType { get; set; }

    [Range(1, 100)]
    public int NumberOfRooms { get; set; } = 1;  // How many rooms of this type

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
