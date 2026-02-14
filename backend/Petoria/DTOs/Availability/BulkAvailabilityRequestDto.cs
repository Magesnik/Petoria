using System.ComponentModel.DataAnnotations;

namespace Petoria.DTOs.Availability;

/// <summary>
/// DTO за масово задаване на наличност за диапазон от дати.
/// </summary>
public class BulkAvailabilityRequestDto
{
    [Required(ErrorMessage = "RoomTypeId е задължителен")]
    public int RoomTypeId { get; set; }

    [Required(ErrorMessage = "Началната дата е задължителна")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Крайната дата е задължителна")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Броят свободни стаи е задължителен")]
    [Range(0, 1000, ErrorMessage = "Броят стаи трябва да бъде между 0 и 1000")]
    public int AvailableCount { get; set; }
}
