using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;
using System.Security.Claims;

namespace Petoria.Tests.Helpers;

/// <summary>
/// Base class that provides InMemory DbContext and user claim helpers for controller tests.
/// </summary>
public abstract class ControllerTestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;
    private readonly string _dbName;

    protected ControllerTestBase()
    {
        _dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _dbName)
            .Options;
        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Creates a ClaimsPrincipal with the given userId and roles.
    /// </summary>
    protected static ClaimsPrincipal CreateUser(string userId, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, $"{userId}@test.com"),
            new Claim(ClaimTypes.Name, $"{userId}@test.com")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    /// <summary>
    /// Sets ClaimsPrincipal and a mock HttpContext (with Response.Cookies) on a controller.
    /// </summary>
    protected static void SetControllerUser(ControllerBase controller, string userId, params string[] roles)
    {
        var user = CreateUser(userId, roles);
        var httpContext = new DefaultHttpContext { User = user };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    /// <summary>
    /// Seeds a hotel and its owner user into the in-memory database.
    /// </summary>
    protected async Task<(Hotel hotel, ApplicationUser owner)> SeedHotelWithOwner(
        string ownerId = "owner1",
        string hotelName = "Test Hotel",
        int hotelId = 0)
    {
        var owner = new ApplicationUser
        {
            Id = ownerId,
            UserName = $"{ownerId}@test.com",
            Email = $"{ownerId}@test.com",
            FirstName = "Test",
            LastName = "Owner"
        };

        if (!await Context.Users.AnyAsync(u => u.Id == ownerId))
        {
            Context.Users.Add(owner);
        }
        else
        {
            owner = await Context.Users.FirstAsync(u => u.Id == ownerId);
        }

        var hotel = new Hotel
        {
            Name = hotelName,
            Location = "Test Location",
            City = "TestCity",
            Country = "TestCountry",
            CreatedById = ownerId,
            Description = "Test Description"
        };

        if (hotelId > 0) hotel.Id = hotelId;

        Context.Hotels.Add(hotel);
        await Context.SaveChangesAsync();

        return (hotel, owner);
    }

    /// <summary>
    /// Seeds a RoomType for a hotel.
    /// </summary>
    protected async Task<RoomType> SeedRoomType(int hotelId, string name = "Standard", decimal price = 100m, int totalRooms = 5)
    {
        var roomType = new RoomType
        {
            HotelId = hotelId,
            Name = name,
            Description = "Test room type",
            PricePerNight = price,
            Capacity = 2,
            TotalRooms = totalRooms,
        };
        Context.RoomTypes.Add(roomType);
        await Context.SaveChangesAsync();
        return roomType;
    }

    /// <summary>
    /// Seeds an ApplicationUser helper.
    /// </summary>
    protected async Task<ApplicationUser> SeedUser(string userId = "user1", string firstName = "Test", string lastName = "User")
    {
        var existing = await Context.Users.FindAsync(userId);
        if (existing != null) return existing;

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = $"{userId}@test.com",
            Email = $"{userId}@test.com",
            FirstName = firstName,
            LastName = lastName
        };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return user;
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
