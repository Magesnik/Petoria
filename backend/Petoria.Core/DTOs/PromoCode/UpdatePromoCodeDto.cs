using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.PromoCode;

public class UpdatePromoCodeDto
{
    [Required]
    [Range(ValidationConstants.PromoCode.DiscountPercentageMin, ValidationConstants.PromoCode.DiscountPercentageMax)]
    public decimal DiscountPercentage { get; set; }

    [Required]
    [Range(ValidationConstants.PromoCode.MaxUsesMin, int.MaxValue)]
    public int MaxActivations { get; set; }
}
