namespace Petoria.Core.DTOs.Hotel;

public class ModeratorResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime AddedAt { get; set; }
}
