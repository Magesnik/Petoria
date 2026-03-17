using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.Comment;

/// <summary>Заявка: редакция на текста на коментар</summary>
public class UpdateCommentDto
{
    [Required(ErrorMessage = ValidationConstants.Comment.ContentRequired)]
    [MinLength(ValidationConstants.Comment.ContentMinLength, ErrorMessage = ValidationConstants.Comment.ContentMinLengthError)]
    [MaxLength(ValidationConstants.Comment.ContentMaxLength, ErrorMessage = ValidationConstants.Comment.ContentMaxLengthError)]
    public string Text { get; set; } = string.Empty;
}
