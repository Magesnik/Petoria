using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Comment;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

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
    /// Помощен метод за map-ване на Comment Entity → CommentResponseDto.
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
            dto.Replies = c.Replies.Select(r => MapToCommentResponse(r, currentUserId, false)).ToList();
        }

        return dto;
    }

    // GET: api/hotels/{hotelId}/comments
    [HttpGet("~/api/hotels/{hotelId}/comments")]
    public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetHotelComments(int hotelId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var comments = await _context.Comments
            .Where(c => c.HotelId == hotelId && c.ParentCommentId == null) // Only top-level comments
            .Include(c => c.User)
            .Include(c => c.Ratings)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Ratings)
            .OrderByDescending(c => c.Ratings.Count(r => r.IsLike) - c.Ratings.Count(r => !r.IsLike)) // Sort by net likes
            .ToListAsync();

        var result = comments.Select(c => MapToCommentResponse(c, userId, includeReplies: true));

        return Ok(result);
    }

    // POST: api/comments
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CommentResponseDto>> CreateComment([FromBody] CreateCommentDto request)
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
            return NotFound("Hotel not found");
        }

        // Validate parent comment exists if it's a reply
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _context.Comments.FindAsync(request.ParentCommentId.Value);
            if (parentComment == null)
            {
                return NotFound("Parent comment not found");
            }
        }

        // Map DTO → Entity
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

        // Reload with user info
        await _context.Entry(comment).Reference(c => c.User).LoadAsync();

        // Map Entity → Response DTO
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

    // GET: api/comments/{id}
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

    // PUT: api/comments/{id}
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

        // Only allow updating own comments
        if (comment.UserId != userId)
        {
            return Forbid();
        }

        // Map DTO → Entity (update)
        comment.Text = request.Text;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/comments/{id}
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

        // Only allow deleting own comments
        if (comment.UserId != userId)
        {
            return Forbid();
        }

        // Delete all ratings first
        _context.CommentRatings.RemoveRange(comment.Ratings);

        // Delete all replies and their ratings
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

    // POST: api/comments/{id}/rate
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

        // Check if user already rated this comment
        var existingRating = await _context.CommentRatings
            .FirstOrDefaultAsync(r => r.CommentId == id && r.UserId == userId);

        if (existingRating != null)
        {
            // If same rating clicked again, remove it
            if (existingRating.IsLike == request.IsLike)
            {
                _context.CommentRatings.Remove(existingRating);
            }
            else
            {
                // Update to opposite rating
                existingRating.IsLike = request.IsLike;
            }
        }
        else
        {
            // Create new rating
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

        // Get updated counts
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

    // GET: api/comments/{id}/replies
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
