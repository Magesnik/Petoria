using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Room;

/// <summary>
/// DTO за създаване на тип стая.
/// HotelId идва от URL маршрута.
/// НЕ съдържа: Id, HotelId, CreatedAt, UpdatedAt, навигационни свойства.
/// </summary>
public class CreateRoomTypeDto
{
    [Required(ErrorMessage = "Името на типа стая е задължително")]
    [MaxLength(ValidationConstants.RoomType.NameMaxLength, ErrorMessage = "Името не може да надвишава 100 символа")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.RoomType.DescriptionMaxLength, ErrorMessage = "Описанието не може да надвишава 500 символа")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Цената на нощувка е задължителна")]
    [Range(ValidationConstants.RoomType.PricePerNightMin, ValidationConstants.RoomType.PricePerNightMax, ErrorMessage = "Цената трябва да бъде между 0.01 и 100000")]
    public decimal PricePerNight { get; set; }

    [Required(ErrorMessage = "Капацитетът е задължителен")]
    [Range(ValidationConstants.RoomType.CapacityMin, ValidationConstants.RoomType.CapacityMax, ErrorMessage = "Капацитетът трябва да бъде между 1 и 20")]
    public int Capacity { get; set; } = 2;

    [Required(ErrorMessage = "Броят стаи е задължителен")]
    [Range(ValidationConstants.RoomType.TotalRoomsMin, ValidationConstants.RoomType.TotalRoomsMax, ErrorMessage = "Броят стаи трябва да бъде между 1 и 1000")]
    public int TotalRooms { get; set; } = 1;

    [MaxLength(ValidationConstants.RoomType.ImageUrlMaxLength)]
    public string ImageUrl { get; set; } = string.Empty;
}
