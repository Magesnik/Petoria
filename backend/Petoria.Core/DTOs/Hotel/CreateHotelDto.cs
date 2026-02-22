using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Hotel;

/// <summary>
/// DTO за създаване на нов хотел.
/// Използва се от Controller-а при POST заявка.
/// НЕ съдържа: Id, CreatedById, CreatedAt, UpdatedAt, Rating (изчислява се от системата).
/// </summary>
public class CreateHotelDto
{
    [Required(ErrorMessage = ValidationConstants.Hotel.NameRequired)]
    [MaxLength(ValidationConstants.Hotel.NameMaxLength, ErrorMessage = ValidationConstants.Hotel.NameMaxLengthError)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.Hotel.DescriptionMaxLength, ErrorMessage = ValidationConstants.Hotel.DescriptionMaxLengthError)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationConstants.Hotel.LocationRequired)]
    [MaxLength(ValidationConstants.Hotel.LocationMaxLength, ErrorMessage = ValidationConstants.Hotel.LocationMaxLengthError)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.Hotel.CityMaxLength, ErrorMessage = ValidationConstants.Hotel.CityMaxLengthError)]
    public string City { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.Hotel.CountryMaxLength, ErrorMessage = ValidationConstants.Hotel.CountryMaxLengthError)]
    public string Country { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }



    [Range(ValidationConstants.Hotel.StarRatingMin, ValidationConstants.Hotel.StarRatingMax, ErrorMessage = ValidationConstants.Hotel.StarRatingRangeError)]
    public int StarRating { get; set; } = 3;

    [MaxLength(ValidationConstants.Hotel.ImageUrlMaxLength)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.Hotel.ImagesMaxLength)]
    public string Images { get; set; } = "[]";

    [MaxLength(ValidationConstants.Hotel.AmenitiesMaxLength)]
    public string Amenities { get; set; } = "[]";

    [MaxLength(ValidationConstants.Hotel.RoomTypesMaxLength)]
    public string RoomTypes { get; set; } = "[]";

    [MaxLength(ValidationConstants.Hotel.CancellationPoliciesMaxLength)]
    public string CancellationPolicies { get; set; } = "[]";

    public bool IsAvailable { get; set; } = true;
}
