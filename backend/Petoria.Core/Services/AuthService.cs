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

    public async Task<AuthResponse?> GoogleLoginAsync(string googleToken)
    {
        try
        {
            // Verify the Google token
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);

            if (payload == null)
            {
                return null;
            }

            // Check if user exists
            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                // Create new user from Google data
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    EmailConfirmed = true // Google emails are verified
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    return null;
                }

                // Assign SuperAdmin role to specific email
                if (payload.Email.Equals("pepi.200712@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    await _userManager.AddToRoleAsync(user, Petoria.Constants.Roles.SuperAdmin);
                    await _userManager.AddToRoleAsync(user, Petoria.Constants.Roles.Admin);
                }
            }
            else
            {
                // User exists - ensure pepi.200712@gmail.com has SuperAdmin role
                if (payload.Email.Equals("pepi.200712@gmail.com", StringComparison.OrdinalIgnoreCase))
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


    public async Task<AuthResponse> RegisterAsync(RegisterModel model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            // For simplicity, returning null or throwing exception could be handled better
            // In a real app, return the errors
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Registration failed: {errors}");
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        
        // Construct the confirmation link
        // Hardcoding the frontend URL for now, could be moved to configuration
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5174";
        var encodedToken = Uri.EscapeDataString(token);
        var confirmationLink = $"{frontendUrl}/confirm-email?uid={user.Id}&token={encodedToken}";

        // Send confirmation email
        var emailBody = $@"
            <h2>Добре дошли в Petoria!</h2>
            <p>Здравейте {user.FirstName},</p>
            <p>Моля, потвърдете вашия имейл адрес, като кликнете на следния линк:</p>
            <p><a href='{confirmationLink}'>Потвърди имейл адрес</a></p>
            <p>Ако не сте създали този акаунт, можете спокойно да игнорирате този имейл.</p>
            <p>Поздрави,<br>Екипът на Petoria</p>
        ";
        await _emailService.SendEmailAsync(user.Email, "Потвърждение на акаунт - Petoria", emailBody);

        // Return AuthResponse without a token since they need to confirm their email first
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

    public async Task InitializeRolesAndAdminAsync()
    {
        // Create Admin role if it doesn't exist
        if (!await _roleManager.RoleExistsAsync(Petoria.Constants.Roles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(Petoria.Constants.Roles.Admin));
        }

        // Create SuperAdmin role if it doesn't exist
        if (!await _roleManager.RoleExistsAsync(Petoria.Constants.Roles.SuperAdmin))
        {
            await _roleManager.CreateAsync(new IdentityRole(Petoria.Constants.Roles.SuperAdmin));
        }

        // Check if admin user exists
        var adminEmail = "admin@admin.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // Create admin user
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, "123456Q@w");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, Petoria.Constants.Roles.Admin);
            }
        }
        else
        {
            // Ensure existing admin user has Admin role
            if (!await _userManager.IsInRoleAsync(adminUser, Petoria.Constants.Roles.Admin))
            {
                await _userManager.AddToRoleAsync(adminUser, Petoria.Constants.Roles.Admin);
            }
        }

        // Ensure pepi.200712@gmail.com has SuperAdmin role
        var superAdminEmail = "pepi.200712@gmail.com";
        var superAdminUser = await _userManager.FindByEmailAsync(superAdminEmail);
        if (superAdminUser != null)
        {
            if (!await _userManager.IsInRoleAsync(superAdminUser, Petoria.Constants.Roles.SuperAdmin))
            {
                await _userManager.AddToRoleAsync(superAdminUser, Petoria.Constants.Roles.SuperAdmin);
            }
            // Also ensure they have Admin role for backwards compatibility
            if (!await _userManager.IsInRoleAsync(superAdminUser, Petoria.Constants.Roles.Admin))
            {
                await _userManager.AddToRoleAsync(superAdminUser, Petoria.Constants.Roles.Admin);
            }
        }
    }

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

        // Add role claims
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
