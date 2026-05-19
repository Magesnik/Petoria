using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Comment;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

/// <summary>
/// Контролер за коментари: CRUD операции, нишки с отговори, лайк/дислайк.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CommentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Помощен метод за преобразуване на Comment Entity → CommentResponseDto.
    /// </summary>
    private CommentResponseDto MapToCommentResponse(Comment c, string? currentUserId, bool includeReplies = false)
    {
        var dto = new CommentResponseDto
        {
            Id = c.Id,
            Text = c.Text,
            UserId = c.UserId,
            FirstName = c.User?.FirstName,
            LastName = c.User?.LastName,
            AvatarUrl = c.User?.AvatarUrl,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            LikesCount = c.Ratings?.Count(r => r.IsLike) ?? 0,
            DislikesCount = c.Ratings?.Count(r => !r.IsLike) ?? 0,
            UserRating = currentUserId != null ? c.Ratings?.FirstOrDefault(r => r.UserId == currentUserId)?.IsLike : null,
            RepliesCount = c.Replies?.Count ?? 0
        };

        if (includeReplies && c.Replies != null)
        {
            // Рекурсивно преобразуване на вложените отговори
            // Разчитаме на данните, заредени чрез Include
            dto.Replies = c.Replies
                .OrderBy(r => r.CreatedAt) // Отговорите се подреждат хронологично
                .Select(r => MapToCommentResponse(r, currentUserId, true))
                .ToList();
        }

        return dto;
    }

    /// <summary>
    /// Връща всички коментари от най-високо ниво за даден хотел с вложени отговори.
    /// </summary>
    [HttpGet("~/api/hotels/{hotelId}/comments")]
    public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetHotelComments(int hotelId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var comments = await _context.Comments
            .Where(c => c.HotelId == hotelId && c.ParentCommentId == null) // Само коментари от най-високо ниво
            .Include(c => c.User)
            .Include(c => c.Ratings)
            // Ниво 1
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Ratings)
            // Ниво 2
            .Include(c => c.Replies)
                .ThenInclude(r => r.Replies)
                    .ThenInclude(rr => rr.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Replies)
                    .ThenInclude(rr => rr.Ratings)
            // Ниво 3
            .Include(c => c.Replies)
                .ThenInclude(r => r.Replies)
                    .ThenInclude(rr => rr.Replies)
                        .ThenInclude(rrr => rrr.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Replies)
                    .ThenInclude(rr => rr.Replies)
                        .ThenInclude(rrr => rrr.Ratings)
            .OrderByDescending(c => c.Ratings.Count(r => r.IsLike) - c.Ratings.Count(r => !r.IsLike)) // Сортиране по нетни лайкове
            .ToListAsync();

        var result = comments.Select(c => MapToCommentResponse(c, userId, includeReplies: true));

        return Ok(result);
    }

    /// <summary>
    /// Създава нов коментар или отговор на съществуващ коментар.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CommentResponseDto>> CreateComment([FromBody] CreateCommentDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Валидация дали хотелът съществува
        var hotel = await _context.Hotels.FindAsync(request.HotelId);
        if (hotel == null)
        {
            return NotFound("Hotel not found");
        }

        // Валидация дали родителският коментар съществува (ако е отговор)
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _context.Comments.FindAsync(request.ParentCommentId.Value);
            if (parentComment == null)
            {
                return NotFound("Parent comment not found");
            }
        }

        // Преобразуване на DTO → Entity
        var comment = new Comment
        {
            HotelId = request.HotelId,
            UserId = userId,
            Text = request.Text,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Презареждане с информация за потребителя
        await _context.Entry(comment).Reference(c => c.User).LoadAsync();

        // Преобразуване на Entity → Response DTO
        return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, new CommentResponseDto
        {
            Id = comment.Id,
            Text = comment.Text,
            UserId = comment.UserId,
            FirstName = comment.User?.FirstName,
            LastName = comment.User?.LastName,
            AvatarUrl = comment.User?.AvatarUrl,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            LikesCount = 0,
            DislikesCount = 0,
            UserRating = null,
            RepliesCount = 0
        });
    }

    /// <summary>
    /// Връща конкретен коментар по ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CommentResponseDto>> GetComment(int id)
    {
        var comment = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Ratings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Ok(MapToCommentResponse(comment, userId));
    }

    /// <summary>
    /// Редактира текста на собствен коментар.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        // Позволено е редактиране само на собствени коментари
        if (comment.UserId != userId)
        {
            return Forbid();
        }

        // Преобразуване на DTO → Entity (обновяване)
        comment.Text = request.Text;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Изтрива коментар заедно с всички отговори и оценки.
    /// SuperAdmin може да трие навсякъде.
    /// Admin може да трие само в хотелите, които е създал.
    /// HotelModerator може да трие само в хотелите, които модерира.
    /// Обикновен потребител може да трие само собствените си коментари.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var comment = await _context.Comments
            .Include(c => c.Replies)
            .Include(c => c.Ratings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            return NotFound();
        }

        var isSuperAdmin = User.IsInRole(Petoria.Constants.Roles.SuperAdmin);
        var isAdmin = User.IsInRole(Petoria.Constants.Roles.Admin);
        var isModerator = User.IsInRole(Petoria.Constants.Roles.HotelModerator);

        if (!isSuperAdmin)
        {
            if (isAdmin)
            {
                // Admin може да трие само коментари в хотелите, които е създал
                var isOwnerOfHotel = await _context.Hotels
                    .AnyAsync(h => h.Id == comment.HotelId && h.CreatedById == userId);

                if (!isOwnerOfHotel)
                {
                    return Forbid();
                }
            }
            else if (isModerator)
            {
                // HotelModerator може да трие само коментари в хотелите, които модерира
                var isModeratorOfHotel = await _context.HotelModerators
                    .AnyAsync(hm => hm.HotelId == comment.HotelId && hm.UserId == userId);

                if (!isModeratorOfHotel)
                {
                    return Forbid();
                }
            }
            else
            {
                // Обикновен потребител — само собствени коментари
                if (comment.UserId != userId)
                {
                    return Forbid();
                }
            }
        }

        // Първо изтриваме всички оценки
        _context.CommentRatings.RemoveRange(comment.Ratings);

        // Изтриваме всички отговори и техните оценки
        foreach (var reply in comment.Replies)
        {
            var replyRatings = await _context.CommentRatings.Where(r => r.CommentId == reply.Id).ToListAsync();
            _context.CommentRatings.RemoveRange(replyRatings);
        }
        _context.Comments.RemoveRange(comment.Replies);

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Поставя лайк или дислайк на коментар (превключва при повторно натискане).
    /// </summary>
    [HttpPost("{id}/rate")]
    [Authorize]
    public async Task<IActionResult> RateComment(int id, [FromBody] RateCommentDto request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        // Проверка дали потребителят вече е оценил този коментар
        var existingRating = await _context.CommentRatings
            .FirstOrDefaultAsync(r => r.CommentId == id && r.UserId == userId);

        if (existingRating != null)
        {
            // Ако същата оценка е натисната отново — премахваме я
            if (existingRating.IsLike == request.IsLike)
            {
                _context.CommentRatings.Remove(existingRating);
            }
            else
            {
                // Обновяване към противоположна оценка
                existingRating.IsLike = request.IsLike;
            }
        }
        else
        {
            // Създаване на нова оценка
            var rating = new CommentRating
            {
                CommentId = id,
                UserId = userId,
                IsLike = request.IsLike,
                CreatedAt = DateTime.UtcNow
            };
            _context.CommentRatings.Add(rating);
        }

        await _context.SaveChangesAsync();

        // Вземане на актуализираните бройки
        var likesCount = await _context.CommentRatings.CountAsync(r => r.CommentId == id && r.IsLike);
        var dislikesCount = await _context.CommentRatings.CountAsync(r => r.CommentId == id && !r.IsLike);
        var userRating = await _context.CommentRatings
            .Where(r => r.CommentId == id && r.UserId == userId)
            .Select(r => (bool?)r.IsLike)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            likesCount,
            dislikesCount,
            userRating
        });
    }

    /// <summary>
    /// Връща отговорите на конкретен коментар.
    /// </summary>
    [HttpGet("{id}/replies")]
    public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetCommentReplies(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var replies = await _context.Comments
            .Where(c => c.ParentCommentId == id)
            .Include(c => c.User)
            .Include(c => c.Ratings)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var result = replies.Select(r => MapToCommentResponse(r, userId));

        return Ok(result);
    }
}
