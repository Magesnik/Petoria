using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.Models.Auth;

public class RegisterModel
{
    [Required(ErrorMessage = ValidationConstants.User.FirstNameRequired)]
    [MinLength(ValidationConstants.User.FirstNameMinLength, ErrorMessage = ValidationConstants.User.FirstNameMinLengthError)]
    [MaxLength(ValidationConstants.User.FirstNameMaxLength, ErrorMessage = ValidationConstants.User.FirstNameMaxLengthError)]
    [RegularExpression(ValidationConstants.User.NameRegex, ErrorMessage = ValidationConstants.User.FirstNameRegexError)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = ValidationConstants.User.LastNameRequired)]
    [MinLength(ValidationConstants.User.LastNameMinLength, ErrorMessage = ValidationConstants.User.LastNameMinLengthError)]
    [MaxLength(ValidationConstants.User.LastNameMaxLength, ErrorMessage = ValidationConstants.User.LastNameMaxLengthError)]
    [RegularExpression(ValidationConstants.User.NameRegex, ErrorMessage = ValidationConstants.User.LastNameRegexError)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationConstants.User.EmailRequired)]
    [EmailAddress(ErrorMessage = ValidationConstants.User.EmailInvalid)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationConstants.User.PasswordRequired)]
    [MinLength(ValidationConstants.User.PasswordMinLength, ErrorMessage = ValidationConstants.User.PasswordMinLengthError)]
    [RegularExpression(ValidationConstants.User.PasswordRegex, ErrorMessage = ValidationConstants.User.PasswordRegexError)]
    public string Password { get; set; } = string.Empty;
}
