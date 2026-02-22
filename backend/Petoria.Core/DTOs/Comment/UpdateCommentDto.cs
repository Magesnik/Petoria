using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Comment;

/// <summary>
/// DTO за обновяване на коментар.
/// Потребителят може да промени само текста.
/// </summary>
public class UpdateCommentDto
{
    [Required(ErrorMessage = "Текстът на коментара е задължителен")]
    [MinLength(ValidationConstants.Comment.ContentMinLength, ErrorMessage = "Коментарът не може да бъде празен")]
    [MaxLength(ValidationConstants.Comment.ContentMaxLength, ErrorMessage = "Коментарът не може да надвишава 2000 символа")]
    public string Text { get; set; } = string.Empty;
}
