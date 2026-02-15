using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.Room;

/// <summary>
/// DTO за обновяване на тип стая.
/// Id и HotelId идват от URL маршрута.
/// </summary>
public class UpdateRoomTypeDto
{
    [Required(ErrorMessage = "Името на типа стая е задължително")]
    [MaxLength(100, ErrorMessage = "Името не може да надвишава 100 символа")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Описанието не може да надвишава 500 символа")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Цената на нощувка е задължителна")]
    [Range(0.01, 100000, ErrorMessage = "Цената трябва да бъде между 0.01 и 100000")]
    public decimal PricePerNight { get; set; }

    [Required(ErrorMessage = "Капацитетът е задължителен")]
    [Range(1, 20, ErrorMessage = "Капацитетът трябва да бъде между 1 и 20")]
    public int Capacity { get; set; } = 2;

    [Required(ErrorMessage = "Броят стаи е задължителен")]
    [Range(1, 1000, ErrorMessage = "Броят стаи трябва да бъде между 1 и 1000")]
    public int TotalRooms { get; set; } = 1;

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
}
