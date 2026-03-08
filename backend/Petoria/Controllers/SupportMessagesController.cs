using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.Contracts;
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
        private readonly IEmailService _emailService;

        public SupportMessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
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

            // Send Email Notification to Petoria Support
            try
            {
                var emailSubject = $"New Support Message: {message.Subject}";
                var emailBody = $@"
                    <h2>New Support Message Received</h2>
                    <p><strong>From:</strong> {user.FirstName} {user.LastName} ({user.Email})</p>
                    <p><strong>Subject:</strong> {message.Subject}</p>
                    <p><strong>Message:</strong></p>
                    <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #007bff; margin-top: 10px;'>
                        {message.Message.Replace("\n", "<br>")}
                    </div>
                    <br>
                    <p><small>You can reply to this message directly from the <a href='https://petoria.com/admin/support-messages'>Petoria Admin Panel</a>.</small></p>
                ";
                await _emailService.SendEmailAsync("petooriaa@gmail.com", emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                // We log the exception but don't fail the request if email sending fails.
                Console.WriteLine($"Failed to send support email: {ex.Message}");
            }

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

        // GET: api/support/messages/my/unread-count
        [HttpGet("my/unread-count")]
        public async Task<ActionResult<int>> GetMyUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Count messages that the user sent, which are answered but the user hasn't read them yet
            var count = await _context.SupportMessages
                .CountAsync(m => m.UserId == userId && m.IsAnswered && !m.IsReadByUser);

            // We must return an object with a 'count' field to match frontend expectations
            // Frontend expects: data.count
            return Ok(new { count = count });
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

        // DELETE: api/support/messages/my/{id}
        [HttpDelete("my/{id}")]
        public async Task<IActionResult> DeleteMyMessage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var message = await _context.SupportMessages.FindAsync(id);
            if (message == null) return NotFound();

            if (message.UserId != userId) return Forbid();

            if (message.IsAnswered)
            {
                return BadRequest(new { message = "You cannot delete a message that has already been answered." });
            }

            _context.SupportMessages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
