using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Petoria.Core.DTOs.Profile;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _environment = environment;
    }

    // GET: api/profile
    [HttpGet]
    public async Task<ActionResult<ProfileResponseDto>> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Map Entity → Response DTO
        return Ok(new ProfileResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl
        });
    }

    // PUT: api/profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Map DTO → Entity (update)
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // Map Entity → Response DTO
        return Ok(new
        {
            message = "Profile updated successfully",
            user = new ProfileResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarUrl = user.AvatarUrl
            }
        });
    }

    // POST: api/profile/avatar
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Only JPG, JPEG, and PNG files are allowed");
        }

        // Validate file size (max 2MB)
        if (file.Length > 2 * 1024 * 1024)
        {
            return BadRequest("File size must be less than 2MB");
        }

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsPath);

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var oldAvatarPath = Path.Combine(_environment.WebRootPath, user.AvatarUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldAvatarPath))
                {
                    System.IO.File.Delete(oldAvatarPath);
                }
            }

            // Generate unique filename
            var fileName = $"{userId}_{DateTime.Now.Ticks}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update user avatar URL
            user.AvatarUrl = $"/uploads/avatars/{fileName}";
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                message = "Avatar uploaded successfully",
                avatarUrl = user.AvatarUrl
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error uploading avatar", error = ex.Message });
        }
    }

    // DELETE: api/profile/avatar
    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(user.AvatarUrl))
        {
            return BadRequest("No avatar to delete");
        }

        try
        {
            // Delete file from filesystem
            var avatarPath = Path.Combine(_environment.WebRootPath, user.AvatarUrl.TrimStart('/'));
            if (System.IO.File.Exists(avatarPath))
            {
                System.IO.File.Delete(avatarPath);
            }

            // Remove avatar URL from user
            user.AvatarUrl = null;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Avatar deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting avatar", error = ex.Message });
        }
    }
}
