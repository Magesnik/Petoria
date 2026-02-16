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
            SetTokenCookie(result.Token);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var result = await _authService.LoginAsync(model);
        if (result == null)
        {
            return Unauthorized("Invalid credentials");
        }

        SetTokenCookie(result.Token);
        return Ok(result);
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
