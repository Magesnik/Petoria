using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.Models.Auth;

/// <summary>Заявка: имейл и парола за логин</summary>
public class LoginModel
{
    [Required(ErrorMessage = ValidationConstants.User.EmailRequired)]
    [EmailAddress(ErrorMessage = ValidationConstants.User.EmailInvalid)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = ValidationConstants.User.PasswordRequired)]
    public string Password { get; set; } = string.Empty;
}
