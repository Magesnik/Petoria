using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.DTOs.PromoCode;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;
using System.Security.Claims;

namespace Petoria.Controllers;

[ApiController]
[Route("api")]
public class PromoCodesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PromoCodesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("hotels/{hotelId}/promocodes")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PromoCodeDto>>> GetPromoCodes(int hotelId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var hotel = await _context.Hotels.FindAsync(hotelId);

        if (hotel == null) return NotFound("Hotel not found");

        // Authorization check: Owner or Moderator or Admin
        var isOwner = hotel.CreatedById == userId;
        var isModerator = await _context.HotelModerators.AnyAsync(m => m.HotelId == hotelId && m.UserId == userId);
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        if (!isOwner && !isModerator && !isAdmin)
        {
            return Forbid();
        }

        var promoCodes = await _context.PromoCodes
            .Where(p => p.HotelId == hotelId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PromoCodeDto
            {
                Id = p.Id,
                HotelId = p.HotelId,
                Code = p.Code,
                DiscountPercentage = p.DiscountPercentage,
                MaxActivations = p.MaxActivations,
                CurrentActivations = p.CurrentActivations,
                ExpirationDate = p.ExpirationDate,
                CreatedAt = p.CreatedAt,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(promoCodes);
    }

    [HttpGet("promocodes/global")]
    [Authorize(Roles = Petoria.Constants.Roles.SuperAdmin)]
    public async Task<ActionResult<IEnumerable<PromoCodeDto>>> GetGlobalPromoCodes()
    {
        var promoCodes = await _context.PromoCodes
            .Where(p => p.HotelId == null)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PromoCodeDto
            {
                Id = p.Id,
                HotelId = p.HotelId,
                Code = p.Code,
                DiscountPercentage = p.DiscountPercentage,
                MaxActivations = p.MaxActivations,
                CurrentActivations = p.CurrentActivations,
                ExpirationDate = p.ExpirationDate,
                CreatedAt = p.CreatedAt,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(promoCodes);
    }

    [HttpPost("promocodes/global")]
    [Authorize(Roles = Petoria.Constants.Roles.SuperAdmin)]
    public async Task<ActionResult<PromoCodeDto>> CreateGlobalPromoCode(CreatePromoCodeDto dto)
    {
        if (await _context.PromoCodes.AnyAsync(p => p.HotelId == null && p.Code == dto.Code))
        {
            return BadRequest("A global promo code with this code already exists.");
        }

        var promoCode = new PromoCode
        {
            HotelId = null,
            Code = dto.Code.ToUpper(),
            DiscountPercentage = dto.DiscountPercentage,
            MaxActivations = dto.MaxActivations,
            CurrentActivations = 0,
            ExpirationDate = DateTime.UtcNow.AddDays(dto.ValidDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.PromoCodes.Add(promoCode);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGlobalPromoCodes), new { }, new PromoCodeDto
        {
            Id = promoCode.Id,
            HotelId = promoCode.HotelId,
            Code = promoCode.Code,
            DiscountPercentage = promoCode.DiscountPercentage,
            MaxActivations = promoCode.MaxActivations,
            CurrentActivations = promoCode.CurrentActivations,
            ExpirationDate = promoCode.ExpirationDate,
            CreatedAt = promoCode.CreatedAt,
            IsActive = promoCode.IsActive
        });
    }


    [HttpPost("hotels/{hotelId}/promocodes")]
    [Authorize]
    public async Task<ActionResult<PromoCodeDto>> CreatePromoCode(int hotelId, CreatePromoCodeDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var hotel = await _context.Hotels.FindAsync(hotelId);

        if (hotel == null) return NotFound("Hotel not found");

        var isOwner = hotel.CreatedById == userId;
        var isModerator = await _context.HotelModerators.AnyAsync(m => m.HotelId == hotelId && m.UserId == userId);
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        if (!isOwner && !isModerator && !isAdmin)
        {
            return Forbid();
        }
        
        // Check if code exists for this hotel
        if (await _context.PromoCodes.AnyAsync(p => p.HotelId == hotelId && p.Code == dto.Code))
        {
            return BadRequest("Promo code with this code already exists for this hotel.");
        }

        var promoCode = new PromoCode
        {
            HotelId = hotelId,
            Code = dto.Code.ToUpper(),
            DiscountPercentage = dto.DiscountPercentage,
            MaxActivations = dto.MaxActivations,
            CurrentActivations = 0,
            ExpirationDate = DateTime.UtcNow.AddDays(dto.ValidDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.PromoCodes.Add(promoCode);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPromoCodes), new { hotelId }, new PromoCodeDto
        {
            Id = promoCode.Id,
            HotelId = promoCode.HotelId,
            Code = promoCode.Code,
            DiscountPercentage = promoCode.DiscountPercentage,
            MaxActivations = promoCode.MaxActivations,
            CurrentActivations = promoCode.CurrentActivations,
            ExpirationDate = promoCode.ExpirationDate,
            CreatedAt = promoCode.CreatedAt,
            IsActive = promoCode.IsActive
        });
    }

    [HttpPut("promocodes/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePromoCode(int id, UpdatePromoCodeDto dto)
    {
        var promoCode = await _context.PromoCodes.FindAsync(id);
        if (promoCode == null) return NotFound("Promo code not found");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        if (promoCode.HotelId == null)
        {
            if (!User.IsInRole("SuperAdmin")) return Forbid();
        }
        else
        {
            var hotel = await _context.Hotels.FindAsync(promoCode.HotelId);
            if (hotel == null) return NotFound("Hotel associated with promo code not found");

            var isOwner = hotel.CreatedById == userId;
            var isModerator = await _context.HotelModerators.AnyAsync(m => m.HotelId == promoCode.HotelId && m.UserId == userId);

            if (!isOwner && !isModerator && !isAdmin)
            {
                return Forbid();
            }
        }

        promoCode.DiscountPercentage = dto.DiscountPercentage;
        promoCode.MaxActivations = dto.MaxActivations;
        
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("promocodes/{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePromoCode(int id)
    {
        var promoCode = await _context.PromoCodes.FindAsync(id);
        if (promoCode == null) return NotFound("Promo code not found");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        if (promoCode.HotelId == null)
        {
            if (!User.IsInRole("SuperAdmin")) return Forbid();
        }
        else
        {
            var hotel = await _context.Hotels.FindAsync(promoCode.HotelId);
            if (hotel == null) return NotFound("Hotel not found");

            var isOwner = hotel.CreatedById == userId;
            var isModerator = await _context.HotelModerators.AnyAsync(m => m.HotelId == promoCode.HotelId && m.UserId == userId);

            if (!isOwner && !isModerator && !isAdmin)
            {
                return Forbid();
            }
        }

        _context.PromoCodes.Remove(promoCode);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("promocodes/validate")]
    public async Task<ActionResult<PromoCodeValidationResponseDto>> ValidatePromoCode([FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return BadRequest(new { message = "Моля, въведете промо код." });

        code = code.ToUpper();
        var promoCode = await _context.PromoCodes
            .FirstOrDefaultAsync(p => p.Code == code);

        if (promoCode == null || promoCode.CurrentActivations >= promoCode.MaxActivations || promoCode.ExpirationDate < DateTime.UtcNow)
        {
            return NotFound(new { message = "Невалиден или изтекъл промо код." });
        }

        return Ok(new PromoCodeValidationResponseDto
        {
            DiscountPercentage = promoCode.DiscountPercentage,
            HotelId = promoCode.HotelId,
            Code = promoCode.Code
        });
    }
}
