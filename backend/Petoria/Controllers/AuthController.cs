using Microsoft.AspNetCore.Mvc;
using Petoria.Core.Contracts;
using Petoria.Core.Models.Auth;


namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
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
            return BadRequest(new { message = ex.Message });
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
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginModel model)
    {
        var result = await _authService.GoogleLoginAsync(model.GoogleToken);
        if (result == null)
        {
            return Unauthorized("Google authentication failed");
        }

        SetTokenCookie(result.Token);
        return Ok(result);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt", new CookieOptions
        {
            HttpOnly = true,
            Secure = true, 
            SameSite = SameSiteMode.None // Changed to None for ensuring cross-site if needed, or Strict if same domain
        });
        
        // Also try setting it to expired to be sure
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(-1),
            Secure = true,
            SameSite = SameSiteMode.None
        };
        Response.Cookies.Append("jwt", "", cookieOptions);

        return Ok(new { message = "Logged out successfully" });
    }

    private void SetTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            Secure = true, // Set to true in production, but also needed for SameSite=None
            SameSite = SameSiteMode.None // 'None' is often needed if frontend/backend form different origins (localhost:5174 vs 5150)
            // If they were on same domain, Strict or Lax would be better.
            // Since we use CORS with specific origin, SameSite=None + Secure is usually required for cross-origin cookies.
        };
        Response.Cookies.Append("jwt", token, cookieOptions);
    }
}
