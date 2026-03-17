using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Review;

/// <summary>Заявка: създаване/обновяване на ревю с рейтинг</summary>
public class CreateReviewDto
{
    [Required(ErrorMessage = ValidationConstants.HotelReview.RatingRequired)]
    [Range(ValidationConstants.HotelReview.RatingMin, ValidationConstants.HotelReview.RatingMax, ErrorMessage = ValidationConstants.HotelReview.RatingRangeError)]
    public int Rating { get; set; }

    [MaxLength(ValidationConstants.HotelReview.CommentMaxLength, ErrorMessage = ValidationConstants.HotelReview.CommentMaxLengthError)]
    public string ReviewText { get; set; } = string.Empty;
}
