namespace Petoria.Core.DTOs.Reservation;

/// <summary>Отговор: изчислена цена с разбивка по дни и отстъпки</summary>
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

/// <summary>Разбивка на цена за един ден (оригинална цена, отстъпка, крайна цена)</summary>
public class DayPriceBreakdownDto
{
    public DateTime Date { get; set; }
    public decimal OriginalPrice { get; set; }
    public int? DiscountPercentage { get; set; }
    public decimal FinalPrice { get; set; }
}
