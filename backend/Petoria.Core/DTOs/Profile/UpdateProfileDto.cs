using System.ComponentModel.DataAnnotations;
using Petoria.Constants;
namespace Petoria.Core.DTOs.Profile;

/// <summary>
/// DTO за обновяване на потребителски профил.
/// Email и парола НЕ се обновяват тук — за тях има други endpoint-и.
/// </summary>
public class UpdateProfileDto
{
    [MaxLength(ValidationConstants.User.FirstNameMaxLength, ErrorMessage = "Името не може да надвишава 100 символа")]
    public string? FirstName { get; set; }

    [MaxLength(ValidationConstants.User.LastNameMaxLength, ErrorMessage = "Фамилията не може да надвишава 100 символа")]
    public string? LastName { get; set; }

    public string? Theme { get; set; }
    public string? Currency { get; set; }
    public string? Language { get; set; }
}
