namespace Petoria.Core.DTOs.Admin;

/// <summary>Отговор: обобщена статистика за потребител</summary>
public class UserStatsResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public bool IsBlocked { get; set; }
    public int FavoritesCount { get; set; }
    public int ReservationsCount { get; set; }
    public decimal TotalSpent { get; set; }
    public int HotelsCreated { get; set; }
    public int CommentsCount { get; set; }
    public int ReviewsCount { get; set; }
}
