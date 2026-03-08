using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Petoria.Controllers;
using Petoria.Core.DTOs.SupportMessage;
using Petoria.Core.Contracts;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class SupportMessagesControllerTests : ControllerTestBase
{
    private SupportMessagesController CreateController(string userId, params string[] roles)
    {
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => new ApplicationUser { Id = id, FirstName = "Test", LastName = "User", Email = $"{id}@test.com" });

        var mockEmailService = new Mock<IEmailService>();

        var controller = new SupportMessagesController(Context, mockUserManager.Object, mockEmailService.Object);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task CreateMessage_ReturnsOk()
    {
        var controller = CreateController("user1");
        var dto = new CreateSupportMessageDto { Subject = "Help", Message = "I need help" };

        var result = await controller.CreateMessage(dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMyMessages_ReturnsUserMessages()
    {
        Context.SupportMessages.Add(new SupportMessage { UserId = "user1", Subject = "Q1", Message = "Question 1" });
        Context.SupportMessages.Add(new SupportMessage { UserId = "user1", Subject = "Q2", Message = "Question 2" });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.GetMyMessages();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var msgs = Assert.IsAssignableFrom<IEnumerable<SupportMessageDto>>(ok.Value);
        Assert.Equal(2, msgs.Count());
    }

    [Fact]
    public async Task GetAllMessages_Admin_ReturnsAll()
    {
        await SeedUser("u1");
        await SeedUser("u2");
        Context.SupportMessages.Add(new SupportMessage { UserId = "u1", Subject = "Q1", Message = "M1" });
        Context.SupportMessages.Add(new SupportMessage { UserId = "u2", Subject = "Q2", Message = "M2" });
        await Context.SaveChangesAsync();
        var controller = CreateController("admin1", Petoria.Constants.Roles.SuperAdmin);

        var result = await controller.GetAllMessages();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var msgs = Assert.IsAssignableFrom<IEnumerable<SupportMessageDto>>(ok.Value);
        Assert.Equal(2, msgs.Count());
    }

    [Fact]
    public async Task AnswerMessage_Admin_ReturnsNoContent()
    {
        var msg = new SupportMessage { UserId = "u1", Subject = "Q", Message = "M" };
        Context.SupportMessages.Add(msg);
        await Context.SaveChangesAsync();
        var controller = CreateController("admin1", Petoria.Constants.Roles.SuperAdmin);

        var result = await controller.AnswerMessage(msg.Id, new AnswerSupportMessageDto { Response = "Done!" });

        Assert.IsType<NoContentResult>(result);
        var updated = Context.SupportMessages.First(m => m.Id == msg.Id);
        Assert.True(updated.IsAnswered);
    }

    [Fact]
    public async Task MarkAsRead_OwnMessage_ReturnsNoContent()
    {
        var msg = new SupportMessage { UserId = "user1", Subject = "Q", Message = "M", IsAnswered = true, AdminResponse = "OK", IsReadByUser = false };
        Context.SupportMessages.Add(msg);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.MarkAsRead(msg.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task MarkAsRead_OtherUser_ReturnsForbid()
    {
        var msg = new SupportMessage { UserId = "user1", Subject = "Q", Message = "M" };
        Context.SupportMessages.Add(msg);
        await Context.SaveChangesAsync();
        var controller = CreateController("other_user");

        var result = await controller.MarkAsRead(msg.Id);

        Assert.IsType<ForbidResult>(result);
    }
}
