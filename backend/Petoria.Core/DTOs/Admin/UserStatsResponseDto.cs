namespace Petoria.Core.DTOs.Admin;

/// <summary>
/// DTO за изходящи данни на потребителска статистика (списък с потребители).
/// Използва се от SuperAdmin dashboard-а.
/// </summary>
public class UserStatsResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public int FavoritesCount { get; set; }
    public int ReservationsCount { get; set; }
    public decimal TotalSpent { get; set; }
    public int HotelsCreated { get; set; }
    public int CommentsCount { get; set; }
}
