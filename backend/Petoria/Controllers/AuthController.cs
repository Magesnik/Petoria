using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Petoria.Core.Contracts;
using Petoria.Core.Models.Auth;


namespace Petoria.Controllers;

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

    [HttpPost("register")]
    [EnableRateLimiting("register")] // 10 reg/min per IP — prevents spam & email bombing
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        // hCaptcha verification
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
            return BadRequest(new { message = "Registration failed. Please try again." });
        }
    }

    public class ConfirmEmailRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

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

    [HttpPost("login")]
    [EnableRateLimiting("auth")] // 10 attempts/min per IP — brute force protection
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

    [HttpPost("google-login")]
    [EnableRateLimiting("auth")] // 10 attempts/min per IP
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
        
        // Also try setting it to expired to be sure
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
