namespace Petoria.Core.DTOs.HotelMessage;

/// <summary>
/// DTO за брой непрочетени съобщения към хотел
/// </summary>
public class UnreadCountDto
{
    public int HotelId { get; set; }
    public int UnreadCount { get; set; }
}
