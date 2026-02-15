using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.Reservation;

/// <summary>
/// DTO за заявка за изчисляване на цена. 
/// Не създава резервация, а само калкулира крайната цена с отстъпки.
/// </summary>
public class PriceCalculationRequestDto
{
    [Required(ErrorMessage = "RoomTypeId е задължителен")]
    public int RoomTypeId { get; set; }

    [Required(ErrorMessage = "Датата на настаняване е задължителна")]
    public DateTime CheckInDate { get; set; }

    [Required(ErrorMessage = "Датата на напускане е задължителна")]
    public DateTime CheckOutDate { get; set; }

    [Range(1, 100, ErrorMessage = "Броят стаи трябва да бъде между 1 и 100")]
    public int NumberOfRooms { get; set; } = 1;
}
