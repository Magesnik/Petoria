using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Petoria.Controllers;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class AdminControllerTests : ControllerTestBase
{
    private AdminController CreateController(
        string userId,
        bool demoteSetup = false,
        bool superAdminSetup = false,
        params string[] roles)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .Returns((string id) => Context.Users.FirstOrDefaultAsync(u => u.Id == id)!);

        mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        mockUserManager.Setup(m => m.Users)
            .Returns(Context.Users);

        mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(m => m.IsInRoleAsync(It.IsAny<ApplicationUser>(), "SuperAdmin"))
            .ReturnsAsync(superAdminSetup);

        if (demoteSetup)
        {
            // For DemoteFromAdmin: user IS in Admin role
            mockUserManager.Setup(m => m.IsInRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
                .ReturnsAsync(true);
        }
        else
        {
            mockUserManager.Setup(m => m.IsInRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
                .ReturnsAsync(false);
        }

        mockUserManager.Setup(m => m.GetUsersInRoleAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<ApplicationUser>());

        mockUserManager.Setup(m => m.SetLockoutEnabledAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(m => m.SetLockoutEndDateAsync(It.IsAny<ApplicationUser>(), It.IsAny<DateTimeOffset?>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AdminController(mockUserManager.Object, Context);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task GetAllUsers_ReturnsUsers()
    {
        await SeedUser("u1", "Alice", "Alison");
        await SeedUser("u2", "Bob", "Bobson");
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.GetAllUsers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetUserDetails_Exists_ReturnsDetails()
    {
        await SeedUser("u1", "Alice", "Alison");
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.GetUserDetails("u1");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetUserDetails_NotFound_ReturnsNotFound()
    {
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.GetUserDetails("nonexistent");

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task PromoteToAdmin_ReturnsOk()
    {
        await SeedUser("u1");
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.PromoteToAdmin("u1");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DemoteFromAdmin_ReturnsOk()
    {
        await SeedUser("u1");
        var controller = CreateController("admin1", demoteSetup: true, superAdminSetup: false, "SuperAdmin");

        var result = await controller.DemoteFromAdmin("u1");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetDashboardStats_ReturnsStats()
    {
        await SeedUser("u1");
        await SeedHotelWithOwner();
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.GetDashboardStats();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetAllHotelsForAdmin_ReturnsHotels()
    {
        await SeedHotelWithOwner("o1", "Hotel 1");
        await SeedHotelWithOwner("o2", "Hotel 2");
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.GetAllHotelsForAdmin();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task ToggleHotelSuspend_TogglesStatus()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        Assert.False(hotel.IsSuspendedBySuperAdmin);
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.ToggleHotelSuspend(hotel.Id);

        Assert.IsType<OkObjectResult>(result);
        var updated = Context.Hotels.First(h => h.Id == hotel.Id);
        Assert.True(updated.IsSuspendedBySuperAdmin);
    }

    [Fact]
    public async Task GetAllModerators_ReturnsModerators()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var mod = await SeedUser("mod1");
        Context.HotelModerators.Add(new HotelModerator { HotelId = hotel.Id, UserId = mod.Id });
        await Context.SaveChangesAsync();
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.GetAllModerators();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task RemoveModeratorRole_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var mod = await SeedUser("mod1");
        var moderator = new HotelModerator { HotelId = hotel.Id, UserId = mod.Id };
        Context.HotelModerators.Add(moderator);
        await Context.SaveChangesAsync();
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.RemoveModeratorRole(moderator.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task BlockUser_ValidUser_ReturnsOk()
    {
        await SeedUser("u1");
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.BlockUser("u1");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task BlockUser_SelfBlock_ReturnsBadRequest()
    {
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.BlockUser("admin1");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UnblockUser_ValidUser_ReturnsOk()
    {
        await SeedUser("u1");
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.UnblockUser("u1");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task PromoteToSuperAdmin_ValidUser_ReturnsOk()
    {
        await SeedUser("u1");
        // superAdminSetup=false: user is NOT already SuperAdmin
        var controller = CreateController("admin1", false, false, "SuperAdmin");

        var result = await controller.PromoteToSuperAdmin("u1");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DemoteFromSuperAdmin_ValidUser_ReturnsOk()
    {
        await SeedUser("u1");
        // superAdminSetup=true: the target user IS a SuperAdmin (so demotion is valid)
        var controller = CreateController("admin1", false, true, "SuperAdmin");

        var result = await controller.DemoteFromSuperAdmin("u1");

        Assert.IsType<OkObjectResult>(result);
    }
}
