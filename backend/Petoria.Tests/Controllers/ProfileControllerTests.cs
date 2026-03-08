using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Petoria.Controllers;
using Petoria.Core.Contracts;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;
using CloudinaryDotNet.Actions;

namespace Petoria.Tests.Controllers;

public class ProfileControllerTests : ControllerTestBase
{
    private readonly Mock<IPhotoService> _mockPhotoService;
    private Mock<UserManager<ApplicationUser>> _mockUserManager = null!;

    public ProfileControllerTests()
    {
        _mockPhotoService = new Mock<IPhotoService>();
    }

    private ProfileController CreateController(string userId, params string[] roles)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .Returns((string id) => Context.Users.FirstOrDefaultAsync(u => u.Id == id)!);

        _mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>(roles));

        _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new ProfileController(_mockUserManager.Object, _mockPhotoService.Object);

        // Set up HttpContext with RequestServices containing ApplicationDbContext
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Context);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        SetControllerUser(controller, userId, roles);
        controller.ControllerContext.HttpContext.RequestServices = serviceProvider;
        return controller;
    }

    [Fact]
    public async Task GetProfile_Authenticated_ReturnsOk()
    {
        await SeedUser("user1", "Alice", "Alison");
        var controller = CreateController("user1");

        var result = await controller.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetProfile_UserNotFound_ReturnsOk()
    {
        // ProfileController returns Ok(null) for non-existent users (to avoid 401 browser errors)
        var controller = CreateController("nonexistent");

        var result = await controller.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(ok.Value);
    }

    [Fact]
    public async Task UpdateProfile_ValidData_ReturnsOk()
    {
        await SeedUser("user1");
        var controller = CreateController("user1");

        var dto = new Petoria.Core.DTOs.Profile.UpdateProfileDto
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        var result = await controller.UpdateProfile(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UploadAvatar_ValidFile_ReturnsOk()
    {
        await SeedUser("user1");
        _mockPhotoService.Setup(s => s.AddPhotoAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()))
            .ReturnsAsync(new ImageUploadResult
            {
                SecureUrl = new Uri("https://example.com/avatar.jpg"),
                PublicId = "avatar_123"
            });

        var controller = CreateController("user1");
        var file = CreateMockFile("avatar.jpg", "image/jpeg", 1024);

        var result = await controller.UploadAvatar(file);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UploadAvatar_InvalidType_ReturnsBadRequest()
    {
        await SeedUser("user1");
        var controller = CreateController("user1");
        var file = CreateMockFile("virus.exe", "application/exe", 1024);

        var result = await controller.UploadAvatar(file);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadAvatar_TooLarge_ReturnsBadRequest()
    {
        await SeedUser("user1");
        var controller = CreateController("user1");
        var file = CreateMockFile("big.jpg", "image/jpeg", 26 * 1024 * 1024); // 26MB

        var result = await controller.UploadAvatar(file);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteAvatar_HasAvatar_ReturnsOk()
    {
        var user = await SeedUser("user1");
        user.AvatarUrl = "https://example.com/old.jpg";
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.DeleteAvatar();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteAvatar_NoAvatar_ReturnsBadRequest()
    {
        await SeedUser("user1");
        var controller = CreateController("user1");

        var result = await controller.DeleteAvatar();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static Microsoft.AspNetCore.Http.IFormFile CreateMockFile(string fileName, string contentType, int size)
    {
        var stream = new MemoryStream(new byte[size]);
        var mockFile = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(size);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        return mockFile.Object;
    }
}
