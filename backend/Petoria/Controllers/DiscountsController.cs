using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Discount;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;
using System.Security.Claims;

namespace Petoria.Controllers;

/// <summary>
/// Контролер за отстъпки на стаи: CRUD за периодични отстъпки, активна отстъпка по дата
/// </summary>
[Route("api")]
[ApiController]
public class DiscountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DiscountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Връща всички отстъпки за даден хотел
    /// </summary>
    [HttpGet("hotels/{hotelId}/discounts")]
    public async Task<ActionResult<IEnumerable<DiscountResponseDto>>> GetHotelDiscounts(int hotelId)
    {
        var discounts = await _context.RoomDiscounts
            .Include(rd => rd.RoomType)
            .Where(rd => rd.RoomType!.HotelId == hotelId)
            .OrderByDescending(rd => rd.StartDate)
            .Select(rd => new DiscountResponseDto
            {
                Id = rd.Id,
                RoomTypeId = rd.RoomTypeId,
                RoomTypeName = rd.RoomType!.Name,
                StartDate = rd.StartDate,
                EndDate = rd.EndDate,
                DiscountPercentage = rd.DiscountPercentage,
                CreatedAt = rd.CreatedAt,
                UpdatedAt = rd.UpdatedAt,
                IsActive = rd.StartDate <= DateTime.UtcNow && rd.EndDate >= DateTime.UtcNow,
                IsExpired = rd.EndDate < DateTime.UtcNow
            })
            .ToListAsync();

        return Ok(discounts);
    }

    /// <summary>
    /// Връща активната отстъпка за тип стая по дата
    /// </summary>
    [HttpGet("rooms/{roomTypeId}/active-discount")]
    public async Task<ActionResult<DiscountResponseDto>> GetActiveDiscount(int roomTypeId, [FromQuery] DateTime? date)
    {
        var checkDate = date ?? DateTime.UtcNow;

        var discount = await _context.RoomDiscounts
            .Include(rd => rd.RoomType)
            .Where(rd => rd.RoomTypeId == roomTypeId &&
                        rd.StartDate <= checkDate &&
                        rd.EndDate >= checkDate)
            .OrderByDescending(rd => rd.DiscountPercentage)
            .Select(rd => new DiscountResponseDto
            {
                Id = rd.Id,
                RoomTypeId = rd.RoomTypeId,
                RoomTypeName = rd.RoomType!.Name,
                StartDate = rd.StartDate,
                EndDate = rd.EndDate,
                DiscountPercentage = rd.DiscountPercentage,
                CreatedAt = rd.CreatedAt,
                UpdatedAt = rd.UpdatedAt,
                IsActive = true,
                IsExpired = false
            })
            .FirstOrDefaultAsync();

        if (discount == null)
        {
            return NotFound(new { message = "No active discount found" });
        }

        return Ok(discount);
    }

    /// <summary>
    /// Създава нова отстъпка за тип стая в хотел
    /// </summary>
    [Authorize]
    [HttpPost("hotels/{hotelId}/discounts")]
    public async Task<ActionResult<DiscountResponseDto>> CreateDiscount(int hotelId, [FromBody] CreateDiscountDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Проверка на собствеността върху хотела
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

        // Проверка дали типът стая принадлежи на хотела
        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt => rt.Id == dto.RoomTypeId && rt.HotelId == hotelId);

        if (roomType == null)
        {
            return BadRequest(new { message = "Room type not found or does not belong to this hotel" });
        }

        // Валидиране на датите
        if (dto.EndDate <= dto.StartDate)
        {
            return BadRequest(new { message = "End date must be after start date" });
        }

        // Преобразуване на DTO → Entity
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

        // Преобразуване на Entity → Response DTO
        return Ok(new DiscountResponseDto
        {
            Id = discount.Id,
            RoomTypeId = discount.RoomTypeId,
            RoomTypeName = roomType.Name,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            DiscountPercentage = discount.DiscountPercentage,
            CreatedAt = discount.CreatedAt,
            UpdatedAt = discount.UpdatedAt,
            IsActive = discount.StartDate <= DateTime.UtcNow && discount.EndDate >= DateTime.UtcNow,
            IsExpired = false
        });
    }

    /// <summary>
    /// Обновява съществуваща отстъпка по идентификатор
    /// </summary>
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

        // Проверка на собствеността
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        if (discount.RoomType?.Hotel?.CreatedById != userId && !isSuperAdmin)
        {
            return Forbid();
        }

        // Валидиране на датите
        if (dto.EndDate <= dto.StartDate)
        {
            return BadRequest(new { message = "End date must be after start date" });
        }

        // Преобразуване на DTO → Entity (обновяване)
        discount.StartDate = dto.StartDate;
        discount.EndDate = dto.EndDate;
        discount.DiscountPercentage = dto.DiscountPercentage;
        discount.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Изтрива отстъпка по идентификатор
    /// </summary>
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

        // Проверка на собствеността
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
