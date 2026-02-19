namespace Petoria.Core.DTOs.Hotel;

/// <summary>
/// DTO за изходящи данни на хотел.
/// Връща се от Controller-а при GET заявки.
/// Включва допълнителни изчислени полета като DisplayPrice и HasDiscount.
/// </summary>
public class HotelResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DisplayPrice { get; set; }
    public bool HasDiscount { get; set; }
    public int? DiscountPercentage { get; set; }
    public decimal Rating { get; set; }
    public int StarRating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Images { get; set; } = "[]";
    public string Amenities { get; set; } = "[]";
    public string RoomTypes { get; set; } = "[]";
    public bool IsAvailable { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsModerator { get; set; }
    public int ReviewCount { get; set; }
    public int RecentReviewCount { get; set; } // Last 30 days
    public int AvailableRoomsTotal { get; set; }
}
