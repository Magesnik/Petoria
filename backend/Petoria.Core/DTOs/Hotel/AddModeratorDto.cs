using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.Hotel;

/// <summary>Заявка: добавяне на модератор към хотел по имейл</summary>
public class AddModeratorDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
