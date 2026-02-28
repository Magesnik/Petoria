using Microsoft.AspNetCore.Mvc;
using Petoria.Controllers;
using Petoria.Core.DTOs.PromoCode;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class PromoCodesControllerTests : ControllerTestBase
{
    private PromoCodesController CreateController(string userId, params string[] roles)
    {
        var controller = new PromoCodesController(Context);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task GetPromoCodes_ReturnsHotelCodes()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        Context.PromoCodes.Add(new PromoCode { HotelId = hotel.Id, Code = "SAVE10", DiscountPercentage = 10, MaxActivations = 100, ExpirationDate = DateTime.UtcNow.AddDays(30) });
        await Context.SaveChangesAsync();
        var controller = CreateController("owner1");

        var result = await controller.GetPromoCodes(hotel.Id);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetGlobalPromoCodes_ReturnsCodes()
    {
        Context.PromoCodes.Add(new PromoCode { HotelId = null, Code = "GLOBAL20", DiscountPercentage = 20, MaxActivations = 500, ExpirationDate = DateTime.UtcNow.AddDays(60) });
        await Context.SaveChangesAsync();
        var controller = CreateController("admin1", "SuperAdmin");

        var result = await controller.GetGlobalPromoCodes();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateGlobalPromoCode_Admin_ReturnsOk()
    {
        var controller = CreateController("admin1", "SuperAdmin");
        var dto = new CreatePromoCodeDto { Code = "NEWGLOBAL", DiscountPercentage = 15, MaxActivations = 100, ValidDays = 30 };

        var result = await controller.CreateGlobalPromoCode(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task CreatePromoCode_Owner_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var controller = CreateController("owner1");
        var dto = new CreatePromoCodeDto { Code = "HOTEL10", DiscountPercentage = 10, MaxActivations = 50, ValidDays = 14 };

        var result = await controller.CreatePromoCode(hotel.Id, dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task CreatePromoCode_NotOwner_ReturnsForbid()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var controller = CreateController("other_user");
        var dto = new CreatePromoCodeDto { Code = "HACK", DiscountPercentage = 99, MaxActivations = 1, ValidDays = 1 };

        var result = await controller.CreatePromoCode(hotel.Id, dto);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task DeletePromoCode_Exists_ReturnsNoContent()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        var code = new PromoCode { HotelId = hotel.Id, Code = "DEL", DiscountPercentage = 5, MaxActivations = 10, ExpirationDate = DateTime.UtcNow.AddDays(10) };
        Context.PromoCodes.Add(code);
        await Context.SaveChangesAsync();
        var controller = CreateController("owner1");

        var result = await controller.DeletePromoCode(code.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ValidatePromoCode_Valid_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        Context.PromoCodes.Add(new PromoCode { HotelId = hotel.Id, Code = "VALID", DiscountPercentage = 15, MaxActivations = 100, CurrentActivations = 0, ExpirationDate = DateTime.UtcNow.AddDays(30) });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.ValidatePromoCode("VALID");

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task ValidatePromoCode_Expired_ReturnsNotFound()
    {
        var (hotel, _) = await SeedHotelWithOwner("owner1");
        Context.PromoCodes.Add(new PromoCode { HotelId = hotel.Id, Code = "EXPIRED", DiscountPercentage = 15, MaxActivations = 100, CurrentActivations = 0, ExpirationDate = DateTime.UtcNow.AddDays(-1) });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.ValidatePromoCode("EXPIRED");

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
