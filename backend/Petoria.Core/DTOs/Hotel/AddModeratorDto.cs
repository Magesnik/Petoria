using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.Hotel;

public class AddModeratorDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
