using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Petoria.Controllers;
using Petoria.Core.DTOs.Hotel;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class HotelsControllerTests : ControllerTestBase
{
    private HotelsController CreateController(string userId, params string[] roles)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .Returns((string id) => Context.Users.FirstOrDefaultAsync(u => u.Id == id)!);

        mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .Returns((string email) => Context.Users.FirstOrDefaultAsync(u => u.Email == email)!);

        mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>(roles));

        mockUserManager.Setup(m => m.IsInRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser _, string role) => roles.Contains(role));

        mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var httpClient = new HttpClient();
        var controller = new HotelsController(Context, httpClient, mockUserManager.Object);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task GetHotels_ReturnsOk()
    {
        await SeedHotelWithOwner("o1", "Hotel A");
        await SeedHotelWithOwner("o2", "Hotel B");
        var controller = CreateController("anyone");

        var result = await controller.GetHotels(null, null, null, null, null, null, null, null, null, null, null);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetHotel_Exists_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var controller = CreateController("anyone");

        var result = await controller.GetHotel(hotel.Id);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetHotel_NotFound_ReturnsNotFound()
    {
        var controller = CreateController("anyone");

        var result = await controller.GetHotel(9999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateHotel_ValidData_ReturnsCreated()
    {
        await SeedUser("owner1");
        var controller = CreateController("owner1", Petoria.Constants.Roles.Admin);

        var dto = new CreateHotelDto
        {
            Name = "New Hotel",
            Description = "Nice place",
            Location = "City Center",
            City = "Sofia",
            Country = "Bulgaria",
            StarRating = 4
        };

        var result = await controller.CreateHotel(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task DeleteHotel_Owner_ReturnsNoContent()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var controller = CreateController("owner1", Petoria.Constants.Roles.Admin);

        var result = await controller.DeleteHotel(hotel.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteHotel_NotOwner_ReturnsForbid()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var controller = CreateController("other_user", Petoria.Constants.Roles.Admin);

        var result = await controller.DeleteHotel(hotel.Id);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetMyHotels_ReturnsOwnedHotels()
    {
        await SeedHotelWithOwner("owner1", "My Hotel 1");
        await SeedHotelWithOwner("owner1", "My Hotel 2");
        await SeedHotelWithOwner("other", "Other Hotel");
        var controller = CreateController("owner1", Petoria.Constants.Roles.Admin);

        var result = await controller.GetMyHotels();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetCities_ReturnsCities()
    {
        await SeedHotelWithOwner("o1", "H1");
        var controller = CreateController("anyone");

        var result = await controller.GetCities();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetCountries_ReturnsCountries()
    {
        await SeedHotelWithOwner("o1", "H1");
        var controller = CreateController("anyone");

        var result = await controller.GetCountries();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task AddModerator_Owner_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var mod = await SeedUser("mod1");
        var controller = CreateController("owner1", Petoria.Constants.Roles.Admin);

        var result = await controller.AddModerator(hotel.Id, new AddModeratorDto { Email = mod.Email! });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetModerators_ReturnsModeratorList()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var mod = await SeedUser("mod1");
        Context.HotelModerators.Add(new HotelModerator { HotelId = hotel.Id, UserId = mod.Id });
        await Context.SaveChangesAsync();
        var controller = CreateController("owner1", Petoria.Constants.Roles.Admin);

        var result = await controller.GetModerators(hotel.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task RemoveModerator_Owner_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var mod = await SeedUser("mod1");
        Context.HotelModerators.Add(new HotelModerator { HotelId = hotel.Id, UserId = mod.Id });
        await Context.SaveChangesAsync();
        var controller = CreateController("owner1", Petoria.Constants.Roles.Admin);

        var result = await controller.RemoveModerator(hotel.Id, mod.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetPopularDestinations_ReturnsOk()
    {
        await SeedHotelWithOwner("o1", "H1");
        var controller = CreateController("anyone");

        var result = await controller.GetPopularDestinations();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAllAmenities_ReturnsOk()
    {
        var controller = CreateController("anyone");

        var result = await controller.GetAllAmenities();

        Assert.IsType<OkObjectResult>(result.Result);
    }
}
