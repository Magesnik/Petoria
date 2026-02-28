using Microsoft.AspNetCore.Mvc;
using Petoria.Controllers;
using Petoria.Core.DTOs.Cart;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class CartControllerTests : ControllerTestBase
{
    private CartController CreateController(string userId)
    {
        var controller = new CartController(Context);
        SetControllerUser(controller, userId);
        return controller;
    }

    [Fact]
    public async Task GetCart_ReturnsUserCartItems()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        await SeedUser("user1");
        Context.CartItems.Add(new Petoria.Infrastructure.Data.Entities.CartItem
        {
            UserId = "user1", HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(3),
            NumberOfRooms = 1, TotalPrice = 200, OriginalPrice = 200
        });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.GetCart();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<CartItemResponseDto>>(ok.Value);
        Assert.Single(items);
    }

    [Fact]
    public async Task GetCart_NoUser_ReturnsUnauthorized()
    {
        var controller = new CartController(Context);
        SetControllerUser(controller, ""); // empty = no user
        // Override the user with empty claim
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal();

        var result = await controller.GetCart();

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task AddToCart_ValidItem_ReturnsCreated()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        var controller = CreateController("user1");

        var dto = new AddToCartDto
        {
            HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(3),
            NumberOfRooms = 1, TotalPrice = 200, OriginalPrice = 200
        };

        var result = await controller.AddToCart(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task AddToCart_InvalidDates_ReturnsBadRequest()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        var controller = CreateController("user1");

        var dto = new AddToCartDto
        {
            HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(3), CheckOutDate = DateTime.Today.AddDays(1), // invalid
            NumberOfRooms = 1, TotalPrice = 200, OriginalPrice = 200
        };

        var result = await controller.AddToCart(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task AddToCart_HotelNotFound_ReturnsNotFound()
    {
        var controller = CreateController("user1");
        var dto = new AddToCartDto
        {
            HotelId = 9999, RoomTypeId = 1,
            CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(3),
            NumberOfRooms = 1, TotalPrice = 100, OriginalPrice = 100
        };

        var result = await controller.AddToCart(dto);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task RemoveFromCart_OwnItem_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        var item = new Petoria.Infrastructure.Data.Entities.CartItem
        {
            UserId = "user1", HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(3),
            NumberOfRooms = 1, TotalPrice = 200, OriginalPrice = 200
        };
        Context.CartItems.Add(item);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.RemoveFromCart(item.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RemoveFromCart_OtherUserItem_ReturnsForbid()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        var item = new Petoria.Infrastructure.Data.Entities.CartItem
        {
            UserId = "other_user", HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(3),
            NumberOfRooms = 1, TotalPrice = 200, OriginalPrice = 200
        };
        Context.CartItems.Add(item);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.RemoveFromCart(item.Id);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task ClearCart_RemovesAllItems()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        Context.CartItems.AddRange(
            new Petoria.Infrastructure.Data.Entities.CartItem { UserId = "user1", HotelId = hotel.Id, RoomTypeId = room.Id, CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(2), TotalPrice = 100, OriginalPrice = 100 },
            new Petoria.Infrastructure.Data.Entities.CartItem { UserId = "user1", HotelId = hotel.Id, RoomTypeId = room.Id, CheckInDate = DateTime.Today.AddDays(3), CheckOutDate = DateTime.Today.AddDays(4), TotalPrice = 100, OriginalPrice = 100 }
        );
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.ClearCart();

        Assert.IsType<OkObjectResult>(result);
        Assert.Empty(Context.CartItems.Where(c => c.UserId == "user1"));
    }

    [Fact]
    public async Task GetCartCount_ReturnsCount()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        Context.CartItems.Add(new Petoria.Infrastructure.Data.Entities.CartItem
        {
            UserId = "user1", HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(2),
            TotalPrice = 100, OriginalPrice = 100
        });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.GetCartCount();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(1, (int)ok.Value!);
    }
}
