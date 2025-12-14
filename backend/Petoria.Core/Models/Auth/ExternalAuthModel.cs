namespace Petoria.Core.Models.Auth;

public class ExternalAuthModel
{
    public string Provider { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ExternalUserId { get; set; }
}
