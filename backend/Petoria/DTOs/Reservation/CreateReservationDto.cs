using System.ComponentModel.DataAnnotations;

namespace Petoria.DTOs.Reservation;

/// <summary>
/// DTO за създаване на резервация.
/// UserId се взима от JWT токена, не от клиента.
/// НЕ съдържа: Id, UserId, TotalPrice (изчислява се), Status, CreatedAt, UpdatedAt.
/// </summary>
public class CreateReservationDto
{
    [Required(ErrorMessage = "HotelId е задължителен")]
    public int HotelId { get; set; }

    [Required(ErrorMessage = "RoomTypeId е задължителен")]
    public int RoomTypeId { get; set; }

    [Required(ErrorMessage = "Датата на настаняване е задължителна")]
    public DateTime CheckInDate { get; set; }

    [Required(ErrorMessage = "Датата на напускане е задължителна")]
    public DateTime CheckOutDate { get; set; }

    [Range(1, 100, ErrorMessage = "Броят стаи трябва да бъде между 1 и 100")]
    public int NumberOfRooms { get; set; } = 1;

    [MaxLength(500, ErrorMessage = "Бележките не могат да надвишават 500 символа")]
    public string? Notes { get; set; }
}
