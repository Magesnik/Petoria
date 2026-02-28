using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Petoria.Controllers;
using Petoria.Core.Contracts;
using Petoria.Core.DTOs.Reservation;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class ReservationsControllerTests : ControllerTestBase
{
    private readonly Mock<IEmailService> _mockEmailService;

    public ReservationsControllerTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private ReservationsController CreateController(string userId, params string[] roles)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .Returns((string id) => Context.Users.FirstOrDefaultAsync(u => u.Id == id)!);

        mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>(roles));

        var controller = new ReservationsController(Context, _mockEmailService.Object, mockUserManager.Object);
        SetControllerUser(controller, userId, roles);
        return controller;
    }

    [Fact]
    public async Task GetMyReservations_ReturnsUserReservations()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        await SeedUser("user1");
        Context.Reservations.Add(new Reservation
        {
            UserId = "user1", HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(5), CheckOutDate = DateTime.Today.AddDays(8),
            TotalPrice = 300, Status = "Confirmed"
        });
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.GetMyReservations();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task CalculatePrice_ValidData_ReturnsPrice()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id, "Standard", 100);
        for (int i = 1; i <= 3; i++)
        {
            Context.RoomAvailabilities.Add(new RoomAvailability
            {
                RoomTypeId = room.Id, Date = DateTime.Today.AddDays(i), AvailableCount = 5
            });
        }
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var dto = new PriceCalculationRequestDto
        {
            RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(4),
            NumberOfRooms = 1
        };

        var result = await controller.CalculatePrice(dto);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task CreateReservation_ValidData_ReturnsCreated()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id, "Standard", 100);
        var user = await SeedUser("user1");
        for (int i = 1; i <= 3; i++)
        {
            Context.RoomAvailabilities.Add(new RoomAvailability
            {
                RoomTypeId = room.Id, Date = DateTime.Today.AddDays(i), AvailableCount = 5
            });
        }
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var dto = new CreateReservationDto
        {
            HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(4),
            NumberOfRooms = 1
        };

        var result = await controller.CreateReservation(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task GetReservation_Exists_ReturnsReservation()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        await SeedUser("user1");
        var reservation = new Reservation
        {
            UserId = "user1", HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(5), CheckOutDate = DateTime.Today.AddDays(8),
            TotalPrice = 300, Status = "Confirmed"
        };
        Context.Reservations.Add(reservation);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.GetReservation(reservation.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task CancelReservation_Owner_ReturnsOk()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        hotel.CancellationPolicies = "[]";
        var room = await SeedRoomType(hotel.Id);
        var user = await SeedUser("user1");
        var reservation = new Reservation
        {
            UserId = "user1", HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(30), CheckOutDate = DateTime.Today.AddDays(33),
            TotalPrice = 300, Status = "Confirmed"
        };
        Context.Reservations.Add(reservation);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.CancelReservation(reservation.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CancelReservation_AlreadyCancelled_ReturnsBadRequest()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id);
        await SeedUser("user1");
        var reservation = new Reservation
        {
            UserId = "user1", HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(5), CheckOutDate = DateTime.Today.AddDays(8),
            TotalPrice = 300, Status = "Cancelled"
        };
        Context.Reservations.Add(reservation);
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var result = await controller.CancelReservation(reservation.Id);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateReservation_NoAvailability_ReturnsBadRequest()
    {
        var (hotel, _) = await SeedHotelWithOwner();
        var room = await SeedRoomType(hotel.Id, "Standard", 100, 1);
        await SeedUser("user1");
        for (int i = 1; i <= 3; i++)
        {
            Context.RoomAvailabilities.Add(new RoomAvailability
            {
                RoomTypeId = room.Id, Date = DateTime.Today.AddDays(i), AvailableCount = 0
            });
        }
        await Context.SaveChangesAsync();
        var controller = CreateController("user1");

        var dto = new CreateReservationDto
        {
            HotelId = hotel.Id, RoomTypeId = room.Id,
            CheckInDate = DateTime.Today.AddDays(1), CheckOutDate = DateTime.Today.AddDays(4),
            NumberOfRooms = 1
        };

        var result = await controller.CreateReservation(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
