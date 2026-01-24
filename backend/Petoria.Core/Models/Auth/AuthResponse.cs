namespace Petoria.Core.Models.Auth;

public class AuthResponse
{
    public string Id { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}
