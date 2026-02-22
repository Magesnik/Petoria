using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

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

    [Range(ValidationConstants.RoomType.CapacityMin, ValidationConstants.RoomType.CapacityMax)]
    public int NumberOfRooms { get; set; } = 1;

    public decimal TotalPrice { get; set; }
    public decimal OriginalPrice { get; set; }
}
