using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UploadController : ControllerBase
{
    private readonly Petoria.Core.Contracts.IPhotoService _photoService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(Petoria.Core.Contracts.IPhotoService photoService, ILogger<UploadController> logger)
    {
        _photoService = photoService;
        _logger = logger;
    }

    [HttpPost("image")]
    [Authorize(Roles = Petoria.Constants.Roles.Admin)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".avif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = $"Invalid file type: '{extension}'. Allowed types: jpg, jpeg, png, gif, webp" });
            }

            // Validate file size (max 25MB)
            if (file.Length > 25 * 1024 * 1024)
            {
                return BadRequest(new { message = "File size exceeds 25MB limit" });
            }

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null)
            {
                return BadRequest(new { message = result.Error.Message });
            }

            return Ok(new { url = result.SecureUrl.AbsoluteUri, publicId = result.PublicId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
        }
    }

    [HttpDelete("image/{publicId}")]
    [Authorize(Roles = Petoria.Constants.Roles.Admin)]
    public async Task<IActionResult> DeleteImage(string publicId)
    {
        try
        {
            var result = await _photoService.DeletePhotoAsync(publicId);

            if (result.Error != null)
            {
                return BadRequest(new { message = result.Error.Message });
            }

            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, new { message = "Error deleting file", error = ex.Message });
        }
    }
}
