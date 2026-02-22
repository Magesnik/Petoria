using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Availability;

/// <summary>
/// DTO за масово задаване на наличност за диапазон от дати.
/// </summary>
public class BulkAvailabilityRequestDto
{
    [Required(ErrorMessage = ValidationConstants.Availability.RoomTypeRequired)]
    public int RoomTypeId { get; set; }

    [Required(ErrorMessage = ValidationConstants.Availability.StartDateRequired)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = ValidationConstants.Availability.EndDateRequired)]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = ValidationConstants.Availability.AvailableCountRequired)]
    [Range(ValidationConstants.Availability.AvailableCountMin, ValidationConstants.Availability.AvailableCountMax, ErrorMessage = ValidationConstants.Availability.AvailableCountRangeError)]
    public int AvailableCount { get; set; }
}
