using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Petoria.Core.Contracts;
using Petoria.Core.Models.Auth;


namespace Petoria.Controllers;

/// <summary>
/// Контролер за автентикация: регистрация, логин, Google OAuth, потвърждение на имейл, logout
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _authService = authService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    /// <summary>
    /// Регистрира нов потребител с hCaptcha верификация
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("register")] // 10 рег./мин. на IP — предотвратява спам и бомбардиране с имейли
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        // Верификация на hCaptcha
        var secretKey = _configuration["HcaptchaSettings:SecretKey"];
        if (!string.IsNullOrEmpty(secretKey))
        {
            if (string.IsNullOrEmpty(model.HcaptchaToken))
                return BadRequest(new { message = "CaptchaRequired" });

            var client = _httpClientFactory.CreateClient();
            var verifyResponse = await client.PostAsync(
                _configuration["HcaptchaSettings:VerifyUrl"],
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secretKey,
                    ["response"] = model.HcaptchaToken
                }));

            var json = await verifyResponse.Content.ReadFromJsonAsync<JsonElement>();
            if (!json.GetProperty("success").GetBoolean())
                return BadRequest(new { message = "CaptchaFailed" });
        }

        try
        {
            var result = await _authService.RegisterAsync(model);
            if (!string.IsNullOrEmpty(result.Token))
            {
                SetTokenCookie(result.Token);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message == "EmailNotConfirmed")
            {
                return BadRequest(new { message = "EmailNotConfirmed" });
            }
            if (ex.Message.StartsWith("Registration failed:"))
            {
                return BadRequest(new { message = ex.Message });
            }
            // ВРЪЩАМЕ ДЕТАЙЛНА ГРЕШКА ЗА ДЕБЪГВАНЕ
            return BadRequest(new { message = ex.Message, detail = ex.ToString() });
        }
    }

    public class ConfirmEmailRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// Потвърждава имейл адрес чрез токен
    /// </summary>
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Token))
        {
            return BadRequest("Invalid confirmation request");
        }

        var result = await _authService.ConfirmEmailAsync(request.UserId, request.Token);
        if (result)
        {
            return Ok(new { message = "Email confirmed successfully" });
        }
        return BadRequest("Email confirmation failed");
    }

    /// <summary>
    /// Влизане в системата с имейл и парола
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")] // 10 опита/мин. на IP — защита срещу brute force
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            var result = await _authService.LoginAsync(model);
            if (result == null)
            {
                return Unauthorized("Invalid credentials");
            }

            SetTokenCookie(result.Token);
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message == "EmailNotConfirmed")
            {
                return BadRequest("EmailNotConfirmed");
            }
            if (ex.Message == "UserIsBlocked")
            {
                return BadRequest("UserIsBlocked");
            }
            return BadRequest(new { message = "Login failed. Please try again." });
        }
    }

    /// <summary>
    /// Влизане чрез Google OAuth токен
    /// </summary>
    [HttpPost("google-login")]
    [EnableRateLimiting("auth")] // 10 опита/мин. на IP
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginModel model)
    {
        try
        {
            var result = await _authService.GoogleLoginAsync(model.GoogleToken);
            if (result == null)
            {
                return Unauthorized("Google authentication failed");
            }

            SetTokenCookie(result.Token);
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message == "UserIsBlocked")
            {
                return BadRequest("UserIsBlocked");
            }
            return BadRequest(new { message = "Google login failed. Please try again." });
        }
    }

    /// <summary>
    /// Излизане от системата и изтриване на JWT бисквитката
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var isLocalhost = Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isLocalhost,
            SameSite = isLocalhost ? SameSiteMode.Lax : SameSiteMode.None
        };

        Response.Cookies.Delete("jwt", cookieOptions);

        // Допълнително задаване на изтекла бисквитка за сигурност
        var expiredOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(-1),
            Secure = !isLocalhost,
            SameSite = isLocalhost ? SameSiteMode.Lax : SameSiteMode.None
        };
        Response.Cookies.Append("jwt", "", expiredOptions);

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Задава JWT токен като HttpOnly бисквитка
    /// </summary>
    private void SetTokenCookie(string token)
    {
        var isLocalhost = Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            Secure = !isLocalhost,
            SameSite = isLocalhost ? SameSiteMode.Lax : SameSiteMode.None
        };
        Response.Cookies.Append("jwt", token, cookieOptions);
    }
}
