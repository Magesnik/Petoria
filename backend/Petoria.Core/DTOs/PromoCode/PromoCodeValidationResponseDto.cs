namespace Petoria.Core.DTOs.PromoCode;

public class PromoCodeValidationResponseDto
{
    public decimal DiscountPercentage { get; set; }
    public int? HotelId { get; set; }
    public bool IsGlobal => HotelId == null;
    public string Code { get; set; }
}
