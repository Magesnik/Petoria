using System.ComponentModel.DataAnnotations;
using Petoria.Constants;
namespace Petoria.Core.DTOs.Reservation;

/// <summary>
/// DTO за създаване на резервация.
/// UserId се взима от JWT токена, не от клиента.
/// НЕ съдържа: Id, UserId, TotalPrice (изчислява се), Status, CreatedAt, UpdatedAt.
/// </summary>
public class CreateReservationDto
{
    [Required(ErrorMessage = ValidationConstants.Reservation.HotelRequired)]
    public int HotelId { get; set; }

    [Required(ErrorMessage = ValidationConstants.Reservation.RoomTypeRequired)]
    public int RoomTypeId { get; set; }

    [Required(ErrorMessage = ValidationConstants.Reservation.CheckInDateRequired)]
    public DateTime CheckInDate { get; set; }

    [Required(ErrorMessage = ValidationConstants.Reservation.CheckOutDateRequired)]
    public DateTime CheckOutDate { get; set; }

    [Range(ValidationConstants.Reservation.RoomsMin, ValidationConstants.Reservation.RoomsMax, ErrorMessage = ValidationConstants.Reservation.RoomsRangeError)]
    public int NumberOfRooms { get; set; } = 1;

    [MaxLength(ValidationConstants.Reservation.NotesMaxLength, ErrorMessage = ValidationConstants.Reservation.NotesMaxLengthError)]
    public string? Notes { get; set; }
}
