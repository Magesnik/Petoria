using System.ComponentModel.DataAnnotations;
using Petoria.Constants;
namespace Petoria.Core.DTOs.Profile;

/// <summary>Заявка: обновяване на профила (име, фамилия, настройки)</summary>
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
