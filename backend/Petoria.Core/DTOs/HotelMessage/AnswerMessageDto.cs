using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.HotelMessage;

/// <summary>
/// DTO за отговор на администратор на съобщение
/// </summary>
public class AnswerMessageDto
{
    [Required(ErrorMessage = "Отговорът е задължителен")]
    [MinLength(10, ErrorMessage = "Отговорът трябва да бъде поне 10 символа")]
    [MaxLength(2000, ErrorMessage = "Отговорът не може да бъде по-дълъг от 2000 символа")]
    public string AdminResponse { get; set; } = string.Empty;
}
