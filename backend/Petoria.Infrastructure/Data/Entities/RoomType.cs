using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Petoria.Constants;
using Petoria.Infrastructure.Data.Entities;

public class RoomType
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int HotelId { get; set; }

    [ForeignKey("HotelId")]
    public Hotel? Hotel { get; set; }

    [Required]
    [MaxLength(ValidationConstants.RoomType.NameMaxLength)]
    public string Name { get; set; } = string.Empty;  // "Единична", "Двойна", "Апартамент"

    [MaxLength(ValidationConstants.RoomType.DescriptionMaxLength)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerNight { get; set; }

    [Required]
    [Range(ValidationConstants.RoomType.CapacityMin, ValidationConstants.RoomType.CapacityMax)]
    public int Capacity { get; set; } = 2;  // Брой гости

    [Required]
    [Range(ValidationConstants.RoomType.TotalRoomsMin, ValidationConstants.RoomType.TotalRoomsMax)]
    public int TotalRooms { get; set; } = 1;  // Колко стаи от този тип има

    [MaxLength(ValidationConstants.RoomType.ImageUrlMaxLength)]
    public string ImageUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for availability records
    public ICollection<RoomAvailability> Availabilities { get; set; } = new List<RoomAvailability>();
    
    // Navigation property for discounts
    public ICollection<RoomDiscount> Discounts { get; set; } = new List<RoomDiscount>();
}
