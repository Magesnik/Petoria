namespace Petoria.Core.DTOs.Room;

/// <summary>
/// DTO за изходящи данни на тип стая.
/// Връща се вместо Entity-то, НЕ съдържа навигационни свойства.
/// </summary>
public class RoomTypeResponseDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int Capacity { get; set; }
    public int TotalRooms { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
