using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.Hotel;

/// <summary>
/// DTO за създаване на нов хотел.
/// Използва се от Controller-а при POST заявка.
/// НЕ съдържа: Id, CreatedById, CreatedAt, UpdatedAt, Rating (изчислява се от системата).
/// </summary>
public class CreateHotelDto
{
    [Required(ErrorMessage = "Името на хотела е задължително")]
    [MaxLength(200, ErrorMessage = "Името не може да надвишава 200 символа")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Описанието не може да надвишава 2000 символа")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Локацията е задължителна")]
    [MaxLength(300, ErrorMessage = "Локацията не може да надвишава 300 символа")]
    public string Location { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Градът не може да надвишава 100 символа")]
    public string City { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Държавата не може да надвишава 100 символа")]
    public string Country { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }



    [Range(1, 5, ErrorMessage = "Звездният рейтинг трябва да бъде между 1 и 5")]
    public int StarRating { get; set; } = 3;

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Images { get; set; } = "[]";

    [MaxLength(1000)]
    public string Amenities { get; set; } = "[]";

    [MaxLength(1000)]
    public string RoomTypes { get; set; } = "[]";

    [MaxLength(2000)]
    public string CancellationPolicies { get; set; } = "[]";

    public bool IsAvailable { get; set; } = true;
}
