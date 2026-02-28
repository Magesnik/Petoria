using Microsoft.AspNetCore.Mvc;
using Petoria.Controllers;
using Petoria.Core.DTOs.Review;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class ReviewsControllerTests : ControllerTestBase
{
    private ReviewsController CreateController(string userId, params string[] roles)
    {
        var controller = new ReviewsController(Context);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task GetReviews_ReturnsReviewsForHotel()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var user = await SeedUser("reviewer1", "Rev", "User");
        Context.HotelReviews.Add(new HotelReview { HotelId = hotel.Id, UserId = user.Id, Rating = 5, ReviewText = "Great!" });
        await Context.SaveChangesAsync();
        var controller = CreateController("anyone");

        var result = await controller.GetReviews(hotel.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var reviews = Assert.IsAssignableFrom<IEnumerable<ReviewResponseDto>>(ok.Value);
        Assert.Single(reviews);
    }

    [Fact]
    public async Task PostReview_NewReview_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        var controller = CreateController("user1");

        var result = await controller.PostReview(hotel.Id, new CreateReviewDto { Rating = 4, ReviewText = "Nice" });

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(Context.HotelReviews.Where(r => r.HotelId == hotel.Id));
    }

    [Fact]
    public async Task PostReview_UpdateExisting_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        Context.HotelReviews.Add(new HotelReview { HotelId = hotel.Id, UserId = "user1", Rating = 3, ReviewText = "OK" });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.PostReview(hotel.Id, new CreateReviewDto { Rating = 5, ReviewText = "Updated!" });

        Assert.IsType<OkObjectResult>(result);
        var review = Context.HotelReviews.First(r => r.HotelId == hotel.Id && r.UserId == "user1");
        Assert.Equal(5, review.Rating);
    }

    [Fact]
    public async Task PostReview_HotelNotFound_ReturnsNotFound()
    {
        await SeedUser("user1");
        var controller = CreateController("user1");

        var result = await controller.PostReview(9999, new CreateReviewDto { Rating = 5, ReviewText = "Hello" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteReview_OwnReview_ReturnsNoContent()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        var review = new HotelReview { HotelId = hotel.Id, UserId = "user1", Rating = 4, ReviewText = "Good" };
        Context.HotelReviews.Add(review);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.DeleteReview(hotel.Id, review.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteReview_OtherUser_ReturnsForbid()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        var review = new HotelReview { HotelId = hotel.Id, UserId = "user1", Rating = 4, ReviewText = "Good" };
        Context.HotelReviews.Add(review);
        await Context.SaveChangesAsync();
        var controller = CreateController("other_user");

        var result = await controller.DeleteReview(hotel.Id, review.Id);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteReview_WrongHotel_ReturnsBadRequest()
    {
        var (hotel1, _) = await SeedHotelWithOwner("owner1", "Hotel 1");
        var (hotel2, _) = await SeedHotelWithOwner("owner2", "Hotel 2");
        await SeedUser("user1");
        var review = new HotelReview { HotelId = hotel1.Id, UserId = "user1", Rating = 4, ReviewText = "Good" };
        Context.HotelReviews.Add(review);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.DeleteReview(hotel2.Id, review.Id);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task PostReview_UpdatesHotelRating()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        await SeedUser("user1");
        await SeedUser("user2");
        Context.HotelReviews.Add(new HotelReview { HotelId = hotel.Id, UserId = "user2", Rating = 2, ReviewText = "Bad" });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        await controller.PostReview(hotel.Id, new CreateReviewDto { Rating = 4, ReviewText = "Good" });

        var updatedHotel = Context.Hotels.First(h => h.Id == hotel.Id);
        Assert.Equal(3m, updatedHotel.Rating); // (2+4)/2 = 3
    }
}
