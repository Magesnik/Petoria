namespace Petoria.Core.DTOs.Hotel;

/// <summary>Отговор: пълна информация за хотел</summary>
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
    public string CancellationPolicies { get; set; } = "[]";
    public bool IsAvailable { get; set; }
    public bool IsSuspendedBySuperAdmin { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsModerator { get; set; }
    public int ReviewCount { get; set; }
    public int RecentReviewCount { get; set; }
    public int AvailableRoomsTotal { get; set; }
}
