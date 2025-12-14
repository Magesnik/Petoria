using Microsoft.AspNetCore.Identity;

namespace Petoria.Infrastructure.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    // OAuth properties
    public string AuthProvider { get; set; } = "Local"; // Local, Google, Facebook, Microsoft
    public string? ExternalProviderId { get; set; } // External provider's unique ID
}
