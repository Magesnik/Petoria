namespace Petoria.DTOs.Deals;

/// <summary>
/// DTO за Last Minute оферти.
/// Показва хотели с ниска наличност в следващите 7 дни.
/// </summary>
public class LastMinuteOfferResponseDto
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string HotelCity { get; set; } = string.Empty;
    public string HotelCountry { get; set; } = string.Empty;
    public string HotelImageUrl { get; set; } = string.Empty;
    public decimal HotelRating { get; set; }

    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Description { get; set; } = string.Empty;

    public decimal OriginalPrice { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal SaveAmount { get; set; }

    public int AvailableRoomsCount { get; set; }
    public DateTime EarliestAvailableDate { get; set; }
    public int DaysUntilCheckIn { get; set; }
}
