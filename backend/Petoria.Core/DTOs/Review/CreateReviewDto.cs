using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Review;

/// <summary>
/// DTO за създаване/обновяване на ревю.
/// UserId и HotelId идват съответно от JWT токен и URL маршрут.
/// </summary>
public class CreateReviewDto
{
    [Required(ErrorMessage = ValidationConstants.HotelReview.RatingRequired)]
    [Range(ValidationConstants.HotelReview.RatingMin, ValidationConstants.HotelReview.RatingMax, ErrorMessage = ValidationConstants.HotelReview.RatingRangeError)]
    public int Rating { get; set; }

    [MaxLength(ValidationConstants.HotelReview.CommentMaxLength, ErrorMessage = ValidationConstants.HotelReview.CommentMaxLengthError)]
    public string ReviewText { get; set; } = string.Empty;
}
