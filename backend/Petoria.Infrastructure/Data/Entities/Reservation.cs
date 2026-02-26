using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Petoria.Constants;
using Petoria.Infrastructure.Data.Entities;

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

    [Range(ValidationConstants.Reservation.RoomsMin, ValidationConstants.Reservation.RoomsMax)]
    public int NumberOfRooms { get; set; } = 1;  // How many rooms of this type

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RefundAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal RetainedAmount { get; set; } = 0;

    [MaxLength(ValidationConstants.Reservation.StatusMaxLength)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

    [MaxLength(ValidationConstants.Reservation.NotesMaxLength)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
