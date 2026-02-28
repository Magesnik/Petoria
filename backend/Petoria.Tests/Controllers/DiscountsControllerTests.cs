using Microsoft.AspNetCore.Mvc;
using Petoria.Controllers;
using Petoria.Core.DTOs.Discount;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class DiscountsControllerTests : ControllerTestBase
{
    private DiscountsController CreateController(string userId, params string[] roles)
    {
        var controller = new DiscountsController(Context);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task GetHotelDiscounts_ReturnsDiscounts()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        Context.RoomDiscounts.Add(new RoomDiscount
        {
            RoomTypeId = room.Id, StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow.AddDays(5), DiscountPercentage = 20
        });
        await Context.SaveChangesAsync();
        var controller = CreateController("anyone");

        var result = await controller.GetHotelDiscounts(hotel.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var discounts = Assert.IsAssignableFrom<IEnumerable<DiscountResponseDto>>(ok.Value);
        Assert.Single(discounts);
    }

    [Fact]
    public async Task GetActiveDiscount_Found_ReturnsDiscount()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        Context.RoomDiscounts.Add(new RoomDiscount
        {
            RoomTypeId = room.Id, StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(10), DiscountPercentage = 15
        });
        await Context.SaveChangesAsync();
        var controller = CreateController("anyone");

        var result = await controller.GetActiveDiscount(room.Id, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var discount = Assert.IsType<DiscountResponseDto>(ok.Value);
        Assert.Equal(15, discount.DiscountPercentage);
    }

    [Fact]
    public async Task GetActiveDiscount_NotFound_ReturnsNotFound()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        var controller = CreateController("anyone");

        var result = await controller.GetActiveDiscount(room.Id, null);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateDiscount_ValidData_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var room = await SeedRoomType(hotel.Id);
        var controller = CreateController("owner1");

        var dto = new CreateDiscountDto
        {
            RoomTypeId = room.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(10),
            DiscountPercentage = 25
        };

        var result = await controller.CreateDiscount(hotel.Id, dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateDiscount_NotOwner_ReturnsForbid()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var room = await SeedRoomType(hotel.Id);
        var controller = CreateController("other_user");

        var dto = new CreateDiscountDto
        {
            RoomTypeId = room.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(10),
            DiscountPercentage = 25
        };

        var result = await controller.CreateDiscount(hotel.Id, dto);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task CreateDiscount_InvalidDates_ReturnsBadRequest()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var room = await SeedRoomType(hotel.Id);
        var controller = CreateController("owner1");

        var dto = new CreateDiscountDto
        {
            RoomTypeId = room.Id,
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(1), // end before start
            DiscountPercentage = 25
        };

        var result = await controller.CreateDiscount(hotel.Id, dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateDiscount_ValidData_ReturnsNoContent()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var room = await SeedRoomType(hotel.Id);
        var discount = new RoomDiscount
        {
            RoomTypeId = room.Id, StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(5), DiscountPercentage = 10
        };
        Context.RoomDiscounts.Add(discount);
        await Context.SaveChangesAsync();
        var controller = CreateController("owner1");

        var dto = new UpdateDiscountDto
        {
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(15), DiscountPercentage = 30
        };

        var result = await controller.UpdateDiscount(discount.Id, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteDiscount_Exists_ReturnsNoContent()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var room = await SeedRoomType(hotel.Id);
        var discount = new RoomDiscount
        {
            RoomTypeId = room.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), DiscountPercentage = 10
        };
        Context.RoomDiscounts.Add(discount);
        await Context.SaveChangesAsync();
        var controller = CreateController("owner1");

        var result = await controller.DeleteDiscount(discount.Id);

        Assert.IsType<NoContentResult>(result);
    }
}
