namespace Petoria.Core.DTOs.Availability;

/// <summary>Отговор: наличност на стая за дата (свободни стаи, блокирана, отстъпка)</summary>
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
