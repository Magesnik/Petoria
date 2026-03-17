using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Favorite;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;
using System.Security.Claims;

namespace Petoria.Controllers;

/// <summary>
/// Контролер за любими хотели: списък, добавяне, премахване, toggle, проверка
/// </summary>
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

    /// <summary>
    /// Връща списък с любимите хотели на текущия потребител
    /// </summary>
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
                // Вземане на минималната цена от типовете стаи
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

    /// <summary>
    /// Връща списък с идентификаторите на любимите хотели на потребителя
    /// </summary>
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

    /// <summary>
    /// Добавя хотел към любимите на потребителя
    /// </summary>
    [HttpPost("{hotelId}")]
    public async Task<ActionResult> AddFavorite(int hotelId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Проверка дали хотелът съществува
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound("Hotel not found");
        }

        // Проверка дали вече е добавен в любими
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

    /// <summary>
    /// Премахва хотел от любимите на потребителя
    /// </summary>
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

    /// <summary>
    /// Превключва статуса на хотел в любими (добавя/премахва)
    /// </summary>
    [HttpPost("toggle/{hotelId}")]
    public async Task<ActionResult> ToggleFavorite(int hotelId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Проверка дали хотелът съществува
        var hotel = await _context.Hotels.FindAsync(hotelId);
        if (hotel == null)
        {
            return NotFound("Hotel not found");
        }

        var existingFavorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.HotelId == hotelId);

        if (existingFavorite != null)
        {
            // Премахване от любими
            _context.Favorites.Remove(existingFavorite);
            await _context.SaveChangesAsync();
            return Ok(new { isFavorite = false, hotelId });
        }
        else
        {
            // Добавяне в любими
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

    /// <summary>
    /// Проверява дали хотел е в любимите на потребителя
    /// </summary>
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
