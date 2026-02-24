using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Reservation;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly Petoria.Core.Contracts.IEmailService _emailService;

    public ReservationsController(ApplicationDbContext context, Petoria.Core.Contracts.IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // GET: api/reservations/my - Get current user's reservations
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReservationResponseDto>>> GetMyReservations()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var reservations = await _context.Reservations
            .Include(r => r.Hotel)
            .Include(r => r.RoomType)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReservationResponseDto
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
            })
            .ToListAsync();

        return Ok(reservations);
    }

    // POST: api/reservations/calculate - Calculate price without creating reservation
    [HttpPost("calculate")]
    public async Task<ActionResult<PriceCalculationResponseDto>> CalculatePrice(PriceCalculationRequestDto request)
    {
        var roomType = await _context.RoomTypes.FindAsync(request.RoomTypeId);
        if (roomType == null)
        {
            return NotFound(new { message = "Room type not found" });
        }

        if (request.CheckOutDate <= request.CheckInDate)
        {
            return BadRequest(new { message = "Check-out date must be after check-in date" });
        }

        var numberOfNights = (int)(request.CheckOutDate.Date - request.CheckInDate.Date).TotalDays;
        
        // Get discounts for the date range
        var discounts = await _context.RoomDiscounts
            .Where(d => d.RoomTypeId == request.RoomTypeId &&
                        d.EndDate >= request.CheckInDate.Date &&
                        d.StartDate <= request.CheckOutDate.Date)
            .ToListAsync();

        // Calculate price per day with discounts
        var breakdown = new List<DayPriceBreakdownDto>();
        decimal totalPrice = 0;
        decimal originalTotal = 0;

        for (var date = request.CheckInDate.Date; date < request.CheckOutDate.Date; date = date.AddDays(1))
        {
            var discount = discounts
                .Where(d => d.StartDate.Date <= date && d.EndDate.Date >= date)
                .OrderByDescending(d => d.DiscountPercentage)
                .FirstOrDefault();

            var dayOriginal = roomType.PricePerNight;
            var dayFinal = discount != null 
                ? dayOriginal * (1 - discount.DiscountPercentage / 100m)
                : dayOriginal;

            breakdown.Add(new DayPriceBreakdownDto
            {
                Date = date,
                OriginalPrice = dayOriginal,
                DiscountPercentage = discount?.DiscountPercentage,
                FinalPrice = dayFinal
            });

            originalTotal += dayOriginal;
            totalPrice += dayFinal;
        }

        totalPrice *= request.NumberOfRooms;
        originalTotal *= request.NumberOfRooms;

        // Check for moderator/owner if user is logged in
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            var isModerator = await _context.HotelModerators
                .AnyAsync(hm => hm.HotelId == roomType.HotelId && hm.UserId == userId);
            
            var isOwner = await _context.Hotels
                .AnyAsync(h => h.Id == roomType.HotelId && h.CreatedById == userId);

            if (isModerator || isOwner)
            {
                totalPrice = 0;
            }
        }

        return Ok(new PriceCalculationResponseDto
        {
            NumberOfNights = numberOfNights,
            PricePerNight = roomType.PricePerNight,
            NumberOfRooms = request.NumberOfRooms,
            TotalPrice = totalPrice,
            OriginalPrice = originalTotal,
            TotalDiscount = originalTotal - totalPrice,
            Breakdown = breakdown
        });
    }

    // POST: api/reservations - Create a new reservation
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReservationResponseDto>> CreateReservation(CreateReservationDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Validate hotel exists
        var hotel = await _context.Hotels.FindAsync(request.HotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        // Validate room type exists and belongs to hotel
        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt => rt.Id == request.RoomTypeId && rt.HotelId == request.HotelId);
        
        if (roomType == null)
        {
            return NotFound(new { message = "Room type not found" });
        }

        // Validate dates
        if (request.CheckInDate.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(new { message = "Check-in date cannot be in the past" });
        }

        if (request.CheckOutDate <= request.CheckInDate)
        {
            return BadRequest(new { message = "Check-out date must be after check-in date" });
        }

        // Check availability for all dates in the range
        for (var date = request.CheckInDate.Date; date < request.CheckOutDate.Date; date = date.AddDays(1))
        {
            var availability = await _context.RoomAvailabilities
                .FirstOrDefaultAsync(ra => ra.RoomTypeId == request.RoomTypeId && ra.Date == date);

            // If no record exists, use TotalRooms as available count
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

        // Calculate total price with discounts
        var numberOfNights = (int)(request.CheckOutDate.Date - request.CheckInDate.Date).TotalDays;
        
        // Get discounts for the date range
        var discounts = await _context.RoomDiscounts
            .Where(d => d.RoomTypeId == request.RoomTypeId &&
                        d.EndDate >= request.CheckInDate.Date &&
                        d.StartDate <= request.CheckOutDate.Date)
            .ToListAsync();

        decimal totalPrice = 0;
        for (var date = request.CheckInDate.Date; date < request.CheckOutDate.Date; date = date.AddDays(1))
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
        totalPrice *= request.NumberOfRooms;

        // Check if user is moderator for this hotel
        var isModerator = await _context.HotelModerators
            .AnyAsync(hm => hm.HotelId == request.HotelId && hm.UserId == userId);
        
        // Also check if owner (though owners usually don't book their own rooms via API, logic applies)
        var isOwner = await _context.Hotels
            .AnyAsync(h => h.Id == request.HotelId && h.CreatedById == userId);

        if (isModerator || isOwner)
        {
            totalPrice = 0; // Free for moderators and owners
        }

        // Create reservation — Map DTO → Entity
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
        
        // Send email
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(userEmail))
        {
            try
            {
                var subject = $"Потвърждение на резервация в {hotel.Name}";
                var body = $@"
                    <div style=""font-family: Arial, sans-serif; background-color: #f4f7f6; padding: 40px 20px; color: #333;"">
                        <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">
                            
                            <!-- Header -->
                            <div style=""background-color: #2F61E6; padding: 25px; text-align: center;"">
                                <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600;"">Petoria</h1>
                            </div>
                            
                            <!-- Body -->
                            <div style=""padding: 30px;"">
                                <h2 style=""color: #2c3e50; font-size: 20px; margin-top: 0;"">Успешна резервация! 🎉</h2>
                                <p style=""font-size: 16px; line-height: 1.5; color: #555;"">
                                    Здравейте, <br><br>
                                    Вашата резервация в <strong>{hotel.Name}</strong> е успешно потвърдена. Очакваме ви с нетърпение!
                                </p>
                                
                                <!-- Details Card -->
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
                                
                                <!-- Total Price -->
                                <div style=""background-color: #ebf8ff; border-left: 4px solid #3182ce; padding: 15px; margin-bottom: 25px;"">
                                    <p style=""margin: 0; color: #2b6cb0; font-size: 16px;"">
                                        Обща цена: <strong>{totalPrice} лв.</strong>
                                    </p>
                                </div>
                                
                                <p style=""font-size: 15px; color: #718096; margin-bottom: 0;"">
                                    Благодарим ви, че избрахте Petoria! За въпроси, свържете се с нас.
                                </p>
                            </div>
                            
                            <!-- Footer -->
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
                // Log exception in production, but don't fail the reservation
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        // Update availability for all dates
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
                // Create availability record with reduced count
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

        // Return response — Map Entity → Response DTO
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

    // GET: api/reservations/5 - Get a specific reservation
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

        // Check if user is moderator for this hotel
        var isModerator = await _context.HotelModerators
            .AnyAsync(hm => hm.HotelId == reservation.HotelId && hm.UserId == userId);
        
        // Check if user is owner
        var isOwner = await _context.Hotels
            .AnyAsync(h => h.Id == reservation.HotelId && h.CreatedById == userId);

        // Only allow user to see their own reservations (or SuperAdmin, Moderator, or Owner)
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

    // PUT: api/reservations/5/cancel - Cancel a reservation
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

        // Check if user is moderator for this hotel
        var isModerator = await _context.HotelModerators
            .AnyAsync(hm => hm.HotelId == reservation.HotelId && hm.UserId == userId);
        
        // Check if user is owner
        var isOwner = await _context.Hotels
            .AnyAsync(h => h.Id == reservation.HotelId && h.CreatedById == userId);

        // Only allow user to cancel their own reservations (or SuperAdmin, Moderator, or Owner)
        if (!isSuperAdmin && reservation.UserId != userId && !isModerator && !isOwner)
        {
            return Forbid();
        }

        if (reservation.Status == "Cancelled")
        {
            return BadRequest(new { message = "Reservation is already cancelled" });
        }

        // Restore availability
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

        await _context.SaveChangesAsync();

        // Send email
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(userEmail))
        {
            try
            {
                var hotel = await _context.Hotels.FindAsync(reservation.HotelId);
                var subject = $"Отмяна на резервация в {hotel?.Name ?? "хотела"}";
                var body = $@"
                    <div style=""font-family: Arial, sans-serif; background-color: #f4f7f6; padding: 40px 20px; color: #333;"">
                        <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">
                            
                            <!-- Header -->
                            <div style=""background-color: #e53e3e; padding: 25px; text-align: center;"">
                                <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600;"">Petoria</h1>
                            </div>
                            
                            <!-- Body -->
                            <div style=""padding: 30px;"">
                                <h2 style=""color: #2c3e50; font-size: 20px; margin-top: 0;"">Успешно отменена резервация</h2>
                                <p style=""font-size: 16px; line-height: 1.5; color: #555;"">
                                    Здравейте, <br><br>
                                    Вашата резервация в <strong>{hotel?.Name ?? "хотела"}</strong> за периода <strong>{reservation.CheckInDate:dd.MM.yyyy} - {reservation.CheckOutDate:dd.MM.yyyy}</strong> беше успешно отменена.
                                </p>
                                
                                <div style=""background-color: #fffaf0; border-left: 4px solid #dd6b20; padding: 15px; margin: 25px 0;"">
                                    <p style=""margin: 0; color: #c05621; font-size: 15px;"">
                                        Ако това е станало по погрешка или имате въпроси, моля не се колебайте да се свържете с нас възможно най-скоро.
                                    </p>
                                </div>
                                
                                <p style=""font-size: 15px; color: #718096; margin-bottom: 0;"">
                                    Поздрави,<br/>Екипът на Petoria
                                </p>
                            </div>
                            
                            <!-- Footer -->
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
                Console.WriteLine($"Error sending cancellation email: {ex.Message}");
            }
        }

        return Ok(new { message = "Reservation cancelled successfully" });
    }

    // POST: api/reservations/confirm-cart - Create reservations after successful Stripe payment
    [HttpPost("confirm-cart")]
    [Authorize]
    public async Task<IActionResult> ConfirmCart([FromBody] ConfirmCartRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var createdReservations = new List<int>();

        // Find promo code
        PromoCode promo = null;
        if (!string.IsNullOrEmpty(request.PromoCode))
        {
            promo = await _context.PromoCodes.FirstOrDefaultAsync(p => p.Code == request.PromoCode);
            if (promo != null && (!promo.IsActive || promo.CurrentActivations >= promo.MaxActivations || promo.ExpirationDate <= DateTime.UtcNow))
            {
                promo = null; // invalid
            }
        }
        bool promoUsed = false;

        foreach (var item in request.Items)
        {
            var hotel = await _context.Hotels.FindAsync(item.HotelId);
            if (hotel == null) continue;

            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(rt => rt.Id == item.RoomTypeId && rt.HotelId == item.HotelId);
            if (roomType == null) continue;

            // Calculate total price with discounts
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

            // Apply global or hotel-specific promo code discount
            if (promo != null && (promo.HotelId == null || promo.HotelId == item.HotelId))
            {
                totalPrice = totalPrice * (1 - promo.DiscountPercentage / 100m);
                promoUsed = true;
            }

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

            // Update availability
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

        // Increment promo code usage if it was successfully applied to at least one reservation
        if (promoUsed && promo != null)
        {
            promo.CurrentActivations++;
            _context.PromoCodes.Update(promo);
            await _context.SaveChangesAsync();
        }

        // Send aggregated email
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(userEmail) && request.Items.Any())
        {
            try
            {
                var subject = "Успешна резервация през Petoria Cart";
                
                var htmlBody = @"
                    <div style=""font-family: Arial, sans-serif; background-color: #f4f7f6; padding: 40px 20px; color: #333;"">
                        <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">
                            
                            <!-- Header -->
                            <div style=""background-color: #2F61E6; padding: 25px; text-align: center;"">
                                <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600;"">Petoria</h1>
                            </div>
                            
                            <!-- Body -->
                            <div style=""padding: 30px;"">
                                <h2 style=""color: #2c3e50; font-size: 20px; margin-top: 0;"">Успешна резервация! 🎉</h2>
                                <p style=""font-size: 16px; line-height: 1.5; color: #555;"">
                                    Здравейте, <br><br>
                                    Благодарим ви, че избрахте Petoria. Вашите резервации са успешно потвърдени и платени. Ето детайлите:
                                </p>";

                foreach (var item in request.Items)
                {
                    var itemHotel = await _context.Hotels.FindAsync(item.HotelId);
                    var itemRoomType = await _context.RoomTypes.FindAsync(item.RoomTypeId);
                    
                    if (itemHotel != null && itemRoomType != null)
                    {
                        var nights = (int)(item.CheckOutDate.Date - item.CheckInDate.Date).TotalDays;
                        htmlBody += $@"
                                <!-- Details Card -->
                                <div style=""background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 20px; margin: 15px 0;"">
                                    <h3 style=""margin-top: 0; color: #2b6cb0; font-size: 16px; border-bottom: 2px solid #e2e8f0; padding-bottom: 10px;"">{itemHotel.Name}</h3>
                                    
                                    <table style=""width: 100%; border-collapse: collapse; margin-top: 10px;"">
                                        <tr>
                                            <td style=""padding: 6px 0; color: #718096; width: 40%; font-size: 15px;"">Стая:</td>
                                            <td style=""padding: 6px 0; font-weight: 600; color: #2d3748; font-size: 15px;"">{itemRoomType.Name}</td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 6px 0; color: #718096; font-size: 15px;"">Период:</td>
                                            <td style=""padding: 6px 0; font-weight: 600; color: #2d3748; font-size: 15px;"">{item.CheckInDate:dd.MM.yyyy} - {item.CheckOutDate:dd.MM.yyyy} ({nights} нощувки)</td>
                                        </tr>
                                        <tr>
                                            <td style=""padding: 6px 0; color: #718096; font-size: 15px;"">Брой стаи:</td>
                                            <td style=""padding: 6px 0; font-weight: 600; color: #2d3748; font-size: 15px;"">{item.NumberOfRooms}</td>
                                        </tr>
                                    </table>
                                </div>";
                    }
                }

                htmlBody += @"
                                <p style=""font-size: 15px; color: #718096; margin-top: 25px; margin-bottom: 0;"">
                                    Очакваме ви с нетърпение! За въпроси, свържете се с нас.
                                </p>
                            </div>
                            
                            <!-- Footer -->
                            <div style=""background-color: #f7fafc; padding: 20px; text-align: center; border-top: 1px solid #edf2f7;"">
                                <p style=""margin: 0; color: #a0aec0; font-size: 13px;"">
                                    &copy; " + DateTime.UtcNow.Year + @" Petoria. Всички права запазени.
                                </p>
                            </div>
                            
                        </div>
                    </div>";

                await _emailService.SendEmailAsync(userEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending cart confirmation email: {ex.Message}");
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

