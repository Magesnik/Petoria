using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Discount;

/// <summary>
/// DTO за създаване на отстъпка.
/// </summary>
public class CreateDiscountDto
{
    [Required(ErrorMessage = "RoomTypeId е задължителен")]
    public int RoomTypeId { get; set; }

    [Required(ErrorMessage = "Началната дата е задължителна")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Крайната дата е задължителна")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Процентът на отстъпка е задължителен")]
    [Range(ValidationConstants.Discount.PercentageMin, ValidationConstants.Discount.PercentageMax, ErrorMessage = "Отстъпката трябва да бъде между 1% и 99%")]
    public int DiscountPercentage { get; set; }
}
