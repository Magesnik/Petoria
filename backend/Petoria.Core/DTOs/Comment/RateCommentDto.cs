using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.Comment;

/// <summary>Заявка: лайк/дислайк на коментар</summary>
public class RateCommentDto
{
    [Required]
    public bool IsLike { get; set; }
}
