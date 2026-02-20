using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

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

    public int MaxActivations { get; set; }
    
    public int CurrentActivations { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpirationDate { get; set; }

    public bool IsActive => CurrentActivations < MaxActivations && DateTime.UtcNow <= ExpirationDate;
}
