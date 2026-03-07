using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.SupportMessage;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

using System.Security.Claims;

namespace Petoria.Controllers
{
    [Route("api/support/messages")]
    [ApiController]
    [Authorize]
    public class SupportMessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupportMessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/support/messages
        [HttpPost]
        public async Task<ActionResult<SupportMessageDto>> CreateMessage([FromBody] CreateSupportMessageDto createDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Check for unanswered messages limit
            var unansweredCount = await _context.SupportMessages
                .CountAsync(m => m.UserId == userId && !m.IsAnswered);

            if (unansweredCount >= 10)
            {
                return BadRequest(new { message = "You have reached the maximum limit of 10 unanswered messages." });
            }

            var message = new SupportMessage
            {
                UserId = userId,
                Subject = createDto.Subject,
                Message = createDto.Message,
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);

            return CreatedAtAction(nameof(GetMyMessages), new { id = message.Id }, new SupportMessageDto
            {
                Id = message.Id,
                Subject = message.Subject,
                Message = message.Message,
                IsAnswered = message.IsAnswered,
                CreatedAt = message.CreatedAt,
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                UserEmail = user.Email
            });
        }

        // GET: api/support/messages/my
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<SupportMessageDto>>> GetMyMessages()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var messages = await _context.SupportMessages
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new SupportMessageDto
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Message = m.Message,
                    IsAnswered = m.IsAnswered,
                    AdminResponse = m.AdminResponse,
                    CreatedAt = m.CreatedAt,
                    AnsweredAt = m.AnsweredAt,
                    IsReadByUser = m.IsReadByUser,
                    UserId = m.UserId,
                    // We don't need user details here as it's for the current user
                })
                .ToListAsync();

            return Ok(messages);
        }

        // GET: api/support/messages/admin
        [HttpGet("admin")]
        [Authorize(Roles = Petoria.Constants.Roles.SuperAdmin)]
        public async Task<ActionResult<IEnumerable<SupportMessageDto>>> GetAllMessages()
        {
            var messages = await _context.SupportMessages
                .Include(m => m.User)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new SupportMessageDto
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Message = m.Message,
                    IsAnswered = m.IsAnswered,
                    AdminResponse = m.AdminResponse,
                    CreatedAt = m.CreatedAt,
                    AnsweredAt = m.AnsweredAt,
                    IsReadByUser = m.IsReadByUser,
                    UserId = m.User.Id,
                    UserName = $"{m.User.FirstName} {m.User.LastName}",
                    UserEmail = m.User.Email
                })
                .ToListAsync();

            return Ok(messages);
        }

        // PUT: api/support/messages/{id}/answer
        [HttpPut("{id}/answer")]
        [Authorize(Roles = Petoria.Constants.Roles.SuperAdmin)]
        public async Task<IActionResult> AnswerMessage(int id, [FromBody] AnswerSupportMessageDto answerDto)
        {
            var message = await _context.SupportMessages.FindAsync(id);
            if (message == null) return NotFound();

            message.AdminResponse = answerDto.Response;
            message.IsAnswered = true;
            message.AnsweredAt = DateTime.UtcNow;
            message.IsReadByUser = false; // Mark as unread for the user so they get notified
            message.IsReadByAdmin = true; 

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/support/messages/admin/unread-count
        [HttpGet("admin/unread-count")]
        [Authorize(Roles = Petoria.Constants.Roles.SuperAdmin)]
        public async Task<ActionResult<int>> GetAdminUnreadCount()
        {
            // Count messages that are NOT answered yet
            var count = await _context.SupportMessages
                .CountAsync(m => !m.IsAnswered);

            return Ok(count);
        }

        // PUT: api/support/messages/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var message = await _context.SupportMessages.FindAsync(id);
            if (message == null) return NotFound();

            if (message.UserId != userId) return Forbid();

            message.IsReadByUser = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/support/messages/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = Petoria.Constants.Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.SupportMessages.FindAsync(id);
            if (message == null) return NotFound();

            _context.SupportMessages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
