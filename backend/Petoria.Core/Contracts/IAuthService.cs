using Petoria.Core.Models.Auth;

namespace Petoria.Core.Contracts;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterModel model);
    Task<AuthResponse> LoginAsync(LoginModel model);
    Task<AuthResponse?> ExternalLoginAsync(ExternalAuthModel model);
    Task InitializeRolesAndAdminAsync();
}
