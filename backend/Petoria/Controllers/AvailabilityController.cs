using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.DTOs.Availability;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/hotels/{hotelId}/availability")]
[ApiController]
public class AvailabilityController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AvailabilityController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/hotels/5/availability?from=2026-01-25&to=2026-02-25
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvailabilityResponseDto>>> GetAvailability(
        int hotelId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        // Get room types for this hotel
        var roomTypes = await _context.RoomTypes
            .Where(rt => rt.HotelId == hotelId)
            .ToListAsync();

        if (!roomTypes.Any())
        {
            return Ok(new List<AvailabilityResponseDto>());
        }

        // Get existing availability records
        var roomTypeIds = roomTypes.Select(rt => rt.Id).ToList();
        var availabilityRecords = await _context.RoomAvailabilities
            .Where(ra => roomTypeIds.Contains(ra.RoomTypeId) && 
                         ra.Date >= from.Date && 
                         ra.Date <= to.Date)
            .ToListAsync();

        // Get active discounts for this date range
        var discounts = await _context.RoomDiscounts
            .Where(rd => roomTypeIds.Contains(rd.RoomTypeId) && 
                         rd.EndDate >= from.Date && 
                         rd.StartDate <= to.Date)
            .ToListAsync();

        var result = new List<AvailabilityResponseDto>();

        // Generate availability data for each date and room type
        for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
        {
            foreach (var roomType in roomTypes)
            {
                var existingRecord = availabilityRecords
                    .FirstOrDefault(ra => ra.RoomTypeId == roomType.Id && ra.Date == date);

                // Find active discount for this date and room type
                var activeDiscount = discounts.FirstOrDefault(d => 
                    d.RoomTypeId == roomType.Id && 
                    date >= d.StartDate.Date && 
                    date <= d.EndDate.Date);

                result.Add(new AvailabilityResponseDto
                {
                    RoomTypeId = roomType.Id,
                    RoomTypeName = roomType.Name,
                    PricePerNight = roomType.PricePerNight,
                    Capacity = roomType.Capacity,
                    Date = date,
                    // If no record exists, use TotalRooms as available count
                    AvailableCount = existingRecord?.AvailableCount ?? roomType.TotalRooms,
                    IsBlocked = existingRecord?.IsBlocked ?? false,
                    DiscountPercentage = activeDiscount?.DiscountPercentage,
                    DiscountedPrice = activeDiscount != null 
                        ? roomType.PricePerNight * (1 - activeDiscount.DiscountPercentage / 100m) 
                        : null
                });
            }
        }

        return Ok(result.OrderBy(a => a.Date).ThenBy(a => a.RoomTypeId));
    }

    // POST: api/hotels/5/availability/bulk - Set availability for a date range
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetBulkAvailability(int hotelId, BulkAvailabilityRequestDto request)
    {
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        // Check authorization
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (!isSuperAdmin && hotel.CreatedById != userId)
        {
            return Forbid("You can only manage availability for hotels that you created");
        }

        // Verify room type belongs to this hotel
        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt => rt.Id == request.RoomTypeId && rt.HotelId == hotelId);

        if (roomType == null)
        {
            return NotFound(new { message = "Room type not found" });
        }

        // Update or create availability records for each date
        for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
        {
            var existingRecord = await _context.RoomAvailabilities
                .FirstOrDefaultAsync(ra => ra.RoomTypeId == request.RoomTypeId && ra.Date == date);

            if (existingRecord != null)
            {
                existingRecord.AvailableCount = request.AvailableCount;
                existingRecord.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.RoomAvailabilities.Add(new RoomAvailability
                {
                    RoomTypeId = request.RoomTypeId,
                    Date = date,
                    AvailableCount = request.AvailableCount,
                    IsBlocked = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Availability updated successfully" });
    }

    // PUT: api/hotels/5/availability/block - Block or unblock dates
    [HttpPut("block")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BlockDates(int hotelId, BlockDatesRequestDto request)
    {
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        // Check authorization
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (!isSuperAdmin && hotel.CreatedById != userId)
        {
            return Forbid("You can only manage availability for hotels that you created");
        }

        // Get room types to block
        var roomTypes = await _context.RoomTypes
            .Where(rt => rt.HotelId == hotelId && 
                        (request.RoomTypeId == null || rt.Id == request.RoomTypeId))
            .ToListAsync();

        if (!roomTypes.Any())
        {
            return NotFound(new { message = "Room types not found" });
        }

        // Block/unblock each date for each room type
        foreach (var roomType in roomTypes)
        {
            for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
            {
                var existingRecord = await _context.RoomAvailabilities
                    .FirstOrDefaultAsync(ra => ra.RoomTypeId == roomType.Id && ra.Date == date);

                if (existingRecord != null)
                {
                    existingRecord.IsBlocked = request.IsBlocked;
                    existingRecord.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.RoomAvailabilities.Add(new RoomAvailability
                    {
                        RoomTypeId = roomType.Id,
                        Date = date,
                        AvailableCount = roomType.TotalRooms,
                        IsBlocked = request.IsBlocked,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = request.IsBlocked ? "Dates blocked successfully" : "Dates unblocked successfully" });
    }

    // POST: api/hotels/5/availability/initialize - Initialize availability for all room types
    [HttpPost("initialize")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> InitializeAvailability(int hotelId, [FromQuery] int daysAhead = 90)
    {
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        // Check authorization
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (!isSuperAdmin && hotel.CreatedById != userId)
        {
            return Forbid("You can only manage availability for hotels that you created");
        }

        var roomTypes = await _context.RoomTypes
            .Where(rt => rt.HotelId == hotelId)
            .ToListAsync();

        if (!roomTypes.Any())
        {
            return BadRequest(new { message = "No room types found. Please add room types first." });
        }

        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(daysAhead);

        foreach (var roomType in roomTypes)
        {
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var exists = await _context.RoomAvailabilities
                    .AnyAsync(ra => ra.RoomTypeId == roomType.Id && ra.Date == date);

                if (!exists)
                {
                    _context.RoomAvailabilities.Add(new RoomAvailability
                    {
                        RoomTypeId = roomType.Id,
                        Date = date,
                        AvailableCount = roomType.TotalRooms,
                        IsBlocked = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = $"Availability initialized for {daysAhead} days" });
    }
}
