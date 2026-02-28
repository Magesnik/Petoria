using Microsoft.AspNetCore.Mvc;
using Petoria.Controllers;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class DealsControllerTests : ControllerTestBase
{
    private DealsController CreateController()
    {
        var controller = new DealsController(Context);
        SetControllerUser(controller, "anyone");
        return controller;
    }

    private async Task SeedHotelWithActiveDiscount()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1", "Deal Hotel");
        var room = await SeedRoomType(hotel.Id, "Standard", 100, 5);
        Context.RoomDiscounts.Add(new RoomDiscount
        {
            RoomTypeId = room.Id,
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow.AddDays(30),
            DiscountPercentage = 20
        });
        for (int i = 0; i < 14; i++)
        {
            Context.RoomAvailabilities.Add(new RoomAvailability
            {
                RoomTypeId = room.Id,
                Date = DateTime.UtcNow.Date.AddDays(i),
                AvailableCount = 3,
                IsBlocked = false
            });
        }
        await Context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetDiscountedHotels_ReturnsOk()
    {
        await SeedHotelWithActiveDiscount();
        var controller = CreateController();

        var result = await controller.GetDiscountedHotels();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetLastMinuteDeals_ReturnsOk()
    {
        await SeedHotelWithActiveDiscount();
        var controller = CreateController();

        var result = await controller.GetLastMinuteDeals();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetSeasonalDeals_ReturnsOk()
    {
        await SeedHotelWithActiveDiscount();
        var controller = CreateController();

        var result = await controller.GetSeasonalDeals();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetPackageDeals_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner2", "Package Hotel");
        await SeedRoomType(hotel.Id, "Standard", 80, 5);
        await SeedRoomType(hotel.Id, "Deluxe", 150, 3);
        await SeedRoomType(hotel.Id, "Suite", 250, 2);
        await Context.SaveChangesAsync();
        var controller = CreateController();

        var result = await controller.GetPackageDeals();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }
}
