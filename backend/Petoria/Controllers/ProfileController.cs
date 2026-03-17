using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Profile;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

/// <summary>
/// Контролер за потребителски профил: преглед, редакция, качване/изтриване на аватар
/// </summary>
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

    /// <summary>
    /// Връща профила на текущия потребител или null ако не е автентикиран
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ProfileResponseDto>> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            // Връща 200 OK с null, за да избегне 401 грешки в конзолата на браузъра
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

        // Преобразуване на Entity → Response DTO
        var roles = await _userManager.GetRolesAsync(user);

        // Достъп до контекста на базата данни за изчисляване на общата похарчена сума
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

    /// <summary>
    /// Обновява профилните данни на текущия потребител
    /// </summary>
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

        // Преобразуване на DTO → Entity (обновяване само на подадените полета)
        if (!string.IsNullOrEmpty(request.FirstName)) user.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName)) user.LastName = request.LastName;

        // Обновяване на предпочитанията, ако са подадени
        if (!string.IsNullOrEmpty(request.Theme)) user.Theme = request.Theme;
        if (!string.IsNullOrEmpty(request.Currency)) user.Currency = request.Currency;
        if (!string.IsNullOrEmpty(request.Language)) user.Language = request.Language;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // Преобразуване на Entity → Response DTO
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

    /// <summary>
    /// Качва нов аватар за текущия потребител
    /// </summary>
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

        // Валидиране на типа на файла
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".avif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Only JPG, JPEG, and PNG files are allowed");
        }

        // Валидиране на размера на файла (макс. 10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest("File size must be less than 10MB");
        }

        try
        {
            var uploadResult = await _photoService.AddPhotoAsync(file);

            if (uploadResult.Error != null)
            {
                return BadRequest(uploadResult.Error.Message);
            }

            // Обновяване на URL адреса на аватара
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
            return StatusCode(500, new { message = "Error uploading avatar" });
        }
    }

    /// <summary>
    /// Изтрива аватара на текущия потребител
    /// </summary>
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
            // Забележка: Не съхраняваме PublicId в потребителската entity,
            // затова засега просто изчистваме URL от профила.

            // Премахване на URL адреса на аватара от потребителя
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
            return StatusCode(500, new { message = "Error deleting avatar" });
        }
    }
}
