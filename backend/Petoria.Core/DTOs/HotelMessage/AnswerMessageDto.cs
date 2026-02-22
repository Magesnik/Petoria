using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.HotelMessage;

/// <summary>
/// DTO за отговор на администратор на съобщение
/// </summary>
public class AnswerMessageDto
{
    [Required(ErrorMessage = "Отговорът е задължителен")]
    [MinLength(ValidationConstants.HotelMessage.AnswerMinLength, ErrorMessage = "Отговорът трябва да бъде поне 10 символа")]
    [MaxLength(ValidationConstants.HotelMessage.AnswerMaxLength, ErrorMessage = "Отговорът не може да бъде по-дълъг от 2000 символа")]
    public string AdminResponse { get; set; } = string.Empty;
}
