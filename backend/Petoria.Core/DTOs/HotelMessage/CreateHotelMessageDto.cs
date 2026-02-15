using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.HotelMessage;

/// <summary>
/// DTO за създаване на ново съобщение към хотел
/// </summary>
public class CreateHotelMessageDto
{
    [Required(ErrorMessage = "Темата е задължителна")]
    [MaxLength(200, ErrorMessage = "Темата не може да бъде по-дълга от 200 символа")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Съобщението е задължително")]
    [MinLength(10, ErrorMessage = "Съобщението трябва да бъде поне 10 символа")]
    [MaxLength(2000, ErrorMessage = "Съобщението не може да бъде по-дълго от 2000 символа")]
    public string Message { get; set; } = string.Empty;
}
