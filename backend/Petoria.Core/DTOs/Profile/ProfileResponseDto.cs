namespace Petoria.Core.DTOs.Profile;

/// <summary>Отговор: профил на потребителя (имена, аватар, роли, настройки)</summary>
public class ProfileResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Theme { get; set; }
    public string? Currency { get; set; }
    public string? Language { get; set; }
    public decimal TotalSpent { get; set; }
    public List<string> Roles { get; set; } = new();
}
