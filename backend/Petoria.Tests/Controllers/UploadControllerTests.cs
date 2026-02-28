using Microsoft.AspNetCore.Mvc;
using Moq;
using Petoria.Controllers;
using Petoria.Core.Contracts;
using Petoria.Tests.Helpers;
using Microsoft.Extensions.Logging;
using CloudinaryDotNet.Actions;

namespace Petoria.Tests.Controllers;

public class UploadControllerTests : ControllerTestBase
{
    private readonly Mock<IPhotoService> _mockPhotoService;
    private readonly Mock<ILogger<UploadController>> _mockLogger;

    public UploadControllerTests()
    {
        _mockPhotoService = new Mock<IPhotoService>();
        _mockLogger = new Mock<ILogger<UploadController>>();
    }

    private UploadController CreateController(string userId, params string[] roles)
    {
        var controller = new UploadController(_mockPhotoService.Object, _mockLogger.Object);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task UploadImage_NoFile_ReturnsBadRequest()
    {
        var controller = CreateController("admin1", Petoria.Constants.Roles.Admin);

        var result = await controller.UploadImage(null!);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadImage_InvalidType_ReturnsBadRequest()
    {
        var controller = CreateController("admin1", Petoria.Constants.Roles.Admin);
        var file = CreateMockFile("test.exe", "application/octet-stream", 100);

        var result = await controller.UploadImage(file);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadImage_TooLarge_ReturnsBadRequest()
    {
        var controller = CreateController("admin1", Petoria.Constants.Roles.Admin);
        var file = CreateMockFile("test.jpg", "image/jpeg", 6 * 1024 * 1024); // 6MB

        var result = await controller.UploadImage(file);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadImage_ValidFile_ReturnsOk()
    {
        var uploadResult = new ImageUploadResult
        {
            SecureUrl = new Uri("https://example.com/photo.jpg"),
            PublicId = "photo_123"
        };
        _mockPhotoService.Setup(s => s.AddPhotoAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()))
            .ReturnsAsync(uploadResult);

        var controller = CreateController("admin1", Petoria.Constants.Roles.Admin);
        var file = CreateMockFile("test.jpg", "image/jpeg", 1024);

        var result = await controller.UploadImage(file);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteImage_Success_ReturnsOk()
    {
        _mockPhotoService.Setup(s => s.DeletePhotoAsync(It.IsAny<string>()))
            .ReturnsAsync(new DeletionResult { Result = "ok" });
        var controller = CreateController("admin1", Petoria.Constants.Roles.Admin);

        var result = await controller.DeleteImage("photo_123");

        Assert.IsType<OkObjectResult>(result);
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
