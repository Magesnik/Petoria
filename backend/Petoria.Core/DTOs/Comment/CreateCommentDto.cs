using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Comment;

/// <summary>
/// DTO за създаване на коментар.
/// UserId се взима от JWT токена автоматично.
/// </summary>
public class CreateCommentDto
{
    [Required(ErrorMessage = "HotelId е задължителен")]
    public int HotelId { get; set; }

    [Required(ErrorMessage = ValidationConstants.Comment.ContentRequired)]
    [MinLength(ValidationConstants.Comment.ContentMinLength, ErrorMessage = ValidationConstants.Comment.ContentMinLengthError)]
    [MaxLength(ValidationConstants.Comment.ContentMaxLength, ErrorMessage = ValidationConstants.Comment.ContentMaxLengthError)]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Ако е null — създава се коментар от първо ниво.
    /// Ако е попълнен — създава се отговор на друг коментар.
    /// </summary>
    public int? ParentCommentId { get; set; }
}
