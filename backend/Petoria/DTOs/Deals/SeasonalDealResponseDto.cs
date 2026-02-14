namespace Petoria.DTOs.Deals;

/// <summary>
/// DTO за сезонни оферти.
/// Като DiscountedHotelResponseDto, но с допълнително поле за сезон.
/// </summary>
public class SeasonalDealResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal MinPrice { get; set; }
    public int MaxDiscount { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal SaveAmount { get; set; }
    public string Season { get; set; } = string.Empty;
}
