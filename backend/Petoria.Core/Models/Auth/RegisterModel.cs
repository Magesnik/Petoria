using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.Models.Auth;

public class RegisterModel
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(ValidationConstants.User.PasswordMinLength)]
    public string Password { get; set; } = string.Empty;
}
