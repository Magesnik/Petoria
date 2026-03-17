using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Petoria.Core.Contracts;
using Petoria.Core.Models.Auth;
using Petoria.Infrastructure.Data.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;

namespace Petoria.Core.Services;

/// <summary>
/// Услуга за автентикация — регистрация, логин, Google OAuth, потвърждение на имейл и инициализация на роли.
/// Генерира JWT токени с 7-дневна валидност.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _emailService = emailService;
    }

    /// <summary>Влизане с имейл и парола. Проверява потвърждение на имейл и блокиране на акаунта.</summary>
    public async Task<AuthResponse> LoginAsync(LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return null;
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            throw new Exception("EmailNotConfirmed");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new Exception("UserIsBlocked");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return await GenerateJwtToken(user, roles.ToList());
    }

    /// <summary>Логин чрез Google OAuth. Създава нов потребител ако не съществува. Дава SuperAdmin роля на конфигурирани имейли.</summary>
    public async Task<AuthResponse?> GoogleLoginAsync(string googleToken)
    {
        try
        {
            // Валидиране на Google токена
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);

            if (payload == null)
            {
                return null;
            }

            // Проверка дали потребителят вече съществува
            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                // Създаване на нов потребител от Google данните
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    EmailConfirmed = true // Google имейлите са вече верифицирани
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    return null;
                }

                // Даване на SuperAdmin роля ако имейлът е в конфигурирания списък
                if (IsSuperAdminEmail(payload.Email))
                {
                    await _userManager.AddToRoleAsync(user, Petoria.Constants.Roles.SuperAdmin);
                    await _userManager.AddToRoleAsync(user, Petoria.Constants.Roles.Admin);
                }
            }
            else
            {
                // Осигуряване на SuperAdmin роля за конфигурирани имейли при съществуващ потребител
                if (IsSuperAdminEmail(payload.Email))
                {
                    if (!await _userManager.IsInRoleAsync(user, Petoria.Constants.Roles.SuperAdmin))
                    {
                        await _userManager.AddToRoleAsync(user, Petoria.Constants.Roles.SuperAdmin);
                    }
                    if (!await _userManager.IsInRoleAsync(user, Petoria.Constants.Roles.Admin))
                    {
                        await _userManager.AddToRoleAsync(user, Petoria.Constants.Roles.Admin);
                    }
                }
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new Exception("UserIsBlocked");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return await GenerateJwtToken(user, roles.ToList());
        }
        catch (Exception)
        {
            return null;
        }
    }


    /// <summary>Регистрация на нов потребител. Изпраща имейл за потвърждение. Не дава JWT докато имейлът не е потвърден.</summary>
    public async Task<AuthResponse> RegisterAsync(RegisterModel model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
                // Събиране на грешките от Identity
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Registration failed: {errors}");
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Генериране на токен за потвърждение на имейл
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        
        // Изграждане на линк за потвърждение към frontend-а
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5174";
        var encodedToken = Uri.EscapeDataString(token);
        var confirmationLink = $"{frontendUrl}/confirm-email?uid={user.Id}&token={encodedToken}";

        // Изпращане на имейл за потвърждение
        var emailBody = $@"
            <h2>Добре дошли в Petoria!</h2>
            <p>Здравейте {user.FirstName},</p>
            <p>Моля, потвърдете вашия имейл адрес, като кликнете на следния линк:</p>
            <p><a href='{confirmationLink}'>Потвърди имейл адрес</a></p>
            <p>Ако не сте създали този акаунт, можете спокойно да игнорирате този имейл.</p>
            <p>Поздрави,<br>Екипът на Petoria</p>
        ";
        await _emailService.SendEmailAsync(user.Email, "Потвърждение на акаунт - Petoria", emailBody);

        // Връщане на отговор без токен — потребителят трябва първо да потвърди имейла си
        return new AuthResponse
        {
            Id = user.Id,
            Token = string.Empty,
            Email = user.Email!,
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            Roles = roles.ToList(),
            AvatarUrl = user.AvatarUrl
        };
    }

    /// <summary>Потвърждава имейл адреса чрез токен от линка за потвърждение.</summary>
    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    /// <summary>Създава ролите (Admin, SuperAdmin) и админ акаунта при стартиране на приложението.</summary>
    public async Task InitializeRolesAndAdminAsync()
    {
        // Създаване на Admin роля ако не съществува
        if (!await _roleManager.RoleExistsAsync(Petoria.Constants.Roles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(Petoria.Constants.Roles.Admin));
        }

        // Създаване на SuperAdmin роля ако не съществува
        if (!await _roleManager.RoleExistsAsync(Petoria.Constants.Roles.SuperAdmin))
        {
            await _roleManager.CreateAsync(new IdentityRole(Petoria.Constants.Roles.SuperAdmin));
        }

        // Проверка дали админ потребителят вече съществува
        var adminEmail = _configuration["AdminSettings:DefaultAdminEmail"] ?? "admin@admin.com";
        var adminPassword = _configuration["AdminSettings:DefaultAdminPassword"];
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null && !string.IsNullOrEmpty(adminPassword))
        {
            // Създаване на админ потребител само ако паролата е конфигурирана
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, Petoria.Constants.Roles.Admin);
            }
        }
        else if (adminUser != null)
        {
            // Осигуряване на Admin роля за съществуващия админ
            if (!await _userManager.IsInRoleAsync(adminUser, Petoria.Constants.Roles.Admin))
            {
                await _userManager.AddToRoleAsync(adminUser, Petoria.Constants.Roles.Admin);
            }
        }

        // Осигуряване на SuperAdmin роля за конфигурирани имейли
        var superAdminEmails = _configuration["AdminSettings:SuperAdminEmails"]
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

        foreach (var email in superAdminEmails)
        {
            var superAdminUser = await _userManager.FindByEmailAsync(email);
            if (superAdminUser != null)
            {
                if (!await _userManager.IsInRoleAsync(superAdminUser, Petoria.Constants.Roles.SuperAdmin))
                {
                    await _userManager.AddToRoleAsync(superAdminUser, Petoria.Constants.Roles.SuperAdmin);
                }
                if (!await _userManager.IsInRoleAsync(superAdminUser, Petoria.Constants.Roles.Admin))
                {
                    await _userManager.AddToRoleAsync(superAdminUser, Petoria.Constants.Roles.Admin);
                }
            }
        }
    }

    /// <summary>Проверява дали имейлът е в конфигурирания списък на SuperAdmin имейли.</summary>
    private bool IsSuperAdminEmail(string email)
    {
        var superAdminEmails = _configuration["AdminSettings:SuperAdminEmails"]
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

        return superAdminEmails.Any(e => e.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Генерира JWT токен с claims за потребителя (ID, имейл, име, роли). Валидност: 7 дни.</summary>
    private async Task<AuthResponse> GenerateJwtToken(ApplicationUser user, List<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim("FirstName", user.FirstName ?? ""),
            new Claim("LastName", user.LastName ?? "")
        };

        // Добавяне на роли като claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new AuthResponse
        {
            Id = user.Id,
            Token = tokenHandler.WriteToken(token),
            Email = user.Email!,
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            Expiration = tokenDescriptor.Expires.Value,
            Roles = roles,
            AvatarUrl = user.AvatarUrl
        };
    }
}
