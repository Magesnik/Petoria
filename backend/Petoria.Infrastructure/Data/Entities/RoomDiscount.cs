using System.ComponentModel.DataAnnotations;
using Petoria.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Отстъпка за тип стая за период от дати.
/// </summary>
public class RoomDiscount
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RoomTypeId { get; set; }

    [ForeignKey("RoomTypeId")]
    public RoomType? RoomType { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Процент отстъпка за периода.
    /// </summary>
    [Required]
    [Range(ValidationConstants.Discount.PercentageMin, ValidationConstants.Discount.PercentageMax)]
    public int DiscountPercentage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
