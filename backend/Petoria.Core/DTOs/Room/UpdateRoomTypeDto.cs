using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Room;

/// <summary>
/// DTO за обновяване на тип стая.
/// Id и HotelId идват от URL маршрута.
/// </summary>
public class UpdateRoomTypeDto
{
    [Required(ErrorMessage = ValidationConstants.RoomType.NameRequired)]
    [MaxLength(ValidationConstants.RoomType.NameMaxLength, ErrorMessage = ValidationConstants.RoomType.NameMaxLengthError)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.RoomType.DescriptionMaxLength, ErrorMessage = ValidationConstants.RoomType.DescriptionMaxLengthError)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationConstants.RoomType.PricePerNightRequired)]
    [Range(ValidationConstants.RoomType.PricePerNightMin, ValidationConstants.RoomType.PricePerNightMax, ErrorMessage = ValidationConstants.RoomType.PricePerNightRangeError)]
    public decimal PricePerNight { get; set; }

    [Required(ErrorMessage = ValidationConstants.RoomType.CapacityRequired)]
    [Range(ValidationConstants.RoomType.CapacityMin, ValidationConstants.RoomType.CapacityMax, ErrorMessage = ValidationConstants.RoomType.CapacityRangeError)]
    public int Capacity { get; set; } = 2;

    [Required(ErrorMessage = ValidationConstants.RoomType.TotalRoomsRequired)]
    [Range(ValidationConstants.RoomType.TotalRoomsMin, ValidationConstants.RoomType.TotalRoomsMax, ErrorMessage = ValidationConstants.RoomType.TotalRoomsRangeError)]
    public int TotalRooms { get; set; } = 1;

    [MaxLength(ValidationConstants.RoomType.ImageUrlMaxLength)]
    public string ImageUrl { get; set; } = string.Empty;
}
