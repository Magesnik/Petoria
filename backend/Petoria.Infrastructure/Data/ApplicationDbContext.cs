using Microsoft.AspNetCore.Identity;
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
    public DbSet<HotelReview> HotelReviews { get; set; }
    public DbSet<RoomDiscount> RoomDiscounts { get; set; }
    public DbSet<HotelMessage> HotelMessages { get; set; }
    public DbSet<SupportMessage> SupportMessages { get; set; }
    public DbSet<HotelModerator> HotelModerators { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Fix for MySQL key length issue with ASP.NET Core Identity (utf8mb4 limit 767 bytes -> 191 chars)
        builder.Entity<ApplicationUser>(entity => entity.Property(m => m.Id).HasMaxLength(191));
        builder.Entity<ApplicationUser>(entity => entity.Property(m => m.NormalizedEmail).HasMaxLength(191));
        builder.Entity<ApplicationUser>(entity => entity.Property(m => m.NormalizedUserName).HasMaxLength(191));

        builder.Entity<IdentityRole>(entity => entity.Property(m => m.Id).HasMaxLength(191));
        builder.Entity<IdentityRole>(entity => entity.Property(m => m.NormalizedName).HasMaxLength(191));

        builder.Entity<IdentityUserLogin<string>>(entity => entity.Property(m => m.LoginProvider).HasMaxLength(191));
        builder.Entity<IdentityUserLogin<string>>(entity => entity.Property(m => m.ProviderKey).HasMaxLength(191));
        builder.Entity<IdentityUserLogin<string>>(entity => entity.Property(m => m.UserId).HasMaxLength(191));

        builder.Entity<IdentityUserRole<string>>(entity => entity.Property(m => m.UserId).HasMaxLength(191));
        builder.Entity<IdentityUserRole<string>>(entity => entity.Property(m => m.RoleId).HasMaxLength(191));

        builder.Entity<IdentityUserToken<string>>(entity => entity.Property(m => m.UserId).HasMaxLength(191));
        builder.Entity<IdentityUserToken<string>>(entity => entity.Property(m => m.LoginProvider).HasMaxLength(191));
        builder.Entity<IdentityUserToken<string>>(entity => entity.Property(m => m.Name).HasMaxLength(191));

        builder.Entity<IdentityUserClaim<string>>(entity => entity.Property(m => m.UserId).HasMaxLength(191));
        builder.Entity<IdentityRoleClaim<string>>(entity => entity.Property(m => m.RoleId).HasMaxLength(191));
        
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
        
        // Configure RoomType -> RoomDiscounts relationship
        builder.Entity<RoomDiscount>()
            .HasOne(rd => rd.RoomType)
            .WithMany(rt => rt.Discounts)
            .HasForeignKey(rd => rd.RoomTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Add index for efficient discount queries
        builder.Entity<RoomDiscount>()
            .HasIndex(rd => new { rd.RoomTypeId, rd.StartDate, rd.EndDate });

        // Ensure unique moderator per hotel
        builder.Entity<HotelModerator>()
            .HasIndex(hm => new { hm.HotelId, hm.UserId })
            .IsUnique();

        // Configure PromoCode
        builder.Entity<PromoCode>()
            .HasIndex(p => new { p.HotelId, p.Code })
            .IsUnique(); // Unique code per hotel

        builder.Entity<PromoCode>()
            .Property(p => p.DiscountPercentage)
            .HasColumnType("decimal(5,2)");
    }
}
