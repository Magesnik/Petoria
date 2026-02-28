using Microsoft.AspNetCore.Mvc;
using Petoria.Controllers;
using Petoria.Core.DTOs.Room;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class RoomsControllerTests : ControllerTestBase
{
    private RoomsController CreateController(string userId, params string[] roles)
    {
        var controller = new RoomsController(Context);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task GetRoomTypes_ReturnsRooms()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedRoomType(hotel.Id, "Standard");
        await SeedRoomType(hotel.Id, "Deluxe", 200);
        var controller = CreateController("anyone");

        var result = await controller.GetRoomTypes(hotel.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var rooms = Assert.IsAssignableFrom<IEnumerable<RoomTypeResponseDto>>(ok.Value);
        Assert.Equal(2, rooms.Count());
    }

    [Fact]
    public async Task GetRoomTypes_HotelNotFound_ReturnsNotFound()
    {
        var controller = CreateController("anyone");

        var result = await controller.GetRoomTypes(9999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetRoomType_Exists_ReturnsRoom()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        var controller = CreateController("anyone");

        var result = await controller.GetRoomType(hotel.Id, room.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<RoomTypeResponseDto>(ok.Value);
        Assert.Equal("Standard", dto.Name);
    }

    [Fact]
    public async Task GetRoomType_NotFound_ReturnsNotFound()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var controller = CreateController("anyone");

        var result = await controller.GetRoomType(hotel.Id, 9999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateRoomType_Owner_ReturnsCreated()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var controller = CreateController("owner1", Petoria.Constants.Roles.Admin);

        var dto = new CreateRoomTypeDto
        {
            Name = "Suite", Description = "Luxury", PricePerNight = 300,
            Capacity = 4, TotalRooms = 3
        };

        var result = await controller.CreateRoomType(hotel.Id, dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task CreateRoomType_NotOwner_ReturnsForbid()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var controller = CreateController("other_user", Petoria.Constants.Roles.Admin);

        var dto = new CreateRoomTypeDto
        {
            Name = "Suite", Description = "Luxury", PricePerNight = 300,
            Capacity = 4, TotalRooms = 3
        };

        var result = await controller.CreateRoomType(hotel.Id, dto);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task UpdateRoomType_Owner_ReturnsNoContent()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var room = await SeedRoomType(hotel.Id);
        var controller = CreateController("owner1", Petoria.Constants.Roles.Admin);

        var dto = new UpdateRoomTypeDto
        {
            Name = "Updated", Description = "Updated desc", PricePerNight = 150,
            Capacity = 3, TotalRooms = 4
        };

        var result = await controller.UpdateRoomType(hotel.Id, room.Id, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteRoomType_Owner_ReturnsNoContent()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var room = await SeedRoomType(hotel.Id);
        var controller = CreateController("owner1", Petoria.Constants.Roles.Admin);

        var result = await controller.DeleteRoomType(hotel.Id, room.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(Context.RoomTypes.Where(rt => rt.Id == room.Id));
    }
}
