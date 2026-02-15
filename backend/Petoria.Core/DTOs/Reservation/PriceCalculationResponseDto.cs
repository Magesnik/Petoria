namespace Petoria.Core.DTOs.Reservation;

/// <summary>
/// DTO за отговор при изчисляване на цена.
/// Включва подробна разбивка по дни с отстъпки.
/// </summary>
public class PriceCalculationResponseDto
{
    public int NumberOfNights { get; set; }
    public decimal PricePerNight { get; set; }
    public int NumberOfRooms { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal TotalDiscount { get; set; }
    public List<DayPriceBreakdownDto> Breakdown { get; set; } = new();
}

/// <summary>
/// Разбивка на цена за един ден, показваща оригинална цена, отстъпка и крайна цена.
/// </summary>
public class DayPriceBreakdownDto
{
    public DateTime Date { get; set; }
    public decimal OriginalPrice { get; set; }
    public int? DiscountPercentage { get; set; }
    public decimal FinalPrice { get; set; }
}
