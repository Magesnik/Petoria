using System.ComponentModel.DataAnnotations;
using Petoria.Constants;
namespace Petoria.Core.DTOs.Profile;

/// <summary>
/// DTO за обновяване на потребителски профил.
/// Email и парола НЕ се обновяват тук — за тях има други endpoint-и.
/// </summary>
public class UpdateProfileDto
{
    [MaxLength(ValidationConstants.User.FirstNameMaxLength, ErrorMessage = ValidationConstants.User.FirstNameMaxLengthError)]
    public string? FirstName { get; set; }

    [MaxLength(ValidationConstants.User.LastNameMaxLength, ErrorMessage = ValidationConstants.User.LastNameMaxLengthError)]
    public string? LastName { get; set; }

    public string? Theme { get; set; }
    public string? Currency { get; set; }
    public string? Language { get; set; }
}
