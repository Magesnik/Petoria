using Microsoft.AspNetCore.Mvc;
using Petoria.Controllers;
using Petoria.Core.DTOs.Availability;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class AvailabilityControllerTests : ControllerTestBase
{
    private AvailabilityController CreateController(string userId, params string[] roles)
    {
        var controller = new AvailabilityController(Context);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task GetAvailability_ReturnsDateRange()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id, "Standard", 100, 5);
        Context.RoomAvailabilities.Add(new RoomAvailability
        {
            RoomTypeId = room.Id, Date = DateTime.UtcNow.Date, AvailableCount = 3
        });
        await Context.SaveChangesAsync();
        var controller = CreateController("anyone");

        var from = DateTime.UtcNow.Date.AddDays(-1);
        var to = DateTime.UtcNow.Date.AddDays(1);
        var result = await controller.GetAvailability(hotel.Id, from, to);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task SetBulkAvailability_CreatesRecords()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var room = await SeedRoomType(hotel.Id, "Standard", 100, 5);
        var controller = CreateController("owner1");

        var dto = new BulkAvailabilityRequestDto
        {
            RoomTypeId = room.Id,
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddDays(5),
            AvailableCount = 3
        };
        var result = await controller.SetBulkAvailability(hotel.Id, dto);

        Assert.IsType<OkObjectResult>(result);
        Assert.True(Context.RoomAvailabilities.Any(ra => ra.RoomTypeId == room.Id));
    }

    [Fact]
    public async Task BlockDates_BlocksRange()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var room = await SeedRoomType(hotel.Id, "Standard", 100, 5);
        // Pre-seed availability
        Context.RoomAvailabilities.Add(new RoomAvailability
        {
            RoomTypeId = room.Id, Date = DateTime.UtcNow.Date.AddDays(2), AvailableCount = 5
        });
        await Context.SaveChangesAsync();
        var controller = CreateController("owner1");

        var dto = new BlockDatesRequestDto
        {
            RoomTypeId = room.Id,
            StartDate = DateTime.UtcNow.Date.AddDays(2),
            EndDate = DateTime.UtcNow.Date.AddDays(2),
            IsBlocked = true
        };
        var result = await controller.BlockDates(hotel.Id, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task InitializeAvailability_CreatesForAllRoomTypes()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        await SeedRoomType(hotel.Id, "Standard", 100, 5);
        await SeedRoomType(hotel.Id, "Deluxe", 200, 3);
        var controller = CreateController("owner1");

        var result = await controller.InitializeAvailability(hotel.Id);

        Assert.IsType<OkObjectResult>(result);
    }
}
