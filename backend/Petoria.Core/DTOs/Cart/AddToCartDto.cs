using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Cart;

/// <summary>Заявка: добавяне на артикул в количката</summary>
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
}
