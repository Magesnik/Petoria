using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Discount;

/// <summary>
/// DTO за обновяване на отстъпка.
/// RoomTypeId НЕ може да се променя — само дати и процент.
/// </summary>
public class UpdateDiscountDto
{
    [Required(ErrorMessage = ValidationConstants.Discount.StartDateRequired)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = ValidationConstants.Discount.EndDateRequired)]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = ValidationConstants.Discount.PercentageRequired)]
    [Range(ValidationConstants.Discount.PercentageMin, ValidationConstants.Discount.PercentageMax, ErrorMessage = ValidationConstants.Discount.PercentageRangeError)]
    public int DiscountPercentage { get; set; }
}
