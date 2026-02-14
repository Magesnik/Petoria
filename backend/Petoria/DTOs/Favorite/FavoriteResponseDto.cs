namespace Petoria.DTOs.Favorite;

/// <summary>
/// DTO за изходящи данни на любим хотел.
/// Включва основна информация за хотела без тежки полета.
/// </summary>
public class FavoriteResponseDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string HotelCity { get; set; } = string.Empty;
    public string HotelCountry { get; set; } = string.Empty;
    public string HotelImageUrl { get; set; } = string.Empty;
    public decimal HotelPricePerNight { get; set; }
    public decimal HotelRating { get; set; }
    public DateTime CreatedAt { get; set; }
}
