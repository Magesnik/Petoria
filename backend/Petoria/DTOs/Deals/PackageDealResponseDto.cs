namespace Petoria.DTOs.Deals;

/// <summary>
/// DTO за пакетни оферти (7 нощувки с отстъпка).
/// </summary>
public class PackageDealResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int PackageNights { get; set; }
    public decimal OriginalPackagePrice { get; set; }
    public decimal DiscountedPackagePrice { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal SaveAmount { get; set; }
    public decimal PricePerNightWithDiscount { get; set; }
}
