using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

public class Hotel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerNight { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal Rating { get; set; } = 0;

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    // JSON array of additional image URLs
    [MaxLength(2000)]
    public string Images { get; set; } = "[]";

    // JSON array of amenities
    [MaxLength(1000)]
    public string Amenities { get; set; } = "[]";

    // JSON array of room types
    [MaxLength(1000)]
    public string RoomTypes { get; set; } = "[]";

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
