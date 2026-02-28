using Microsoft.AspNetCore.Mvc;
using Petoria.Controllers;
using Petoria.Core.DTOs.Favorite;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class FavoritesControllerTests : ControllerTestBase
{
    private FavoritesController CreateController(string userId)
    {
        var controller = new FavoritesController(Context);
        SetControllerUser(controller, userId);
        return controller;
    }

    [Fact]
    public async Task GetUserFavorites_ReturnsFavorites()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedRoomType(hotel.Id);
        Context.Favorites.Add(new Petoria.Infrastructure.Data.Entities.Favorite { UserId = "user1", HotelId = hotel.Id });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.GetUserFavorites();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var favs = Assert.IsAssignableFrom<IEnumerable<FavoriteResponseDto>>(ok.Value);
        Assert.Single(favs);
    }

    [Fact]
    public async Task GetUserFavoriteIds_ReturnsIds()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        Context.Favorites.Add(new Petoria.Infrastructure.Data.Entities.Favorite { UserId = "user1", HotelId = hotel.Id });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.GetUserFavoriteIds();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var ids = Assert.IsAssignableFrom<IEnumerable<int>>(ok.Value);
        Assert.Contains(hotel.Id, ids);
    }

    [Fact]
    public async Task AddFavorite_NewHotel_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var controller = CreateController("user1");

        var result = await controller.AddFavorite(hotel.Id);

        Assert.IsType<OkObjectResult>(result);
        Assert.True(Context.Favorites.Any(f => f.UserId == "user1" && f.HotelId == hotel.Id));
    }

    [Fact]
    public async Task AddFavorite_AlreadyFavorited_ReturnsBadRequest()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        Context.Favorites.Add(new Petoria.Infrastructure.Data.Entities.Favorite { UserId = "user1", HotelId = hotel.Id });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.AddFavorite(hotel.Id);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddFavorite_HotelNotFound_ReturnsNotFound()
    {
        var controller = CreateController("user1");

        var result = await controller.AddFavorite(9999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RemoveFavorite_Exists_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        Context.Favorites.Add(new Petoria.Infrastructure.Data.Entities.Favorite { UserId = "user1", HotelId = hotel.Id });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.RemoveFavorite(hotel.Id);

        Assert.IsType<OkObjectResult>(result);
        Assert.False(Context.Favorites.Any(f => f.UserId == "user1" && f.HotelId == hotel.Id));
    }

    [Fact]
    public async Task RemoveFavorite_NotFound_ReturnsNotFound()
    {
        var controller = CreateController("user1");

        var result = await controller.RemoveFavorite(9999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ToggleFavorite_AddsWhenNotExists()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var controller = CreateController("user1");

        var result = await controller.ToggleFavorite(hotel.Id);

        Assert.IsType<OkObjectResult>(result);
        Assert.True(Context.Favorites.Any(f => f.UserId == "user1" && f.HotelId == hotel.Id));
    }

    [Fact]
    public async Task ToggleFavorite_RemovesWhenExists()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        Context.Favorites.Add(new Petoria.Infrastructure.Data.Entities.Favorite { UserId = "user1", HotelId = hotel.Id });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.ToggleFavorite(hotel.Id);

        Assert.IsType<OkObjectResult>(result);
        Assert.False(Context.Favorites.Any(f => f.UserId == "user1" && f.HotelId == hotel.Id));
    }

    [Fact]
    public async Task CheckFavorite_ReturnsFalseWhenNotFavorited()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var controller = CreateController("user1");

        var result = await controller.CheckFavorite(hotel.Id);

        Assert.IsType<OkObjectResult>(result);
    }
}
