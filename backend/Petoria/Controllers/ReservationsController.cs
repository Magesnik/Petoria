using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.Contracts;
using Petoria.Core.DTOs.Reservation;
using Petoria.Core.Models.Email;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;
using Petoria.Core.Utilities;

namespace Petoria.Controllers;

/// <summary>
/// Контролер за резервации: създаване, калкулация на цени, анулиране с възстановяване, потвърждение от количка.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPricingService _pricingService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        ApplicationDbContext context,
        IEmailService emailService,
        UserManager<ApplicationUser> userManager,
        IPricingService pricingService,
        ILogger<ReservationsController> logger)
    {
        _context = context;
        _emailService = emailService;
        _userManager = userManager;
        _pricingService = pricingService;
        _logger = logger;
    }

    /// <summary>
    /// Връща резервациите на текущия потребител.
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReservationResponseDto>>> GetMyReservations()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var reservationsData = await _context.Reservations
            .Include(r => r.Hotel)
            .Include(r => r.RoomType)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var reservations = reservationsData.Select(r => new ReservationResponseDto
        {
            Id = r.Id,
            HotelId = r.HotelId,
            HotelName = r.Hotel != null ? r.Hotel.Name : "",
            HotelImageUrl = r.Hotel != null ? r.Hotel.ImageUrl : "",
            RoomTypeId = r.RoomTypeId ?? 0,
            RoomTypeName = r.RoomType != null ? r.RoomType.Name : "Standard",
            CheckInDate = r.CheckInDate,
            CheckOutDate = r.CheckOutDate,
            NumberOfRooms = r.NumberOfRooms,
            NumberOfNights = (int)(r.CheckOutDate - r.CheckInDate).TotalDays,
            PricePerNight = r.RoomType != null ? r.RoomType.PricePerNight : 0,
            TotalPrice = r.TotalPrice,
            Status = r.Status,
            Notes = r.Notes,
            CreatedAt = r.CreatedAt
        }).ToList();

        return Ok(reservations);
    }

    /// <summary>
    /// Калкулира цената на резервация без да я създава.
    /// </summary>
    [HttpPost("calculate")]
    public async Task<ActionResult<PriceCalculationResponseDto>> CalculatePrice(PriceCalculationRequestDto request)
    {
        var roomType = await _context.RoomTypes.FindAsync(request.RoomTypeId);
        if (roomType == null)
            return NotFound(new { message = "Room type not found" });

        if (request.CheckOutDate <= request.CheckInDate)
            return BadRequest(new { message = "Check-out date must be after check-in date" });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var result = await _pricingService.CalculatePriceAsync(
            request.RoomTypeId,
            request.CheckInDate,
            request.CheckOutDate,
            request.NumberOfRooms,
            userId);

        return Ok(new PriceCalculationResponseDto
        {
            NumberOfNights = result.NumberOfNights,
            PricePerNight = result.PricePerNight,
            NumberOfRooms = result.NumberOfRooms,
            TotalPrice = result.TotalPrice,
            OriginalPrice = result.OriginalPrice,
            TotalDiscount = result.TotalDiscount,
            Breakdown = result.Breakdown
        });
    }

    /// <summary>
    /// Създава нова резервация с валидация на дати, наличност и изпращане на имейл.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReservationResponseDto>> CreateReservation(CreateReservationDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Валидация дали хотелът съществува
        var hotel = await _context.Hotels.FindAsync(request.HotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        // Валидация дали типът стая съществува и принадлежи на хотела
        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt => rt.Id == request.RoomTypeId && rt.HotelId == request.HotelId);

        if (roomType == null)
        {
            return NotFound(new { message = "Room type not found" });
        }

        // Валидация на датите
        if (request.CheckInDate.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(new { message = "Check-in date cannot be in the past" });
        }

        if (request.CheckOutDate <= request.CheckInDate)
        {
            return BadRequest(new { message = "Check-out date must be after check-in date" });
        }

        // Проверка на наличността за всички дати от диапазона
        for (var date = request.CheckInDate.Date; date < request.CheckOutDate.Date; date = date.AddDays(1))
        {
            var availability = await _context.RoomAvailabilities
                .FirstOrDefaultAsync(ra => ra.RoomTypeId == request.RoomTypeId && ra.Date == date);

            // Ако няма запис, използваме общия брой стаи като наличен
            var availableCount = availability?.AvailableCount ?? roomType.TotalRooms;
            var isBlocked = availability?.IsBlocked ?? false;

            if (isBlocked)
            {
                return BadRequest(new { message = $"Date {date:yyyy-MM-dd} is blocked for bookings" });
            }

            if (availableCount < request.NumberOfRooms)
            {
                return BadRequest(new { message = $"Not enough rooms available on {date:yyyy-MM-dd}. Available: {availableCount}" });
            }
        }

        // Изчисляване на общата цена чрез PricingService (включва 5% last-minute отстъпка)
        var priceResult = await _pricingService.CalculatePriceAsync(
            request.RoomTypeId,
            request.CheckInDate,
            request.CheckOutDate,
            request.NumberOfRooms,
            userId);
        var totalPrice = priceResult.TotalPrice;
        var numberOfNights = priceResult.NumberOfNights;

        // Създаване на резервация — преобразуване на DTO → Entity
        var reservation = new Reservation
        {
            UserId = userId,
            HotelId = request.HotelId,
            RoomTypeId = request.RoomTypeId,
            CheckInDate = request.CheckInDate.Date,
            CheckOutDate = request.CheckOutDate.Date,
            NumberOfRooms = request.NumberOfRooms,
            TotalPrice = totalPrice,
            Status = "Confirmed",
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Изпращане на имейл
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(userEmail))
        {
            try
            {
                var subject = $"Потвърждение на резервация в {hotel.Name}";
                var body = $@"
                    <div style=""font-family: Arial, sans-serif; background-color: #f4f7f6; padding: 40px 20px; color: #333;"">
                        <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">

                            <!-- Хедър -->
                            <div style=""background-color: #2F61E6; padding: 25px; text-align: center;"">
                                <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600;"">Petoria</h1>
                            </div>

                            <!-- Тяло -->
                            <div style=""padding: 30px;"">
                                <h2 style=""color: #2c3e50; font-size: 20px; margin-top: 0;"">Успешна резервация! 🎉</h2>
                                <p style=""font-size: 16px; line-height: 1.5; color: #555;"">
                                    Здравейте, <br><br>
                                    Вашата резервация в <strong>{hotel.Name}</strong> е успешно потвърдена. Очакваме ви с нетърпение!
                                </p>

                                <!-- Карта с детайли -->
                                <div style=""background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 20px; margin: 25px 0;"">
                                    <h3 style=""margin-top: 0; color: #4a5568; font-size: 16px; border-bottom: 2px solid #e2e8f0; padding-bottom: 10px;"">Детайли за настаняването</h3>

                                    <table style=""width: 100%; border-collapse: collapse; margin-top: 15px;"">
                                        <tr>
                                            <td style=""padding: 8px 0; color: #718096; width: 40%;"">Тип стая:</td>
                                            <td style=""padding: 8px 0; font-weight: 600; color: #2d3748;"">{roomType.Name}</td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 8px 0; color: #718096;"">Настаняване:</td>
                                            <td style=""padding: 8px 0; font-weight: 600; color: #2d3748;"">{request.CheckInDate:dd.MM.yyyy}</td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 8px 0; color: #718096;"">Напускане:</td>
                                            <td style=""padding: 8px 0; font-weight: 600; color: #2d3748;"">{request.CheckOutDate:dd.MM.yyyy}</td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 8px 0; color: #718096;"">Нощувки:</td>
                                            <td style=""padding: 8px 0; font-weight: 600; color: #2d3748;"">{numberOfNights}</td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 8px 0; color: #718096;"">Брой стаи:</td>
                                            <td style=""padding: 8px 0; font-weight: 600; color: #2d3748;"">{request.NumberOfRooms}</td>
                                        </tr>
                                    </table>
                                </div>

                                <!-- Обща цена -->
                                <div style=""background-color: #ebf8ff; border-left: 4px solid #3182ce; padding: 15px; margin-bottom: 25px;"">
                                    <table style=""width: 100%; border-collapse: collapse;"">
                                        {(roomType.PricePerNight * request.NumberOfRooms * numberOfNights > totalPrice ? $@"
                                        <tr>
                                            <td style=""padding: 4px 0; color: #718096; font-size: 15px;"">Базова цена:</td>
                                            <td style=""padding: 4px 0; color: #718096; font-size: 15px; text-decoration: line-through;"">{roomType.PricePerNight * request.NumberOfRooms * numberOfNights} лв.</td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 4px 0; color: #38a169; font-size: 15px;"">Спестено:</td>
                                            <td style=""padding: 4px 0; color: #38a169; font-size: 15px;"">{roomType.PricePerNight * request.NumberOfRooms * numberOfNights - totalPrice} лв.</td>
                                        </tr>
                                        " : "")}
                                        <tr>
                                            <td style=""padding: 8px 0 0 0; color: #2b6cb0; font-size: 16px; font-weight: bold;"">Платена сума:</td>
                                            <td style=""padding: 8px 0 0 0; color: #2b6cb0; font-size: 16px; font-weight: bold;"">{totalPrice} лв.</td>
                                        </tr>
                                    </table>
                                </div>

                                <p style=""font-size: 15px; color: #718096; margin-bottom: 0;"">
                                    Благодарим ви, че избрахте Petoria! За въпроси, свържете се с нас.
                                </p>
                            </div>

                            <!-- Футър -->
                            <div style=""background-color: #f7fafc; padding: 20px; text-align: center; border-top: 1px solid #edf2f7;"">
                                <p style=""margin: 0; color: #a0aec0; font-size: 13px;"">
                                    &copy; {DateTime.UtcNow.Year} Petoria. Всички права запазени.
                                </p>
                            </div>

                        </div>
                    </div>
                ";
                await _emailService.SendEmailAsync(userEmail, subject, body);
            }
            catch (Exception ex)
            {
                // Логваме грешката, но не провалваме резервацията
                _logger.LogError(ex, "Error sending email");
            }
        }

        // Обновяване на наличността за всички дати
        for (var date = request.CheckInDate.Date; date < request.CheckOutDate.Date; date = date.AddDays(1))
        {
            var availability = await _context.RoomAvailabilities
                .FirstOrDefaultAsync(ra => ra.RoomTypeId == request.RoomTypeId && ra.Date == date);

            if (availability != null)
            {
                availability.AvailableCount -= request.NumberOfRooms;
                availability.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Създаване на запис за наличност с намален брой
                _context.RoomAvailabilities.Add(new RoomAvailability
                {
                    RoomTypeId = request.RoomTypeId,
                    Date = date,
                    AvailableCount = roomType.TotalRooms - request.NumberOfRooms,
                    IsBlocked = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        // Връщане на отговор — преобразуване на Entity → Response DTO
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, new ReservationResponseDto
        {
            Id = reservation.Id,
            HotelId = reservation.HotelId,
            HotelName = hotel.Name,
            HotelImageUrl = hotel.ImageUrl,
            RoomTypeId = roomType.Id,
            RoomTypeName = roomType.Name,
            CheckInDate = reservation.CheckInDate,
            CheckOutDate = reservation.CheckOutDate,
            NumberOfRooms = reservation.NumberOfRooms,
            NumberOfNights = numberOfNights,
            PricePerNight = roomType.PricePerNight,
            TotalPrice = reservation.TotalPrice,
            Status = reservation.Status,
            Notes = reservation.Notes,
            CreatedAt = reservation.CreatedAt
        });
    }

    /// <summary>
    /// Връща конкретна резервация по ID (само за собственика, модератор, собственик на хотел или SuperAdmin).
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ReservationResponseDto>> GetReservation(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var reservation = await _context.Reservations
            .Include(r => r.Hotel)
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            return NotFound();
        }

        // Проверка дали потребителят е модератор на този хотел
        var isModerator = await _context.HotelModerators
            .AnyAsync(hm => hm.HotelId == reservation.HotelId && hm.UserId == userId);

        // Проверка дали потребителят е собственик на хотела
        var isOwner = await _context.Hotels
            .AnyAsync(h => h.Id == reservation.HotelId && h.CreatedById == userId);

        // Позволяваме достъп само на собственика на резервацията, SuperAdmin, модератор или собственик на хотела
        if (!isSuperAdmin && reservation.UserId != userId && !isModerator && !isOwner)
        {
            return Forbid();
        }

        return Ok(new ReservationResponseDto
        {
            Id = reservation.Id,
            HotelId = reservation.HotelId,
            HotelName = reservation.Hotel?.Name ?? "",
            HotelImageUrl = reservation.Hotel?.ImageUrl ?? "",
            RoomTypeId = reservation.RoomTypeId ?? 0,
            RoomTypeName = reservation.RoomType?.Name ?? "Standard",
            CheckInDate = reservation.CheckInDate,
            CheckOutDate = reservation.CheckOutDate,
            NumberOfRooms = reservation.NumberOfRooms,
            NumberOfNights = (int)(reservation.CheckOutDate - reservation.CheckInDate).TotalDays,
            PricePerNight = reservation.RoomType?.PricePerNight ?? 0,
            TotalPrice = reservation.TotalPrice,
            Status = reservation.Status,
            Notes = reservation.Notes,
            CreatedAt = reservation.CreatedAt
        });
    }

    /// <summary>
    /// Анулира резервация с изчисляване на възстановяване по политика за анулиране.
    /// </summary>
    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var reservation = await _context.Reservations
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            return NotFound();
        }

        // Проверка дали потребителят е модератор на този хотел
        var isModerator = await _context.HotelModerators
            .AnyAsync(hm => hm.HotelId == reservation.HotelId && hm.UserId == userId);

        // Проверка дали потребителят е собственик на хотела
        var isOwner = await _context.Hotels
            .AnyAsync(h => h.Id == reservation.HotelId && h.CreatedById == userId);

        // Позволяваме анулиране само на собственика, SuperAdmin, модератор или собственик на хотела
        if (!isSuperAdmin && reservation.UserId != userId && !isModerator && !isOwner)
        {
            return Forbid();
        }

        if (reservation.Status == "Cancelled")
        {
            return BadRequest(new { message = "Reservation is already cancelled" });
        }

        // Възстановяване на наличността
        if (reservation.RoomTypeId.HasValue)
        {
            for (var date = reservation.CheckInDate.Date; date < reservation.CheckOutDate.Date; date = date.AddDays(1))
            {
                var availability = await _context.RoomAvailabilities
                    .FirstOrDefaultAsync(ra => ra.RoomTypeId == reservation.RoomTypeId && ra.Date == date);

                if (availability != null)
                {
                    availability.AvailableCount += reservation.NumberOfRooms;
                    availability.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

    reservation.Status = "Cancelled";
    reservation.UpdatedAt = DateTime.UtcNow;

    // Изчисляване на възстановяването по политиката за анулиране на хотела
    var hotel = await _context.Hotels.FindAsync(reservation.HotelId);
    decimal refundPercentage = 100m; // По подразбиране 100% възстановяване, ако няма политики
    int daysBeforeCheckIn = (int)Math.Floor((reservation.CheckInDate.Date - DateTime.UtcNow.Date).TotalDays);

    if (hotel != null && !string.IsNullOrEmpty(hotel.CancellationPolicies) && hotel.CancellationPolicies != "[]")
    {
        try
        {
            var policies = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(hotel.CancellationPolicies);
            if (policies != null && policies.Any())
            {
                // Намираме политиката, която съответства (най-малкото daysBefore, което е >= дните до настаняването)
                var activePolicy = policies
                    .Where(p => (int)p.GetProperty("daysBefore").GetInt32() >= daysBeforeCheckIn)
                    .OrderBy(p => (int)p.GetProperty("daysBefore").GetInt32())
                    .FirstOrDefault();

                if (activePolicy.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                {
                    refundPercentage = (decimal)activePolicy.GetProperty("refundPercentage").GetInt32();
                }
                else if (daysBeforeCheckIn >= 0)
                {
                    // Ако анулирането е по-близо до настаняването от най-строгата политика — 0% възстановяване
                    refundPercentage = 0m;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing cancellation policies");
        }
    }

    reservation.RefundAmount = Math.Round(reservation.TotalPrice * (refundPercentage / 100m), 2);
    reservation.RetainedAmount = reservation.TotalPrice - reservation.RefundAmount;

    await _context.SaveChangesAsync();

    // Изпращане на имейл за анулиране
    var cancelUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    if (!string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(cancelUserId))
    {
        try
        {
            var applicationUser = await _userManager.FindByIdAsync(cancelUserId);
            var lang = applicationUser?.Language ?? "bg";

            var ctx = new CancellationEmailContext
            {
                UserLanguage = lang,
                HotelName = hotel?.Name ?? (lang == "en" ? "the hotel" : "хотела"),
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                RefundAmount = reservation.RefundAmount,
                RefundPercentage = refundPercentage,
                DaysBeforeCheckIn = daysBeforeCheckIn
            };

            var (subject, body) = EmailTemplateBuilder.BuildCancellationEmail(ctx);
            await _emailService.SendEmailAsync(userEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending cancellation email");
        }
    }

    return Ok(new { message = "Reservation cancelled successfully" });
}

    /// <summary>
    /// Потвърждава резервации от количка след успешно Stripe плащане.
    /// </summary>
    [HttpPost("confirm-cart")]
    [Authorize]
    public async Task<IActionResult> ConfirmCart([FromBody] ConfirmCartRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var createdReservations = new List<int>();

        // Търсене на промо код
        PromoCode promo = null;
        if (!string.IsNullOrEmpty(request.PromoCode))
        {
            promo = await _context.PromoCodes.FirstOrDefaultAsync(p => p.Code == request.PromoCode);
            if (promo != null && (!promo.IsActive || promo.CurrentActivations >= promo.MaxActivations || promo.ExpirationDate <= DateTime.UtcNow))
            {
                promo = null; // невалиден
            }
        }
        bool promoUsed = false;
        decimal grandTotalOriginal = 0;
        decimal grandTotalPaid = 0;

        foreach (var item in request.Items)
        {
            var hotel = await _context.Hotels.FindAsync(item.HotelId);
            if (hotel == null) continue;

            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(rt => rt.Id == item.RoomTypeId && rt.HotelId == item.HotelId);
            if (roomType == null) continue;

            // Изчисляване на общата цена с отстъпки
            var discounts = await _context.RoomDiscounts
                .Where(d => d.RoomTypeId == item.RoomTypeId &&
                            d.EndDate >= item.CheckInDate.Date &&
                            d.StartDate <= item.CheckOutDate.Date)
                .ToListAsync();

            decimal totalPrice = 0;
            for (var date = item.CheckInDate.Date; date < item.CheckOutDate.Date; date = date.AddDays(1))
            {
                var discount = discounts
                    .Where(d => d.StartDate.Date <= date && d.EndDate.Date >= date)
                    .OrderByDescending(d => d.DiscountPercentage)
                    .FirstOrDefault();

                var dayPrice = discount != null
                    ? roomType.PricePerNight * (1 - discount.DiscountPercentage / 100m)
                    : roomType.PricePerNight;

                totalPrice += dayPrice;
            }
            totalPrice *= item.NumberOfRooms;

            // Преди прилагане на глобален/хотелски промо код
            grandTotalOriginal += totalPrice;

            // Прилагане на глобален или хотелски промо код
            if (promo != null && (promo.HotelId == null || promo.HotelId == item.HotelId))
            {
                totalPrice = totalPrice * (1 - promo.DiscountPercentage / 100m);
                promoUsed = true;
            }

            // След прилагане на глобален/хотелски промо код
            grandTotalPaid += totalPrice;

            var reservation = new Reservation
            {
                UserId = userId,
                HotelId = item.HotelId,
                RoomTypeId = item.RoomTypeId,
                CheckInDate = item.CheckInDate.Date,
                CheckOutDate = item.CheckOutDate.Date,
                NumberOfRooms = item.NumberOfRooms,
                TotalPrice = totalPrice,
                Status = "Confirmed",
                Notes = $"Stripe payment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);

            // Обновяване на наличността
            for (var date = item.CheckInDate.Date; date < item.CheckOutDate.Date; date = date.AddDays(1))
            {
                var availability = await _context.RoomAvailabilities
                    .FirstOrDefaultAsync(ra => ra.RoomTypeId == item.RoomTypeId && ra.Date == date);

                if (availability != null)
                {
                    availability.AvailableCount -= item.NumberOfRooms;
                    availability.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.RoomAvailabilities.Add(new RoomAvailability
                    {
                        RoomTypeId = item.RoomTypeId,
                        Date = date,
                        AvailableCount = roomType.TotalRooms - item.NumberOfRooms,
                        IsBlocked = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            createdReservations.Add(reservation.Id);
        }

        // Увеличаване на броя използвания на промо кода, ако е бил успешно приложен
        if (promoUsed && promo != null)
        {
            promo.CurrentActivations++;
            _context.PromoCodes.Update(promo);
            await _context.SaveChangesAsync();
        }

        // Изпращане на обобщен имейл
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        if (!string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(userIdStr) && request.Items.Any())
        {
            try
            {
                var applicationUser = await _userManager.FindByIdAsync(userIdStr);
                var lang = applicationUser?.Language ?? "bg";

                var ctx = new ConfirmationEmailContext
                {
                    UserLanguage = lang,
                    GrandTotalOriginal = grandTotalOriginal,
                    GrandTotalPaid = grandTotalPaid,
                    PromoCode = promoUsed && promo != null ? promo.Code : string.Empty,
                    PromoCodeDiscountPercentage = promoUsed && promo != null ? promo.DiscountPercentage : null
                };

                foreach (var item in request.Items)
                {
                    var itemHotel = await _context.Hotels.FindAsync(item.HotelId);
                    var itemRoomType = await _context.RoomTypes.FindAsync(item.RoomTypeId);

                    if (itemHotel != null && itemRoomType != null)
                    {
                        ctx.Items.Add(new ConfirmationEmailItem
                        {
                            HotelName = itemHotel.Name,
                            RoomTypeName = itemRoomType.Name,
                            CheckInDate = item.CheckInDate,
                            CheckOutDate = item.CheckOutDate,
                            NumberOfRooms = item.NumberOfRooms
                        });
                    }
                }

                var (subject, body) = EmailTemplateBuilder.BuildConfirmationEmail(ctx);
                await _emailService.SendEmailAsync(userEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending cart confirmation email");
            }
        }

        return Ok(new { message = "Reservations confirmed successfully", reservationIds = createdReservations });
    }
}

public class ConfirmCartRequest
{
    public List<ConfirmCartItem> Items { get; set; } = new();
    public string? PromoCode { get; set; }
}

public class ConfirmCartItem
{
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfRooms { get; set; }
}
