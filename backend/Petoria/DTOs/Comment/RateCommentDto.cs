using System.ComponentModel.DataAnnotations;

namespace Petoria.DTOs.Comment;

/// <summary>
/// DTO за оценяване на коментар (like/dislike).
/// </summary>
public class RateCommentDto
{
    [Required]
    public bool IsLike { get; set; }
}
