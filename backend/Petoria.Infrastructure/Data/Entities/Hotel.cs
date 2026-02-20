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

    // Geographic coordinates for map display (nullable for existing hotels without coordinates)
    [Column(TypeName = "decimal(10,7)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(10,7)")]
    public decimal? Longitude { get; set; }



    [Column(TypeName = "decimal(2,1)")]
    [Range(0, 5)]
    public decimal Rating { get; set; } = 0;

    [Range(1, 5)]
    public int StarRating { get; set; } = 3;

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

    // JSON array of cancellation policies (days before -> refund percentage)
    [MaxLength(2000)]
    public string CancellationPolicies { get; set; } = "[]";

    public bool IsAvailable { get; set; } = true;

    // Set by SuperAdmin to globally hide and lock the hotel
    public bool IsSuspendedBySuperAdmin { get; set; } = false;

    // Track who created the hotel - set by controller, not required from client
    public string CreatedById { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
