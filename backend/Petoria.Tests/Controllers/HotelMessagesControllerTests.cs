using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Petoria.Controllers;
using Petoria.Core.DTOs.HotelMessage;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class HotelMessagesControllerTests : ControllerTestBase
{
    private HotelMessagesController CreateController(string userId, params string[] roles)
    {
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => new ApplicationUser { Id = id, FirstName = "Test", LastName = "User", Email = $"{id}@test.com" });

        var controller = new HotelMessagesController(Context, mockUserManager.Object);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task CreateMessage_ValidData_ReturnsCreated()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        var controller = CreateController("user1");

        var dto = new CreateHotelMessageDto { Subject = "Question", Message = "Is breakfast included in the stay?" };
        var result = await controller.CreateMessage(hotel.Id, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetHotelMessages_Owner_ReturnsMessages()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        await SeedUser("guest1");
        Context.HotelMessages.Add(new HotelMessage { HotelId = hotel.Id, UserId = "guest1", Subject = "Q", Message = "M" });
        await Context.SaveChangesAsync();
        var controller = CreateController("owner1");

        var result = await controller.GetHotelMessages(hotel.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task AnswerMessage_Owner_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var msg = new HotelMessage { HotelId = hotel.Id, UserId = "guest1", Subject = "Q", Message = "M" };
        Context.HotelMessages.Add(msg);
        await Context.SaveChangesAsync();
        var controller = CreateController("owner1");

        var dto = new AnswerMessageDto { AdminResponse = "Yes, breakfast is included!" };
        var result = await controller.AnswerMessage(hotel.Id, msg.Id, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMyMessages_ReturnsUserMessages()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        Context.HotelMessages.Add(new HotelMessage { HotelId = hotel.Id, UserId = "user1", Subject = "Q", Message = "M" });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.GetMyMessages();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task MarkMessageAsRead_OwnMessage_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var msg = new HotelMessage { HotelId = hotel.Id, UserId = "user1", Subject = "Q", Message = "M", IsAnswered = true, AdminResponse = "Done", IsReadByUser = false };
        Context.HotelMessages.Add(msg);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.MarkMessageAsRead(msg.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetUnreadCounts_ReturnsCorrectCounts()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        Context.HotelMessages.Add(new HotelMessage { HotelId = hotel.Id, UserId = "u1", Subject = "Q1", Message = "M1", IsAnswered = false });
        Context.HotelMessages.Add(new HotelMessage { HotelId = hotel.Id, UserId = "u2", Subject = "Q2", Message = "M2", IsAnswered = true });
        await Context.SaveChangesAsync();
        var controller = CreateController("owner1");

        var result = await controller.GetUnreadCounts();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetUnreadUserResponsesCount_ReturnsCount()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        Context.HotelMessages.Add(new HotelMessage { HotelId = hotel.Id, UserId = "user1", Subject = "Q", Message = "M", IsAnswered = true, AdminResponse = "A", IsReadByUser = false });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.GetUnreadUserResponsesCount();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }
}
