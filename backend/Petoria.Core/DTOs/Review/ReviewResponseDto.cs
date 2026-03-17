namespace Petoria.Core.DTOs.Review;

/// <summary>Отговор: ревю с автор и рейтинг</summary>
public class ReviewResponseDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ReviewUserDto User { get; set; } = null!;
}

/// <summary>Вложен DTO: потребителска информация в ревю</summary>
public class ReviewUserDto
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
}
