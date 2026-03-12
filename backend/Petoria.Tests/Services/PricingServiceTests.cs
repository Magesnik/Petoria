using Microsoft.EntityFrameworkCore;
using Petoria.Core.Services;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Tests.Services;

public class PricingServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PricingService _service;

    public PricingServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        _service = new PricingService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<(Hotel hotel, RoomType roomType)> SeedHotelAndRoom(
        string ownerId = "owner1", decimal price = 100m, int totalRooms = 5)
    {
        var owner = new ApplicationUser
        {
            Id = ownerId,
            UserName = $"{ownerId}@test.com",
            Email = $"{ownerId}@test.com",
            FirstName = "Test",
            LastName = "Owner"
        };
        if (!await _context.Users.AnyAsync(u => u.Id == ownerId))
            _context.Users.Add(owner);

        var hotel = new Hotel
        {
            Name = "Test Hotel",
            Location = "Test Location",
            City = "TestCity",
            Country = "TestCountry",
            CreatedById = ownerId,
            Description = "Test"
        };
        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var roomType = new RoomType
        {
            HotelId = hotel.Id,
            Name = "Standard",
            Description = "Standard room",
            PricePerNight = price,
            Capacity = 2,
            TotalRooms = totalRooms
        };
        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();

        return (hotel, roomType);
    }

    [Fact]
    public async Task CalculatePrice_BasicTwoNights_ReturnsTotalCorrectly()
    {
        var (_, roomType) = await SeedHotelAndRoom(price: 100m);
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(2);

        var result = await _service.CalculatePriceAsync(roomType.Id, checkIn, checkOut, 1);

        Assert.Equal(2, result.NumberOfNights);
        Assert.Equal(200m, result.TotalPrice);
        Assert.Equal(100m, result.PricePerNight);
        Assert.Equal(2, result.Breakdown.Count);
    }

    [Fact]
    public async Task CalculatePrice_WithActiveDiscount_AppliesDiscountPercentage()
    {
        var (_, roomType) = await SeedHotelAndRoom(price: 100m);
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(2);

        _context.RoomDiscounts.Add(new RoomDiscount
        {
            RoomTypeId = roomType.Id,
            StartDate = checkIn.AddDays(-1),
            EndDate = checkOut.AddDays(1),
            DiscountPercentage = 20
        });
        await _context.SaveChangesAsync();

        var result = await _service.CalculatePriceAsync(roomType.Id, checkIn, checkOut, 1);

        // $100 * 0.80 * 2 nights = $160
        Assert.Equal(160m, result.TotalPrice);
        Assert.Equal(200m, result.OriginalPrice);
        Assert.Equal(40m, result.TotalDiscount);
        Assert.All(result.Breakdown, day => Assert.Equal(20, day.DiscountPercentage));
    }

    [Fact]
    public async Task CalculatePrice_LastMinuteBooking_AppliesFivePercentDiscount()
    {
        var (_, roomType) = await SeedHotelAndRoom(price: 100m, totalRooms: 5);
        var today = DateTime.UtcNow.Date;

        // Seed availability for today - rooms are available (not blocked)
        _context.RoomAvailabilities.Add(new RoomAvailability
        {
            RoomTypeId = roomType.Id,
            Date = today,
            AvailableCount = 5,
            IsBlocked = false
        });
        await _context.SaveChangesAsync();

        var result = await _service.CalculatePriceAsync(roomType.Id, today, today.AddDays(1), 1);

        // $100 * 0.95 = $95 for 1 night
        Assert.Equal(95m, result.TotalPrice);
        Assert.Equal(5, result.Breakdown.First().DiscountPercentage);
    }

    [Fact]
    public async Task CalculatePrice_LastMinuteButBlocked_NoLastMinuteDiscount()
    {
        var (_, roomType) = await SeedHotelAndRoom(price: 100m);
        var today = DateTime.UtcNow.Date;

        // Room is blocked for today - last-minute discount should NOT apply
        _context.RoomAvailabilities.Add(new RoomAvailability
        {
            RoomTypeId = roomType.Id,
            Date = today,
            AvailableCount = 5,
            IsBlocked = true
        });
        await _context.SaveChangesAsync();

        var result = await _service.CalculatePriceAsync(roomType.Id, today, today.AddDays(1), 1);

        // Full price, no discount
        Assert.Equal(100m, result.TotalPrice);
        Assert.Null(result.Breakdown.First().DiscountPercentage);
    }

    [Fact]
    public async Task CalculatePrice_OwnerUser_ReturnsFree()
    {
        var (hotel, roomType) = await SeedHotelAndRoom(ownerId: "owner1", price: 100m);
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(2);

        var result = await _service.CalculatePriceAsync(roomType.Id, checkIn, checkOut, 1, "owner1");

        Assert.Equal(0m, result.TotalPrice);
    }

    [Fact]
    public async Task CalculatePrice_ModeratorUser_ReturnsFree()
    {
        var (hotel, roomType) = await SeedHotelAndRoom(price: 100m);
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(2);

        // Seed moderator
        var mod = new ApplicationUser
        {
            Id = "mod1",
            UserName = "mod1@test.com",
            Email = "mod1@test.com",
            FirstName = "Mod",
            LastName = "User"
        };
        _context.Users.Add(mod);
        _context.HotelModerators.Add(new HotelModerator { HotelId = hotel.Id, UserId = "mod1" });
        await _context.SaveChangesAsync();

        var result = await _service.CalculatePriceAsync(roomType.Id, checkIn, checkOut, 1, "mod1");

        Assert.Equal(0m, result.TotalPrice);
    }
}
