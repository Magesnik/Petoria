using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Petoria.Core.Contracts;
using Petoria.Core.Models.Auth;
using Petoria.Core.Services;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object, httpContextAccessor.Object, userClaimsPrincipalFactory.Object,
            null!, null!, null!, null!);

        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            roleStore.Object, null!, null!, null!, null!);

        _mockConfig = new Mock<IConfiguration>();
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(s => s["Key"]).Returns("a-very-long-test-key-that-is-at-least-32-chars!!");
        jwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
        jwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
        jwtSection.Setup(s => s["ExpiresInDays"]).Returns("7");
        _mockConfig.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);
        _mockConfig.Setup(c => c["FrontendUrl"]).Returns("http://localhost:5174");
        _mockConfig.Setup(c => c["AdminSettings:SuperAdminEmails"]).Returns(string.Empty);

        _mockEmailService = new Mock<IEmailService>();
        _mockEmailService
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _service = new AuthService(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockRoleManager.Object,
            _mockConfig.Object,
            _mockEmailService.Object);
    }

    private static ApplicationUser MakeUser(string id = "user1", string email = "test@test.com") =>
        new ApplicationUser
        {
            Id = id,
            UserName = email,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true
        };

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponseWithToken()
    {
        var user = MakeUser();
        var model = new LoginModel { Email = user.Email!, Password = "Password123!" };

        _mockUserManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
        _mockUserManager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        _mockSignInManager
            .Setup(m => m.CheckPasswordSignInAsync(user, model.Password, false))
            .ReturnsAsync(SignInResult.Success);
        _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var result = await _service.LoginAsync(model);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsNull()
    {
        _mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _service.LoginAsync(new LoginModel { Email = "none@test.com", Password = "x" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var user = MakeUser();
        _mockUserManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
        _mockUserManager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        _mockSignInManager
            .Setup(m => m.CheckPasswordSignInAsync(user, It.IsAny<string>(), false))
            .ReturnsAsync(SignInResult.Failed);

        var result = await _service.LoginAsync(new LoginModel { Email = user.Email!, Password = "wrong" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_LockedOutUser_ThrowsException()
    {
        var user = MakeUser();
        _mockUserManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _mockUserManager.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
        _mockUserManager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true);

        await Assert.ThrowsAsync<Exception>(() =>
            _service.LoginAsync(new LoginModel { Email = user.Email!, Password = "pass" }));
    }

    [Fact]
    public async Task RegisterAsync_ValidModel_ReturnsAuthResponseWithEmptyToken()
    {
        var model = new RegisterModel
        {
            Email = "new@test.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        _mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager
            .Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());
        _mockUserManager
            .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("confirmation-token");

        var result = await _service.RegisterAsync(model);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Token); // No token until email confirmed
        Assert.Equal(model.Email, result.Email);
        _mockEmailService.Verify(
            e => e.SendEmailAsync(model.Email, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_CreateFails_ThrowsException()
    {
        var model = new RegisterModel
        {
            Email = "dup@test.com",
            Password = "Password123!",
            FirstName = "Dup",
            LastName = "User"
        };

        _mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateEmail",
                Description = "Email already taken"
            }));

        await Assert.ThrowsAsync<Exception>(() => _service.RegisterAsync(model));
    }

    [Fact]
    public async Task ConfirmEmailAsync_ValidToken_ReturnsTrue()
    {
        var user = MakeUser();
        _mockUserManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _mockUserManager
            .Setup(m => m.ConfirmEmailAsync(user, "valid-token"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _service.ConfirmEmailAsync(user.Id, "valid-token");

        Assert.True(result);
    }
}
