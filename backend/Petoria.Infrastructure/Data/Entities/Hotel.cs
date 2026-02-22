using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Petoria.Constants;
using Petoria.Infrastructure.Data.Entities;

public class Hotel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(ValidationConstants.Hotel.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.Hotel.DescriptionMaxLength)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(ValidationConstants.Hotel.LocationMaxLength)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.Hotel.CityMaxLength)]
    public string City { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.Hotel.CountryMaxLength)]
    public string Country { get; set; } = string.Empty;

    // Geographic coordinates for map display (nullable for existing hotels without coordinates)
    [Column(TypeName = "decimal(10,7)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(10,7)")]
    public decimal? Longitude { get; set; }



    [Column(TypeName = "decimal(2,1)")]
    [Range(ValidationConstants.Hotel.RatingMin, ValidationConstants.Hotel.RatingMax)]
    public decimal Rating { get; set; } = 0;

    [Range(ValidationConstants.Hotel.StarRatingMin, ValidationConstants.Hotel.StarRatingMax)]
    public int StarRating { get; set; } = 3;

    [MaxLength(ValidationConstants.Hotel.ImageUrlMaxLength)]
    public string ImageUrl { get; set; } = string.Empty;

    // JSON array of additional image URLs
    [MaxLength(ValidationConstants.Hotel.ImagesMaxLength)]
    public string Images { get; set; } = "[]";

    // JSON array of amenities
    [MaxLength(ValidationConstants.Hotel.AmenitiesMaxLength)]
    public string Amenities { get; set; } = "[]";

    // JSON array of room types
    [MaxLength(ValidationConstants.Hotel.RoomTypesMaxLength)]
    public string RoomTypes { get; set; } = "[]";

    // JSON array of cancellation policies (days before -> refund percentage)
    [MaxLength(ValidationConstants.Hotel.CancellationPoliciesMaxLength)]
    public string CancellationPolicies { get; set; } = "[]";

    public bool IsAvailable { get; set; } = true;

    // Set by SuperAdmin to globally hide and lock the hotel
    public bool IsSuspendedBySuperAdmin { get; set; } = false;

    // Track who created the hotel - set by controller, not required from client
    public string CreatedById { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
