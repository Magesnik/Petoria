namespace Petoria.Core.DTOs.Admin;

/// <summary>
/// DTO за обща статистика на dashboard-а (SuperAdmin).
/// </summary>
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
