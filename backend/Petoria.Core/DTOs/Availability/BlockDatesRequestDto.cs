using System.ComponentModel.DataAnnotations;

namespace Petoria.Core.DTOs.Availability;

/// <summary>Заявка: блокиране/отблокиране на дати за стая</summary>
public class BlockDatesRequestDto
{
    /// <summary>
    /// Ако е null — блокира/отблокира всички типове стаи.
    /// </summary>
    public int? RoomTypeId { get; set; }

    [Required(ErrorMessage = "Началната дата е задължителна")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Крайната дата е задължителна")]
    public DateTime EndDate { get; set; }

    [Required]
    public bool IsBlocked { get; set; }
}
