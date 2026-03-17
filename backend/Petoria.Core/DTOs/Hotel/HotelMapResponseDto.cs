namespace Petoria.Core.DTOs.Hotel;

/// <summary>Отговор: минимални данни за хотел на картата (координати, цена)</summary>
public class HotelMapResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal Rating { get; set; }
    public int StarRating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
