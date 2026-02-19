using System.ComponentModel.DataAnnotations;

namespace Petoria.DTOs.PromoCode;

public class CreatePromoCodeDto
{
    [Required]
    public string Code { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal DiscountPercentage { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int MaxActivations { get; set; }

    [Required]
    [Range(1, 3650)]
    public int ValidDays { get; set; }
}
