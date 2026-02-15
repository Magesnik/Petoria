using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.Profile;

/// <summary>
/// DTO за обновяване на потребителски профил.
/// Email и парола НЕ се обновяват тук — за тях има други endpoint-и.
/// </summary>
public class UpdateProfileDto
{
    [MaxLength(100, ErrorMessage = "Името не може да надвишава 100 символа")]
    public string? FirstName { get; set; }

    [MaxLength(100, ErrorMessage = "Фамилията не може да надвишава 100 символа")]
    public string? LastName { get; set; }
}
