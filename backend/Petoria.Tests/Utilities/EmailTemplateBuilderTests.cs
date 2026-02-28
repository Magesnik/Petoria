using Petoria.Core.Models.Email;
using Petoria.Core.Utilities;

namespace Petoria.Tests.Utilities;

public class EmailTemplateBuilderTests
{
    [Fact]
    public void BuildConfirmationEmail_Bulgarian_ReturnsValidHtml()
    {
        var ctx = new ConfirmationEmailContext
        {
            UserLanguage = "bg",
            GrandTotalPaid = 300,
            GrandTotalOriginal = 300,
            Items = new List<ConfirmationEmailItem>
            {
                new() { HotelName = "Хотел Тест", RoomTypeName = "Стандартна", CheckInDate = DateTime.Today.AddDays(5), CheckOutDate = DateTime.Today.AddDays(8), NumberOfRooms = 1 }
            }
        };

        var (subject, body) = EmailTemplateBuilder.BuildConfirmationEmail(ctx);

        Assert.Contains("Успешна резервация", subject);
        Assert.Contains("<!DOCTYPE html>", body);
        Assert.Contains("Хотел Тест", body);
        Assert.Contains("Стандартна", body);
        Assert.Contains("300", body);
    }

    [Fact]
    public void BuildConfirmationEmail_English_ReturnsValidHtml()
    {
        var ctx = new ConfirmationEmailContext
        {
            UserLanguage = "en",
            GrandTotalPaid = 250,
            GrandTotalOriginal = 300,
            PromoCode = "SAVE20",
            PromoCodeDiscountPercentage = 20,
            Items = new List<ConfirmationEmailItem>
            {
                new() { HotelName = "Test Hotel", RoomTypeName = "Deluxe", CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(3), NumberOfRooms = 2 }
            }
        };

        var (subject, body) = EmailTemplateBuilder.BuildConfirmationEmail(ctx);

        Assert.Contains("Successful Reservation", subject);
        Assert.Contains("Test Hotel", body);
        Assert.Contains("Deluxe", body);
        Assert.Contains("SAVE20", body);
        Assert.Contains("Total saved", body);
    }

    [Fact]
    public void BuildConfirmationEmail_MultipleItems_AllIncluded()
    {
        var ctx = new ConfirmationEmailContext
        {
            UserLanguage = "en",
            GrandTotalPaid = 600,
            GrandTotalOriginal = 600,
            Items = new List<ConfirmationEmailItem>
            {
                new() { HotelName = "Hotel A", RoomTypeName = "Standard", CheckInDate = DateTime.Today, CheckOutDate = DateTime.Today.AddDays(2), NumberOfRooms = 1 },
                new() { HotelName = "Hotel B", RoomTypeName = "Suite", CheckInDate = DateTime.Today.AddDays(3), CheckOutDate = DateTime.Today.AddDays(5), NumberOfRooms = 1 }
            }
        };

        var (_, body) = EmailTemplateBuilder.BuildConfirmationEmail(ctx);

        Assert.Contains("Hotel A", body);
        Assert.Contains("Hotel B", body);
        Assert.Contains("Standard", body);
        Assert.Contains("Suite", body);
    }

    [Fact]
    public void BuildCancellationEmail_Bulgarian_ReturnsValidHtml()
    {
        var ctx = new CancellationEmailContext
        {
            UserLanguage = "bg",
            HotelName = "Хотел Тест",
            CheckInDate = DateTime.Today.AddDays(10),
            CheckOutDate = DateTime.Today.AddDays(13),
            RefundAmount = 200,
            RefundPercentage = 100,
            DaysBeforeCheckIn = 10
        };

        var (subject, body) = EmailTemplateBuilder.BuildCancellationEmail(ctx);

        Assert.Contains("Отмяна на резервация", subject);
        Assert.Contains("Хотел Тест", subject);
        Assert.Contains("<!DOCTYPE html>", body);
        Assert.Contains("Хотел Тест", body);
        Assert.Contains("200", body);
    }

    [Fact]
    public void BuildCancellationEmail_English_ReturnsValidHtml()
    {
        var ctx = new CancellationEmailContext
        {
            UserLanguage = "en",
            HotelName = "Test Hotel",
            CheckInDate = DateTime.Today.AddDays(5),
            CheckOutDate = DateTime.Today.AddDays(8),
            RefundAmount = 150,
            RefundPercentage = 80,
            DaysBeforeCheckIn = 5
        };

        var (subject, body) = EmailTemplateBuilder.BuildCancellationEmail(ctx);

        Assert.Contains("Reservation Cancelled", subject);
        Assert.Contains("Test Hotel", body);
        Assert.Contains("80%", body);
        Assert.Contains("150", body);
    }

    [Fact]
    public void BuildCancellationEmail_PartialRefund_ShowsPenaltyWarning()
    {
        var ctx = new CancellationEmailContext
        {
            UserLanguage = "en",
            HotelName = "Hotel X",
            CheckInDate = DateTime.Today.AddDays(2),
            CheckOutDate = DateTime.Today.AddDays(5),
            RefundAmount = 60,
            RefundPercentage = 60,
            DaysBeforeCheckIn = 2
        };

        var (_, body) = EmailTemplateBuilder.BuildCancellationEmail(ctx);

        // Partial refund shows penalty warning about cancellation policy
        Assert.Contains("60%", body);
        Assert.Contains("2 days before check-in", body);
    }

    [Fact]
    public void BuildCancellationEmail_ZeroRefund_ShowsNoRefundMessage()
    {
        var ctx = new CancellationEmailContext
        {
            UserLanguage = "en",
            HotelName = "Hotel Y",
            CheckInDate = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3),
            RefundAmount = 0,
            RefundPercentage = 0,
            DaysBeforeCheckIn = 1
        };

        var (_, body) = EmailTemplateBuilder.BuildCancellationEmail(ctx);

        Assert.Contains("does not support refunds", body);
    }
}
