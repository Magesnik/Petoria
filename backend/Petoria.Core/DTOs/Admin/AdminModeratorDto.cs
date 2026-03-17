namespace Petoria.Core.DTOs.Admin;

/// <summary>Отговор: данни за модератор с хотел и потребител</summary>
public class AdminModeratorDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}
