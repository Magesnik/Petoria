using Petoria.Core.Models.Auth;

namespace Petoria.Core.Contracts;

/// <summary>
/// Интерфейс за автентикация и управление на потребителски акаунти.
/// Обхваща регистрация, логин, Google OAuth и потвърждение на имейл.
/// </summary>
public interface IAuthService
{
    /// <summary>Регистрира нов потребител и изпраща имейл за потвърждение</summary>
    Task<AuthResponse> RegisterAsync(RegisterModel model);

    /// <summary>Влизане с имейл и парола — връща JWT токен</summary>
    Task<AuthResponse> LoginAsync(LoginModel model);

    /// <summary>Влизане чрез Google OAuth — създава акаунт автоматично ако не съществува</summary>
    Task<AuthResponse?> GoogleLoginAsync(string googleToken);

    /// <summary>Потвърждава имейл адреса на потребител чрез токен от линка</summary>
    Task<bool> ConfirmEmailAsync(string userId, string token);

    /// <summary>Създава ролите (User, Admin, SuperAdmin, HotelModerator) и админ акаунта при стартиране</summary>
    Task InitializeRolesAndAdminAsync();
}
