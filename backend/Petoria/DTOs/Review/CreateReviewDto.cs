using System.ComponentModel.DataAnnotations;

namespace Petoria.DTOs.Review;

/// <summary>
/// DTO за създаване/обновяване на ревю.
/// UserId и HotelId идват съответно от JWT токен и URL маршрут.
/// </summary>
public class CreateReviewDto
{
    [Required(ErrorMessage = "Рейтингът е задължителен")]
    [Range(1, 5, ErrorMessage = "Рейтингът трябва да бъде между 1 и 5")]
    public int Rating { get; set; }

    [MaxLength(2000, ErrorMessage = "Ревюто не може да надвишава 2000 символа")]
    public string ReviewText { get; set; } = string.Empty;
}
