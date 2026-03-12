using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Petoria.Controllers;
using Petoria.Core.Contracts;
using Petoria.Core.Models.Auth;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class AuthControllerTests : ControllerTestBase
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();

        // Mock IHttpClientFactory — captcha is skipped when SecretKey is empty
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();

        // Mock IConfiguration with empty SecretKey so captcha check is bypassed in tests
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["HcaptchaSettings:SecretKey"]).Returns(string.Empty);

        _controller = new AuthController(_mockAuthService.Object, mockHttpClientFactory.Object, mockConfig.Object);
        SetControllerUser(_controller, "user1");
    }

    [Fact]
    public async Task Register_Success_ReturnsOkWithToken()
    {
        var model = new RegisterModel { Email = "new@test.com", Password = "Pass123!", FirstName = "F", LastName = "L" };
        var response = new AuthResponse { Id = "1", Token = "tok", Email = "new@test.com", FirstName = "F", LastName = "L", Roles = new() };
        _mockAuthService.Setup(s => s.RegisterAsync(model)).ReturnsAsync(response);

        var result = await _controller.Register(model);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Register_Failure_ReturnsBadRequest()
    {
        var model = new RegisterModel { Email = "dup@test.com", Password = "P" };
        _mockAuthService.Setup(s => s.RegisterAsync(model)).ThrowsAsync(new Exception("Registration failed"));

        var result = await _controller.Register(model);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var model = new LoginModel { Email = "a@b.com", Password = "p" };
        var response = new AuthResponse { Id = "1", Token = "tok", Email = "a@b.com", Roles = new() };
        _mockAuthService.Setup(s => s.LoginAsync(model)).ReturnsAsync(response);

        var result = await _controller.Login(model);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var model = new LoginModel { Email = "a@b.com", Password = "wrong" };
        _mockAuthService.Setup(s => s.LoginAsync(model)).ReturnsAsync(null as AuthResponse);

        var result = await _controller.Login(model);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GoogleLogin_Success_ReturnsOk()
    {
        var response = new AuthResponse { Id = "1", Token = "tok", Email = "g@g.com", Roles = new() };
        _mockAuthService.Setup(s => s.GoogleLoginAsync("valid_token")).ReturnsAsync(response);

        var result = await _controller.GoogleLogin(new GoogleLoginModel { GoogleToken = "valid_token" });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GoogleLogin_Failure_ReturnsUnauthorized()
    {
        _mockAuthService.Setup(s => s.GoogleLoginAsync("bad_token")).ReturnsAsync(null as AuthResponse);

        var result = await _controller.GoogleLogin(new GoogleLoginModel { GoogleToken = "bad_token" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public void Logout_ReturnsOk()
    {
        var result = _controller.Logout();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
