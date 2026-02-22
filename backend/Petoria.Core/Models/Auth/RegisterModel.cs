using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.Models.Auth;

public class RegisterModel
{
    [Required(ErrorMessage = ValidationConstants.User.FirstNameRequired)]
    [MaxLength(ValidationConstants.User.FirstNameMaxLength, ErrorMessage = ValidationConstants.User.FirstNameMaxLengthError)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = ValidationConstants.User.LastNameRequired)]
    [MaxLength(ValidationConstants.User.LastNameMaxLength, ErrorMessage = ValidationConstants.User.LastNameMaxLengthError)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationConstants.User.EmailRequired)]
    [EmailAddress(ErrorMessage = ValidationConstants.User.EmailInvalid)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationConstants.User.PasswordRequired)]
    [MinLength(ValidationConstants.User.PasswordMinLength, ErrorMessage = ValidationConstants.User.PasswordMinLengthError)]
    public string Password { get; set; } = string.Empty;
}
