using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HotelsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/hotels
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Hotel>>> GetHotels(
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? city,
        [FromQuery] string? amenities,
        [FromQuery] decimal? minRating)
    {
        var query = _context.Hotels.AsQueryable();

        // Search by name or location
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(h => h.Name.Contains(search) || 
                                    h.Location.Contains(search) || 
                                    h.City.Contains(search) ||
                                    h.Country.Contains(search));
        }

        // Filter by price range
        if (minPrice.HasValue)
        {
            query = query.Where(h => h.PricePerNight >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(h => h.PricePerNight <= maxPrice.Value);
        }

        // Filter by city
        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(h => h.City == city);
        }

        // Filter by amenities
        if (!string.IsNullOrWhiteSpace(amenities))
        {
            var amenityList = amenities.Split(',');
            foreach (var amenity in amenityList)
            {
                query = query.Where(h => h.Amenities.Contains(amenity.Trim()));
            }
        }

        // Filter by rating
        if (minRating.HasValue)
        {
            query = query.Where(h => h.Rating >= minRating.Value);
        }

        // Only show available hotels
        query = query.Where(h => h.IsAvailable);

        var hotels = await query.OrderByDescending(h => h.Rating).ToListAsync();
        return Ok(hotels);
    }

    // GET: api/hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Hotel>> GetHotel(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);

        if (hotel == null)
        {
            return NotFound();
        }

        return Ok(hotel);
    }

    // GET: api/hotels/cities
    [HttpGet("cities")]
    public async Task<ActionResult<IEnumerable<string>>> GetCities()
    {
        var cities = await _context.Hotels
            .Where(h => !string.IsNullOrEmpty(h.City))
            .Select(h => h.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(cities);
    }

    // POST: api/hotels
    [HttpPost]
    public async Task<ActionResult<Hotel>> CreateHotel(Hotel hotel)
    {
        hotel.CreatedAt = DateTime.UtcNow;
        hotel.UpdatedAt = DateTime.UtcNow;

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
    }

    // PUT: api/hotels/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHotel(int id, Hotel hotel)
    {
        if (id != hotel.Id)
        {
            return BadRequest();
        }

        hotel.UpdatedAt = DateTime.UtcNow;
        _context.Entry(hotel).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!HotelExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/hotels/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }

        _context.Hotels.Remove(hotel);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool HotelExists(int id)
    {
        return _context.Hotels.Any(e => e.Id == id);
    }
}
