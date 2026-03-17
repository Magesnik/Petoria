using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

/// <summary>
/// Промо код: процент отстъпка, максимум активации, валидност.
/// </summary>
public class PromoCode
{
    public int Id { get; set; }

    public int? HotelId { get; set; }
    [ForeignKey("HotelId")]
    public virtual Hotel? Hotel { get; set; }

    [Required]
    public string Code { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Максимален брой активации на кода.
    /// </summary>
    public int MaxActivations { get; set; }

    /// <summary>
    /// Текущ брой използвания.
    /// </summary>
    public int CurrentActivations { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Дали кодът е все още активен (не е изчерпан и не е изтекъл).
    /// </summary>
    public bool IsActive => CurrentActivations < MaxActivations && DateTime.UtcNow <= ExpirationDate;
}
