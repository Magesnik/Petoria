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
