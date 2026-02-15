namespace Petoria.Core.DTOs.Review;

/// <summary>
/// DTO за изходящи данни на ревю.
/// Включва информация за потребителя, който е оставил ревюто.
/// </summary>
public class ReviewResponseDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ReviewUserDto User { get; set; } = null!;
}

/// <summary>
/// Вложен DTO за потребителската информация в ревю.
/// Показва само публична информация — НЕ съдържа email, пароли и т.н.
/// </summary>
public class ReviewUserDto
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
}
