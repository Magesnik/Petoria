using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Profile;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Petoria.Core.Contracts.IPhotoService _photoService;

    public ProfileController(UserManager<ApplicationUser> userManager, Petoria.Core.Contracts.IPhotoService photoService)
    {
        _userManager = userManager;
        _photoService = photoService;
    }

    // GET: api/profile
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ProfileResponseDto>> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            // Return 200 OK with null to avoid 401 browser console errors
            return Ok(null);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Ok(null);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Unauthorized(new { message = "UserIsBlocked" });
        }

        // Map Entity → Response DTO
        var roles = await _userManager.GetRolesAsync(user);

        // Access database context directly or via service to get total spent
        // We'll calculate it inline since we don't have a specific service for this injected in this action.
        var dbContext = HttpContext.RequestServices.GetRequiredService<Petoria.Infrastructure.Data.ApplicationDbContext>();
        
        var totalSpent = await dbContext.Reservations
            .Where(r => r.UserId == userId)
            .SumAsync(r => 
                (r.Status == "Completed" || r.Status == "Confirmed") ? r.TotalPrice : 
                (r.Status == "Cancelled") ? r.RetainedAmount : 0
            );

        return Ok(new ProfileResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Theme = user.Theme,
            Currency = user.Currency,
            Language = user.Language,
            TotalSpent = totalSpent,
            Roles = roles.ToList()
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

        // Map DTO → Entity (update only provided fields)
        if (!string.IsNullOrEmpty(request.FirstName)) user.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName)) user.LastName = request.LastName;

        // Update preferences if provided
        if (!string.IsNullOrEmpty(request.Theme)) user.Theme = request.Theme;
        if (!string.IsNullOrEmpty(request.Currency)) user.Currency = request.Currency;
        if (!string.IsNullOrEmpty(request.Language)) user.Language = request.Language;

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
                AvatarUrl = user.AvatarUrl,
                Theme = user.Theme,
                Currency = user.Currency,
                Language = user.Language,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
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
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".avif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Only JPG, JPEG, and PNG files are allowed");
        }

        // Validate file size (max 25MB)
        if (file.Length > 25 * 1024 * 1024)
        {
            return BadRequest("File size must be less than 25MB");
        }

        try
        {
            var uploadResult = await _photoService.AddPhotoAsync(file);

            if (uploadResult.Error != null)
            {
                return BadRequest(uploadResult.Error.Message);
            }

            // Update user avatar URL
            user.AvatarUrl = uploadResult.SecureUrl.AbsoluteUri;
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
            // Note: Since we don't store the PublicId in the user entity, we might not be able to delete from Cloudinary easily 
            // unless we extract it from the URL or store it. 
            // For now, we will just clear the URL from the user profile.
            // Ideally, we should store the PublicId in the User entity.

            // Attempt to extract PublicId from URL if possible (Cloudinary URLs usually contain it)
            // But for simplicity/safety, just clear the reference for now or implementing basic extraction if standard format.
            
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
