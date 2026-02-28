using Microsoft.AspNetCore.Mvc;
using Petoria.Controllers;
using Petoria.Core.DTOs.Comment;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class CommentsControllerTests : ControllerTestBase
{
    private CommentsController CreateController(string userId, params string[] roles)
    {
        var controller = new CommentsController(Context);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task GetHotelComments_ReturnsComments()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var user = await SeedUser("user1");
        Context.Comments.Add(new Comment { HotelId = hotel.Id, UserId = user.Id, Text = "Nice hotel!" });
        await Context.SaveChangesAsync();
        var controller = CreateController("anyone");

        var result = await controller.GetHotelComments(hotel.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task CreateComment_ValidData_ReturnsCreated()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        var controller = CreateController("user1");

        var dto = new CreateCommentDto { HotelId = hotel.Id, Text = "Great place!" };
        var result = await controller.CreateComment(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task CreateComment_Reply_LinksToParent()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        var parent = new Comment { HotelId = hotel.Id, UserId = "user1", Text = "Parent" };
        Context.Comments.Add(parent);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var dto = new CreateCommentDto { HotelId = hotel.Id, Text = "Reply!", ParentCommentId = parent.Id };
        var result = await controller.CreateComment(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task UpdateComment_OwnComment_ReturnsNoContent()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        var comment = new Comment { HotelId = hotel.Id, UserId = "user1", Text = "Original" };
        Context.Comments.Add(comment);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var dto = new UpdateCommentDto { Text = "Updated" };
        var result = await controller.UpdateComment(comment.Id, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateComment_NotOwner_ReturnsForbid()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        var comment = new Comment { HotelId = hotel.Id, UserId = "user1", Text = "Original" };
        Context.Comments.Add(comment);
        await Context.SaveChangesAsync();
        var controller = CreateController("other_user");

        var dto = new UpdateCommentDto { Text = "Hacked" };
        var result = await controller.UpdateComment(comment.Id, dto);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteComment_OwnComment_ReturnsNoContent()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        var comment = new Comment { HotelId = hotel.Id, UserId = "user1", Text = "Delete me" };
        Context.Comments.Add(comment);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.DeleteComment(comment.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RateComment_FirstVote_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var user = await SeedUser("user1");
        var comment = new Comment { HotelId = hotel.Id, UserId = user.Id, Text = "Rate me" };
        Context.Comments.Add(comment);
        await Context.SaveChangesAsync();
        await SeedUser("voter1");
        var controller = CreateController("voter1");

        var dto = new RateCommentDto { IsLike = true };
        var result = await controller.RateComment(comment.Id, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RateComment_ChangeVote_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var user = await SeedUser("user1");
        var comment = new Comment { HotelId = hotel.Id, UserId = user.Id, Text = "Rate me" };
        Context.Comments.Add(comment);
        await Context.SaveChangesAsync();
        await SeedUser("voter1");
        Context.CommentRatings.Add(new CommentRating { CommentId = comment.Id, UserId = "voter1", IsLike = true });
        await Context.SaveChangesAsync();
        var controller = CreateController("voter1");

        var dto = new RateCommentDto { IsLike = false };
        var result = await controller.RateComment(comment.Id, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetCommentReplies_ReturnsReplies()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var user = await SeedUser("user1");
        var parent = new Comment { HotelId = hotel.Id, UserId = user.Id, Text = "Parent" };
        Context.Comments.Add(parent);
        await Context.SaveChangesAsync();
        Context.Comments.Add(new Comment { HotelId = hotel.Id, UserId = user.Id, Text = "Reply", ParentCommentId = parent.Id });
        await Context.SaveChangesAsync();
        var controller = CreateController("anyone");

        var result = await controller.GetCommentReplies(parent.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }
}
