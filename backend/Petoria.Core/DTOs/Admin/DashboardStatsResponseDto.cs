namespace Petoria.Core.DTOs.Admin;

/// <summary>Отговор: статистика за админ таблото (потребители, хотели, приходи)</summary>
public class DashboardStatsResponseDto
{
    public int TotalUsers { get; set; }
    public int AdminUsers { get; set; }
    public int SuperAdminUsers { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalReservations { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalHotels { get; set; }
    public int TotalComments { get; set; }
    public int TotalReviews { get; set; }
}
