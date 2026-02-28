using Petoria.Constants;

namespace Petoria.Tests.Constants;

public class ValidationConstantsTests
{
    [Fact]
    public void Hotel_NameMaxLength_IsPositive()
    {
        Assert.True(ValidationConstants.Hotel.NameMaxLength > 0);
    }

    [Fact]
    public void Hotel_DescriptionMaxLength_IsPositive()
    {
        Assert.True(ValidationConstants.Hotel.DescriptionMaxLength > 0);
    }

    [Fact]
    public void Hotel_LocationMaxLength_IsPositive()
    {
        Assert.True(ValidationConstants.Hotel.LocationMaxLength > 0);
    }

    [Fact]
    public void Hotel_StarRatingMin_LessThanOrEqualToMax()
    {
        Assert.True(ValidationConstants.Hotel.StarRatingMin <= ValidationConstants.Hotel.StarRatingMax);
    }

    [Fact]
    public void RoomType_CapacityMin_LessThanMax()
    {
        Assert.True(ValidationConstants.RoomType.CapacityMin < ValidationConstants.RoomType.CapacityMax);
    }

    [Fact]
    public void RoomType_TotalRoomsMin_LessThanMax()
    {
        Assert.True(ValidationConstants.RoomType.TotalRoomsMin < ValidationConstants.RoomType.TotalRoomsMax);
    }

    [Fact]
    public void Discount_PercentageMin_LessThanMax()
    {
        Assert.True(ValidationConstants.Discount.PercentageMin < ValidationConstants.Discount.PercentageMax);
    }

    [Fact]
    public void Reservation_RoomsMin_LessThanMax()
    {
        Assert.True(ValidationConstants.Reservation.RoomsMin < ValidationConstants.Reservation.RoomsMax);
    }

    [Fact]
    public void HotelReview_RatingMin_LessThanMax()
    {
        Assert.True(ValidationConstants.HotelReview.RatingMin < ValidationConstants.HotelReview.RatingMax);
    }

    [Fact]
    public void Comment_ContentMaxLength_IsPositive()
    {
        Assert.True(ValidationConstants.Comment.ContentMaxLength > 0);
    }

    [Fact]
    public void Availability_AvailableCountMin_IsNonNegative()
    {
        Assert.True(ValidationConstants.Availability.AvailableCountMin >= 0);
    }
}
