using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petoria.Infrastructure.Data.Entities;

public class Favorite
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    [Required]
    public int HotelId { get; set; }

    [ForeignKey("HotelId")]
    public Hotel? Hotel { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
