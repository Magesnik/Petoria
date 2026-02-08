using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

public class RoomType
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int HotelId { get; set; }

    [ForeignKey("HotelId")]
    public Hotel? Hotel { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;  // "Единична", "Двойна", "Апартамент"

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerNight { get; set; }

    [Required]
    [Range(1, 20)]
    public int Capacity { get; set; } = 2;  // Брой гости

    [Required]
    [Range(1, 1000)]
    public int TotalRooms { get; set; } = 1;  // Колко стаи от този тип има

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for availability records
    public ICollection<RoomAvailability> Availabilities { get; set; } = new List<RoomAvailability>();
    
    // Navigation property for discounts
    public ICollection<RoomDiscount> Discounts { get; set; } = new List<RoomDiscount>();
}
