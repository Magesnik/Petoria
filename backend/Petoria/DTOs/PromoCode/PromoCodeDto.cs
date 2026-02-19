namespace Petoria.DTOs.PromoCode;

public class PromoCodeDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string Code { get; set; }
    public decimal DiscountPercentage { get; set; }
    public int MaxActivations { get; set; }
    public int CurrentActivations { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsActive { get; set; }
}
