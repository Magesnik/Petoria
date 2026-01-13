using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // GET: api/hotels/{hotelId}/comments
    [HttpGet("~/api/hotels/{hotelId}/comments")]
    public async Task<ActionResult> GetHotelComments(int hotelId)
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

        var result = comments.Select(c => new
        {
            id = c.Id,
            text = c.Text,
            userId = c.UserId,
            firstName = c.User.FirstName,
            lastName = c.User.LastName,
            avatarUrl = c.User.AvatarUrl,
            createdAt = c.CreatedAt,
            updatedAt = c.UpdatedAt,
            likesCount = c.Ratings.Count(r => r.IsLike),
            dislikesCount = c.Ratings.Count(r => !r.IsLike),
            userRating = userId != null ? c.Ratings.FirstOrDefault(r => r.UserId == userId)?.IsLike : null,
            repliesCount = c.Replies.Count,
            replies = c.Replies.Select(r => new
            {
                id = r.Id,
                text = r.Text,
                userId = r.UserId,
                firstName = r.User.FirstName,
                lastName = r.User.LastName,
                avatarUrl = r.User.AvatarUrl,
                createdAt = r.CreatedAt,
                updatedAt = r.UpdatedAt,
                likesCount = r.Ratings.Count(rt => rt.IsLike),
                dislikesCount = r.Ratings.Count(rt => !rt.IsLike),
                userRating = userId != null ? r.Ratings.FirstOrDefault(rt => rt.UserId == userId)?.IsLike : null
            }).ToList()
        });

        return Ok(result);
    }

    // POST: api/comments
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateComment([FromBody] CreateCommentRequest request)
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

        return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, new
        {
            id = comment.Id,
            text = comment.Text,
            userId = comment.UserId,
            firstName = comment.User.FirstName,
            lastName = comment.User.LastName,
            avatarUrl = comment.User.AvatarUrl,
            createdAt = comment.CreatedAt,
            updatedAt = comment.UpdatedAt,
            likesCount = 0,
            dislikesCount = 0,
            userRating = (bool?)null,
            repliesCount = 0
        });
    }

    // GET: api/comments/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetComment(int id)
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

        return Ok(new
        {
            id = comment.Id,
            text = comment.Text,
            userId = comment.UserId,
            firstName = comment.User.FirstName,
            lastName = comment.User.LastName,
            avatarUrl = comment.User.AvatarUrl,
            createdAt = comment.CreatedAt,
            updatedAt = comment.UpdatedAt,
            likesCount = comment.Ratings.Count(r => r.IsLike),
            dislikesCount = comment.Ratings.Count(r => !r.IsLike),
            userRating = userId != null ? comment.Ratings.FirstOrDefault(r => r.UserId == userId)?.IsLike : null
        });
    }

    // PUT: api/comments/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
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
    public async Task<IActionResult> RateComment(int id, [FromBody] RateCommentRequest request)
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
    public async Task<ActionResult> GetCommentReplies(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var replies = await _context.Comments
            .Where(c => c.ParentCommentId == id)
            .Include(c => c.User)
            .Include(c => c.Ratings)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var result = replies.Select(r => new
        {
            id = r.Id,
            text = r.Text,
            userId = r.UserId,
            firstName = r.User.FirstName,
            lastName = r.User.LastName,
            avatarUrl = r.User.AvatarUrl,
            createdAt = r.CreatedAt,
            updatedAt = r.UpdatedAt,
            likesCount = r.Ratings.Count(rt => rt.IsLike),
            dislikesCount = r.Ratings.Count(rt => !rt.IsLike),
            userRating = userId != null ? r.Ratings.FirstOrDefault(rt => rt.UserId == userId)?.IsLike : null
        });

        return Ok(result);
    }
}

public class CreateCommentRequest
{
    public int HotelId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
}

public class UpdateCommentRequest
{
    public string Text { get; set; } = string.Empty;
}

public class RateCommentRequest
{
    public bool IsLike { get; set; }
}
