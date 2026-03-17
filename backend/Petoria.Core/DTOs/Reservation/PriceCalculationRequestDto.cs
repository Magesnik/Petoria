using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Reservation;

/// <summary>Заявка: изчисление на цена без реална резервация</summary>
public class PriceCalculationRequestDto
{
    [Required(ErrorMessage = "RoomTypeId е задължителен")]
    public int RoomTypeId { get; set; }

    [Required(ErrorMessage = "Датата на настаняване е задължителна")]
    public DateTime CheckInDate { get; set; }

    [Required(ErrorMessage = "Датата на напускане е задължителна")]
    public DateTime CheckOutDate { get; set; }

    [Range(ValidationConstants.Reservation.RoomsMin, ValidationConstants.Reservation.RoomsMax, ErrorMessage = "Броят стаи трябва да бъде между 1 и 100")]
    public int NumberOfRooms { get; set; } = 1;
}
