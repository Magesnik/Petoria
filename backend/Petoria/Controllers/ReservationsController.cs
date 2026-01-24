using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // DTO for creating reservation
    public class CreateReservationRequest
    {
        public int HotelId { get; set; }
        public int RoomTypeId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfRooms { get; set; } = 1;
        public string? Notes { get; set; }
    }

    // DTO for reservation response
    public class ReservationResponse
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string HotelImageUrl { get; set; } = string.Empty;
        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfRooms { get; set; }
        public int NumberOfNights { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO for price calculation
    public class PriceCalculationRequest
    {
        public int RoomTypeId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfRooms { get; set; } = 1;
    }

    public class PriceCalculationResponse
    {
        public int NumberOfNights { get; set; }
        public decimal PricePerNight { get; set; }
        public int NumberOfRooms { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // GET: api/reservations/my - Get current user's reservations
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReservationResponse>>> GetMyReservations()
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
            .Select(r => new ReservationResponse
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
    public async Task<ActionResult<PriceCalculationResponse>> CalculatePrice(PriceCalculationRequest request)
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
        var totalPrice = roomType.PricePerNight * numberOfNights * request.NumberOfRooms;

        return Ok(new PriceCalculationResponse
        {
            NumberOfNights = numberOfNights,
            PricePerNight = roomType.PricePerNight,
            NumberOfRooms = request.NumberOfRooms,
            TotalPrice = totalPrice
        });
    }

    // POST: api/reservations - Create a new reservation
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReservationResponse>> CreateReservation(CreateReservationRequest request)
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

        // Calculate total price
        var numberOfNights = (int)(request.CheckOutDate.Date - request.CheckInDate.Date).TotalDays;
        var totalPrice = roomType.PricePerNight * numberOfNights * request.NumberOfRooms;

        // Create reservation
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

        // Return response
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, new ReservationResponse
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
    public async Task<ActionResult<ReservationResponse>> GetReservation(int id)
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

        return Ok(new ReservationResponse
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
