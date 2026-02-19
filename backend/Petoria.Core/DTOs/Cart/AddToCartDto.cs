using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.Cart;

public class AddToCartDto
{
    [Required]
    public int HotelId { get; set; }

    [Required]
    public int RoomTypeId { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Range(1, 20)]
    public int NumberOfRooms { get; set; } = 1;

    public decimal TotalPrice { get; set; }
    public decimal OriginalPrice { get; set; }
}
