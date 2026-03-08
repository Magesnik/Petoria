using Petoria.Core.Models.Auth;

namespace Petoria.Core.Contracts;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterModel model);
    Task<AuthResponse> LoginAsync(LoginModel model);
    Task<AuthResponse?> GoogleLoginAsync(string googleToken);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task InitializeRolesAndAdminAsync();
}
