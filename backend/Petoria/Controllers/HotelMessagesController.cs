using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.HotelMessage;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/hotels")]
[ApiController]
public class HotelMessagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HotelMessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Потребител създава ново съобщение към хотел
    /// POST /api/hotels/{hotelId}/messages
    /// </summary>
    [HttpPost("{hotelId}/messages")]
    [Authorize]
    public async Task<ActionResult> CreateMessage(int hotelId, [FromBody] CreateHotelMessageDto dto)
    {
        // Check if hotel exists
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound("Хотелът не е намерен");
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var message = new HotelMessage
        {
            HotelId = hotelId,
            UserId = userId,
            Subject = dto.Subject,
            Message = dto.Message,
            IsAnswered = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.HotelMessages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Съобщението е изпратено успешно", id = message.Id });
    }

    /// <summary>
    /// Администратор на хотел получава всички съобщения за своя хотел
    /// GET /api/hotels/{hotelId}/messages
    /// </summary>
    [HttpGet("{hotelId}/messages")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<HotelMessageResponseDto>>> GetHotelMessages(int hotelId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Check if hotel exists and user is the owner
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound("Хотелът не е намерен");
        }

        // Check if user owns this hotel, is SuperAdmin, or is a Moderator
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var isModerator = await _context.HotelModerators
            .AnyAsync(hm => hm.HotelId == hotelId && hm.UserId == userId);

        if (hotel.CreatedById != userId && !isSuperAdmin && !isModerator)
        {
            return Forbid();
        }

        var messages = await _context.HotelMessages
            .Where(m => m.HotelId == hotelId)
            .Include(m => m.User)
            .Include(m => m.Hotel)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new HotelMessageResponseDto
            {
                Id = m.Id,
                HotelId = m.HotelId,
                HotelName = m.Hotel.Name,
                UserId = m.UserId,
                UserName = m.User.FirstName + " " + m.User.LastName,
                UserEmail = m.User.Email!,
                Subject = m.Subject,
                Message = m.Message,
                IsAnswered = m.IsAnswered,
                AdminResponse = m.AdminResponse,
                CreatedAt = m.CreatedAt,
                AnsweredAt = m.AnsweredAt
            })
            .ToListAsync();

        return Ok(messages);
    }

    /// <summary>
    /// Администратор отговаря на съобщение
    /// PUT /api/hotels/{hotelId}/messages/{id}/answer
    /// </summary>
    [HttpPut("{hotelId}/messages/{id}/answer")]
    [Authorize]
    public async Task<ActionResult> AnswerMessage(int hotelId, int id, [FromBody] AnswerMessageDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var message = await _context.HotelMessages
            .Include(m => m.Hotel)
            .FirstOrDefaultAsync(m => m.Id == id && m.HotelId == hotelId);

        if (message == null)
        {
            return NotFound("Съобщението не е намерено");
        }

        // Check if user owns this hotel, is SuperAdmin, or is a Moderator
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var isModerator = await _context.HotelModerators
            .AnyAsync(hm => hm.HotelId == hotelId && hm.UserId == userId);

        if (message.Hotel.CreatedById != userId && !isSuperAdmin && !isModerator)
        {
            return Forbid();
        }

        message.AdminResponse = dto.AdminResponse;
        message.IsAnswered = true;
        message.AnsweredAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Отговорът е записан успешно" });
    }

    /// <summary>
    /// Получава брой неотговорени съобщения за всички хотели на администратора
    /// GET /api/hotels/my/messages/unread-counts
    /// </summary>
    [HttpGet("my/messages/unread-counts")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UnreadCountDto>>> GetUnreadCounts()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Get all hotels owned by this admin OR where they are a moderator
        var ownedHotelIds = await _context.Hotels
            .Where(h => h.CreatedById == userId)
            .Select(h => h.Id)
            .ToListAsync();

        var moderatedHotelIds = await _context.HotelModerators
            .Where(hm => hm.UserId == userId)
            .Select(hm => hm.HotelId)
            .ToListAsync();

        var hotelIds = ownedHotelIds.Concat(moderatedHotelIds).Distinct().ToList();

        // Get unread message counts for each hotel
        var unreadCounts = await _context.HotelMessages
            .Where(m => hotelIds.Contains(m.HotelId) && !m.IsAnswered)
            .GroupBy(m => m.HotelId)
            .Select(g => new UnreadCountDto
            {
                HotelId = g.Key,
                UnreadCount = g.Count()
            })
            .ToListAsync();

        return Ok(unreadCounts);
    }

    /// <summary>
    /// Потребител получава своите изпратени съобщения
    /// GET /api/hotels/my/messages
    /// </summary>
    [HttpGet("my/messages")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<HotelMessageResponseDto>>> GetMyMessages()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var messages = await _context.HotelMessages
            .Where(m => m.UserId == userId)
            .Include(m => m.Hotel)
            .Include(m => m.User)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new HotelMessageResponseDto
            {
                Id = m.Id,
                HotelId = m.HotelId,
                HotelName = m.Hotel.Name,
                UserId = m.UserId,
                UserName = m.User.FirstName + " " + m.User.LastName,
                UserEmail = m.User.Email!,
                Subject = m.Subject,
                Message = m.Message,
                IsAnswered = m.IsAnswered,
                AdminResponse = m.AdminResponse,
                CreatedAt = m.CreatedAt,
                AnsweredAt = m.AnsweredAt
            })
            .ToListAsync();

        return Ok(messages);
    }

    /// <summary>
    /// Потребител маркира съобщение като прочетено
    /// PUT /api/hotels/my/messages/{id}/read
    /// </summary>
    [HttpPut("my/messages/{id}/read")]
    [Authorize]
    public async Task<ActionResult> MarkMessageAsRead(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var message = await _context.HotelMessages
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (message == null)
        {
            return NotFound("Съобщението не е намерено");
        }

        message.IsReadByUser = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Съобщението е маркирано като прочетено" });
    }

    /// <summary>
    /// Получава брой непрочетени отговори за потребителя
    /// GET /api/hotels/my/messages/unread-responses-count
    /// </summary>
    [HttpGet("my/messages/unread-responses-count")]
    [Authorize]
    public async Task<ActionResult<UnreadUserResponseCountDto>> GetUnreadUserResponsesCount()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var count = await _context.HotelMessages
            .CountAsync(m => m.UserId == userId && m.IsAnswered && !m.IsReadByUser);

        return Ok(new UnreadUserResponseCountDto { Count = count });
    }
}
