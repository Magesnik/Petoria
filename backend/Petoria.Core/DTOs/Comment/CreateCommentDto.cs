using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.Comment;

/// <summary>
/// DTO за създаване на коментар.
/// UserId се взима от JWT токена автоматично.
/// </summary>
public class CreateCommentDto
{
    [Required(ErrorMessage = "HotelId е задължителен")]
    public int HotelId { get; set; }

    [Required(ErrorMessage = "Текстът на коментара е задължителен")]
    [MinLength(1, ErrorMessage = "Коментарът не може да бъде празен")]
    [MaxLength(2000, ErrorMessage = "Коментарът не може да надвишава 2000 символа")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Ако е null — създава се коментар от първо ниво.
    /// Ако е попълнен — създава се отговор на друг коментар.
    /// </summary>
    public int? ParentCommentId { get; set; }
}
