using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.DTOs.Reservation;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReservationsController(ApplicationDbContext context)
    {
        _context = context;
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

        // Only allow user to see their own reservations (or SuperAdmin)
        if (!isSuperAdmin && reservation.UserId != userId)
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

        // Only allow user to cancel their own reservations (or SuperAdmin)
        if (!isSuperAdmin && reservation.UserId != userId)
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

        return Ok(new { message = "Reservation cancelled successfully" });
    }
}
