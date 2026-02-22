namespace Petoria.Core.DTOs.Hotel;

public class PopularDestinationDto
{
    public int HotelId { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public int ReservationCount { get; set; }
}
