namespace Petoria.Core.DTOs.Reservation;

/// <summary>
/// DTO за изходящи данни на резервация.
/// Включва допълнителни полета от свързани Entity-та (име на хотел, тип стая и т.н.).
/// НЕ връща вътрешни системни полета като UserId.
/// </summary>
public class ReservationResponseDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string HotelImageUrl { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfRooms { get; set; }
    public int NumberOfNights { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
