using Microsoft.AspNetCore.Identity;

namespace Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Потребител (разширява IdentityUser): име, фамилия, аватар, тема, валута, език.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }

    // Потребителски предпочитания
    public string? Theme { get; set; }
    public string? Currency { get; set; }
    public string? Language { get; set; }

    public DateTime CreatedAt { get; set; }
}
