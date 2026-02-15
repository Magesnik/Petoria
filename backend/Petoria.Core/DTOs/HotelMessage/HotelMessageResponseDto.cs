namespace Petoria.Core.DTOs.HotelMessage;

/// <summary>
/// DTO за изходящи данни на съобщение
/// </summary>
public class HotelMessageResponseDto
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsAnswered { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public bool IsReadByUser { get; set; }
}
