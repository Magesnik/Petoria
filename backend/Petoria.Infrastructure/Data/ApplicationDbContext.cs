using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentRating> CommentRatings { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<RoomAvailability> RoomAvailabilities { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Unique index for room availability (one record per room type per date)
        builder.Entity<RoomAvailability>()
            .HasIndex(ra => new { ra.RoomTypeId, ra.Date })
            .IsUnique();
        
        // Configure Hotel -> RoomTypes relationship
        builder.Entity<RoomType>()
            .HasOne(rt => rt.Hotel)
            .WithMany()
            .HasForeignKey(rt => rt.HotelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
