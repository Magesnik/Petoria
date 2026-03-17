using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Petoria.Constants;
using Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Хотел: име, описание, локация, координати, рейтинг, звезди, снимки, удобства, типове стаи, политики за анулиране.
/// </summary>
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

    // Географски координати за показване на карта (nullable за съществуващи хотели без координати)
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

    // JSON масив с допълнителни URL адреси на снимки
    [MaxLength(ValidationConstants.Hotel.ImagesMaxLength)]
    public string Images { get; set; } = "[]";

    // JSON масив с удобства
    [MaxLength(ValidationConstants.Hotel.AmenitiesMaxLength)]
    public string Amenities { get; set; } = "[]";

    // JSON масив с типове стаи
    [MaxLength(ValidationConstants.Hotel.RoomTypesMaxLength)]
    public string RoomTypes { get; set; } = "[]";

    // JSON масив с политики за анулиране (дни преди -> процент възстановяване)
    [MaxLength(ValidationConstants.Hotel.CancellationPoliciesMaxLength)]
    public string CancellationPolicies { get; set; } = "[]";

    public bool IsAvailable { get; set; } = true;

    // Задава се от SuperAdmin за глобално скриване и заключване на хотела
    public bool IsSuspendedBySuperAdmin { get; set; } = false;

    // Кой е създал хотела - задава се от контролера, не се изисква от клиента
    public string CreatedById { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
