namespace Petoria.Core.DTOs.Favorite;

/// <summary>Отговор: любим хотел с основна информация</summary>
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
