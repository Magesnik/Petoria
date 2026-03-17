namespace Petoria.Core.DTOs.Room;

/// <summary>Отговор: данни за тип стая (цена, капацитет, брой)</summary>
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
