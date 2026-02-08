using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;
using System.Security.Claims;

namespace Petoria.Controllers;

[Route("api")]
[ApiController]
public class DiscountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DiscountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/hotels/{hotelId}/discounts
    [HttpGet("hotels/{hotelId}/discounts")]
    public async Task<ActionResult<IEnumerable<object>>> GetHotelDiscounts(int hotelId)
    {
        var discounts = await _context.RoomDiscounts
            .Include(rd => rd.RoomType)
            .Where(rd => rd.RoomType!.HotelId == hotelId)
            .OrderByDescending(rd => rd.StartDate)
            .Select(rd => new
            {
                rd.Id,
                rd.RoomTypeId,
                RoomTypeName = rd.RoomType!.Name,
                rd.StartDate,
                rd.EndDate,
                rd.DiscountPercentage,
                rd.CreatedAt,
                rd.UpdatedAt,
                IsActive = rd.StartDate <= DateTime.UtcNow && rd.EndDate >= DateTime.UtcNow,
                IsExpired = rd.EndDate < DateTime.UtcNow
            })
            .ToListAsync();

        return Ok(discounts);
    }

    // GET: api/rooms/{roomTypeId}/active-discount
    [HttpGet("rooms/{roomTypeId}/active-discount")]
    public async Task<ActionResult<object>> GetActiveDiscount(int roomTypeId, [FromQuery] DateTime? date)
    {
        var checkDate = date ?? DateTime.UtcNow;

        var discount = await _context.RoomDiscounts
            .Where(rd => rd.RoomTypeId == roomTypeId &&
                        rd.StartDate <= checkDate &&
                        rd.EndDate >= checkDate)
            .OrderByDescending(rd => rd.DiscountPercentage)
            .FirstOrDefaultAsync();

        if (discount == null)
        {
            return NotFound(new { message = "No active discount found" });
        }

        return Ok(new
        {
            discount.Id,
            discount.DiscountPercentage,
            discount.StartDate,
            discount.EndDate
        });
    }

    // POST: api/hotels/{hotelId}/discounts
    [Authorize]
    [HttpPost("hotels/{hotelId}/discounts")]
    public async Task<ActionResult<object>> CreateDiscount(int hotelId, [FromBody] CreateDiscountDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Verify hotel ownership
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound(new { message = "Hotel not found" });
        }

        var isSuperAdmin = User.IsInRole("SuperAdmin");
        if (hotel.CreatedById != userId && !isSuperAdmin)
        {
            return Forbid();
        }

        // Verify room type belongs to hotel
        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt => rt.Id == dto.RoomTypeId && rt.HotelId == hotelId);
        
        if (roomType == null)
        {
            return BadRequest(new { message = "Room type not found or does not belong to this hotel" });
        }

        // Validate dates
        if (dto.EndDate <= dto.StartDate)
        {
            return BadRequest(new { message = "End date must be after start date" });
        }

        // Create discount
        var discount = new RoomDiscount
        {
            RoomTypeId = dto.RoomTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            DiscountPercentage = dto.DiscountPercentage,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.RoomDiscounts.Add(discount);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            discount.Id,
            discount.RoomTypeId,
            RoomTypeName = roomType.Name,
            discount.StartDate,
            discount.EndDate,
            discount.DiscountPercentage,
            discount.CreatedAt,
            discount.UpdatedAt
        });
    }

    // PUT: api/discounts/{id}
    [Authorize]
    [HttpPut("discounts/{id}")]
    public async Task<IActionResult> UpdateDiscount(int id, [FromBody] UpdateDiscountDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var discount = await _context.RoomDiscounts
            .Include(rd => rd.RoomType)
                .ThenInclude(rt => rt!.Hotel)
            .FirstOrDefaultAsync(rd => rd.Id == id);

        if (discount == null)
        {
            return NotFound();
        }

        // Verify ownership
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        if (discount.RoomType?.Hotel?.CreatedById != userId && !isSuperAdmin)
        {
            return Forbid();
        }

        // Validate dates
        if (dto.EndDate <= dto.StartDate)
        {
            return BadRequest(new { message = "End date must be after start date" });
        }

        // Update discount
        discount.StartDate = dto.StartDate;
        discount.EndDate = dto.EndDate;
        discount.DiscountPercentage = dto.DiscountPercentage;
        discount.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/discounts/{id}
    [Authorize]
    [HttpDelete("discounts/{id}")]
    public async Task<IActionResult> DeleteDiscount(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var discount = await _context.RoomDiscounts
            .Include(rd => rd.RoomType)
                .ThenInclude(rt => rt!.Hotel)
            .FirstOrDefaultAsync(rd => rd.Id == id);

        if (discount == null)
        {
            return NotFound();
        }

        // Verify ownership
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        if (discount.RoomType?.Hotel?.CreatedById != userId && !isSuperAdmin)
        {
            return Forbid();
        }

        _context.RoomDiscounts.Remove(discount);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// DTOs
public class CreateDiscountDto
{
    public int RoomTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DiscountPercentage { get; set; }
}

public class UpdateDiscountDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DiscountPercentage { get; set; }
}
