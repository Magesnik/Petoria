using System.ComponentModel.DataAnnotations;
using Petoria.Constants;

namespace Petoria.Core.DTOs.HotelMessage;

/// <summary>Заявка: ново съобщение до хотел от гост</summary>
public class CreateHotelMessageDto
{
    [Required(ErrorMessage = "Темата е задължителна")]
    [MaxLength(ValidationConstants.HotelMessage.SubjectMaxLength, ErrorMessage = "Темата не може да бъде по-дълга от 200 символа")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Съобщението е задължително")]
    [MinLength(ValidationConstants.HotelMessage.MessageMinLength, ErrorMessage = "Съобщението трябва да бъде поне 10 символа")]
    [MaxLength(ValidationConstants.HotelMessage.MessageMaxLength, ErrorMessage = "Съобщението не може да бъде по-дълго от 2000 символа")]
    public string Message { get; set; } = string.Empty;
}
