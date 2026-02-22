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
    [Required(ErrorMessage = "Името на хотела е задължително")]
    [MaxLength(ValidationConstants.Hotel.NameMaxLength, ErrorMessage = "Името не може да надвишава 200 символа")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.Hotel.DescriptionMaxLength, ErrorMessage = "Описанието не може да надвишава 2000 символа")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Локацията е задължителна")]
    [MaxLength(ValidationConstants.Hotel.LocationMaxLength, ErrorMessage = "Локацията не може да надвишава 300 символа")]
    public string Location { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.Hotel.CityMaxLength, ErrorMessage = "Градът не може да надвишава 100 символа")]
    public string City { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.Hotel.CountryMaxLength, ErrorMessage = "Държавата не може да надвишава 100 символа")]
    public string Country { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }



    [Range(ValidationConstants.Hotel.StarRatingMin, ValidationConstants.Hotel.StarRatingMax, ErrorMessage = "Звездният рейтинг трябва да бъде между 1 и 5")]
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
