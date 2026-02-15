namespace Petoria.Core.DTOs.Comment;

/// <summary>
/// DTO за изходящи данни на коментар.
/// Включва информация за потребителя, рейтинг и брой отговори.
/// НЕ връща вътрешни навигационни свойства на Entity-то.
/// </summary>
public class CommentResponseDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }

    /// <summary>
    /// Рейтингът на текущия потребител: true = like, false = dislike, null = не е гласувал.
    /// </summary>
    public bool? UserRating { get; set; }

    public int RepliesCount { get; set; }

    /// <summary>
    /// Списък с отговори на коментара (за top-level коментари).
    /// </summary>
    public List<CommentResponseDto>? Replies { get; set; }
}
