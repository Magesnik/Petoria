namespace Petoria.Core.DTOs.Deals;

/// <summary>
/// DTO за хотел с активна отстъпка.
/// Показва оригинална и намалена цена.
/// </summary>
public class DiscountedHotelResponseDto
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
}
