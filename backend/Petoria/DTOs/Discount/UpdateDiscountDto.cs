using System.ComponentModel.DataAnnotations;

namespace Petoria.DTOs.Discount;

/// <summary>
/// DTO за обновяване на отстъпка.
/// RoomTypeId НЕ може да се променя — само дати и процент.
/// </summary>
public class UpdateDiscountDto
{
    [Required(ErrorMessage = "Началната дата е задължителна")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Крайната дата е задължителна")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Процентът на отстъпка е задължителен")]
    [Range(1, 99, ErrorMessage = "Отстъпката трябва да бъде между 1% и 99%")]
    public int DiscountPercentage { get; set; }
}
