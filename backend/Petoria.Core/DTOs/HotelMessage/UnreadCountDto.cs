namespace Petoria.Core.DTOs.HotelMessage;

/// <summary>Отговор: брой непрочетени съобщения по хотели</summary>
public class UnreadCountDto
{
    public int HotelId { get; set; }
    public int UnreadCount { get; set; }
}
