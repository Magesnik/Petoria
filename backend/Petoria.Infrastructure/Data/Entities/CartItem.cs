using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Артикул в количката: хотел, стая, дати, брой стаи, цена.
/// </summary>
public class CartItem
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

    [Required]
    public int RoomTypeId { get; set; }

    [ForeignKey("RoomTypeId")]
    public RoomType? RoomType { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    public int NumberOfRooms { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Първоначална цена преди отстъпки.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal OriginalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
