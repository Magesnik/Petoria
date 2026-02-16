using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Favorite;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;
using System.Security.Claims;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FavoritesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/favorites
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FavoriteResponseDto>>> GetUserFavorites()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Hotel)
            .Select(f => new FavoriteResponseDto
            {
                Id = f.Id,
                HotelId = f.HotelId,
                HotelName = f.Hotel!.Name,
                HotelCity = f.Hotel.City,
                HotelCountry = f.Hotel.Country,
                HotelImageUrl = f.Hotel.ImageUrl,
                // Get min price from room types
                HotelPricePerNight = _context.RoomTypes
                    .Where(rt => rt.HotelId == f.HotelId)
                    .OrderBy(rt => rt.PricePerNight)
                    .Select(rt => rt.PricePerNight)
                    .FirstOrDefault(),
                HotelRating = f.Hotel.Rating,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return Ok(favorites);
    }

    // GET: api/favorites/ids
    [HttpGet("ids")]
    public async Task<ActionResult<IEnumerable<int>>> GetUserFavoriteIds()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var favoriteIds = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Select(f => f.HotelId)
            .ToListAsync();

        return Ok(favoriteIds);
    }

    // POST: api/favorites/{hotelId}
    [HttpPost("{hotelId}")]
    public async Task<ActionResult> AddFavorite(int hotelId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Check if hotel exists
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound("Hotel not found");
        }

        // Check if already favorited
        var existingFavorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.HotelId == hotelId);

        if (existingFavorite != null)
        {
            return BadRequest("Hotel is already in favorites");
        }

        var favorite = new Favorite
        {
            UserId = userId,
            HotelId = hotelId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Hotel added to favorites", hotelId });
    }

    // DELETE: api/favorites/{hotelId}
    [HttpDelete("{hotelId}")]
    public async Task<ActionResult> RemoveFavorite(int hotelId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.HotelId == hotelId);

        if (favorite == null)
        {
            return NotFound("Favorite not found");
        }

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Hotel removed from favorites", hotelId });
    }

    // POST: api/favorites/toggle/{hotelId}
    [HttpPost("toggle/{hotelId}")]
    public async Task<ActionResult> ToggleFavorite(int hotelId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Check if hotel exists
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound("Hotel not found");
        }

        var existingFavorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.HotelId == hotelId);

        if (existingFavorite != null)
        {
            // Remove favorite
            _context.Favorites.Remove(existingFavorite);
            await _context.SaveChangesAsync();
            return Ok(new { isFavorite = false, hotelId });
        }
        else
        {
            // Add favorite
            var favorite = new Favorite
            {
                UserId = userId,
                HotelId = hotelId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return Ok(new { isFavorite = true, hotelId });
        }
    }

    // GET: api/favorites/check/{hotelId}
    [HttpGet("check/{hotelId}")]
    public async Task<ActionResult> CheckFavorite(int hotelId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Ok(new { isFavorite = false });
        }

        var isFavorite = await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.HotelId == hotelId);

        return Ok(new { isFavorite, hotelId });
    }
}
