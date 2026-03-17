namespace Petoria.Core.DTOs.PromoCode;

/// <summary>Отговор: резултат от валидация на промо код</summary>
public class PromoCodeValidationResponseDto
{
    public decimal DiscountPercentage { get; set; }
    public int? HotelId { get; set; }
    public bool IsGlobal => HotelId == null;
    public string Code { get; set; }
}
