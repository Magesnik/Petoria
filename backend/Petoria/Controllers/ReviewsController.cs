using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Review;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;
using System.Security.Claims;

namespace Petoria.Controllers;

[Route("api/hotels/{hotelId}/reviews")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReviewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/hotels/{hotelId}/reviews
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetReviews(int hotelId)
    {
        var reviews = await _context.HotelReviews
            .Where(r => r.HotelId == hotelId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                Rating = r.Rating,
                ReviewText = r.ReviewText,
                CreatedAt = r.CreatedAt,
                User = new ReviewUserDto
                {
                    Id = r.User.Id,
                    FirstName = r.User.FirstName,
                    LastName = r.User.LastName,
                    AvatarUrl = r.User.AvatarUrl
                }
            })
            .ToListAsync();

        return Ok(reviews);
    }

    // POST: api/hotels/{hotelId}/reviews
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> PostReview(int hotelId, [FromBody] CreateReviewDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound("Hotel not found");
        }

        // Check if user already reviewed this hotel
        var existingReview = await _context.HotelReviews
            .FirstOrDefaultAsync(r => r.HotelId == hotelId && r.UserId == userId);

        if (existingReview != null)
        {
            // Update existing review — Map DTO → Entity (update)
            existingReview.Rating = dto.Rating;
            existingReview.ReviewText = dto.ReviewText;
            existingReview.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create new review — Map DTO → Entity
            var review = new HotelReview
            {
                HotelId = hotelId,
                UserId = userId,
                Rating = dto.Rating,
                ReviewText = dto.ReviewText,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.HotelReviews.Add(review);
        }

        await _context.SaveChangesAsync();

        // Update Hotel Average Rating
        await UpdateHotelRating(hotelId);

        return Ok(new { message = "Review submitted successfully" });
    }

    // DELETE: api/hotels/{hotelId}/reviews/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int hotelId, int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var review = await _context.HotelReviews.FindAsync(id);

        if (review == null)
        {
            return NotFound();
        }

        if (review.HotelId != hotelId)
        {
            return BadRequest("Review does not belong to this hotel");
        }

        // Check if user is owner or admin
        if (review.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
        {
            return Forbid();
        }

        _context.HotelReviews.Remove(review);
        await _context.SaveChangesAsync();

        // Update Hotel Average Rating
        await UpdateHotelRating(hotelId);

        return NoContent();
    }

    private async Task UpdateHotelRating(int hotelId)
    {
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null) return;

        var reviews = await _context.HotelReviews.Where(r => r.HotelId == hotelId).ToListAsync();
        
        if (reviews.Any())
        {
            hotel.Rating = (decimal)reviews.Average(r => r.Rating);
        }
        else
        {
            hotel.Rating = 0;
        }

        await _context.SaveChangesAsync();
    }
}
