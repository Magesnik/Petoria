namespace Petoria.Core.DTOs.Hotel;

/// <summary>
/// Олекотен DTO за показване на хотели на картата.
/// Съдържа само полетата, необходими за маркерите на картата.
/// </summary>
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
