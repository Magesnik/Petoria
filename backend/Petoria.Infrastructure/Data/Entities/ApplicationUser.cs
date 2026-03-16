using Microsoft.AspNetCore.Identity;

namespace Petoria.Infrastructure.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    
    // User Preferences
    public string? Theme { get; set; }
    public string? Currency { get; set; }
    public string? Language { get; set; }

    public DateTime CreatedAt { get; set; }
}
