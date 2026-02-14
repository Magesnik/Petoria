namespace Petoria.DTOs.Availability;

/// <summary>
/// DTO за изходящи данни за наличност на стая.
/// Показва наличността за конкретна дата и тип стая.
/// </summary>
public class AvailabilityResponseDto
{
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int Capacity { get; set; }
    public DateTime Date { get; set; }
    public int AvailableCount { get; set; }
    public bool IsBlocked { get; set; }
    public int? DiscountPercentage { get; set; }
    public decimal? DiscountedPrice { get; set; }
}
