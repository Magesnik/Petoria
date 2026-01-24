using Microsoft.AspNetCore.Authorization;
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
        [FromQuery] string? country,
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

        // Filter by country
        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(h => h.Country == country);
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

    // GET: api/hotels/my - Get hotels created by current user
    [HttpGet("my")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Hotel>>> GetMyHotels()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var hotels = await _context.Hotels
            .Where(h => h.CreatedById == userId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        return Ok(hotels);
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

    // GET: api/hotels/countries
    [HttpGet("countries")]
    public async Task<ActionResult<IEnumerable<string>>> GetCountries()
    {
        var countries = await _context.Hotels
            .Where(h => !string.IsNullOrEmpty(h.Country))
            .Select(h => h.Country)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(countries);
    }

    // GET: api/hotels/amenities
    [HttpGet("amenities")]
    public async Task<ActionResult<IEnumerable<string>>> GetAllAmenities()
    {
        var hotels = await _context.Hotels
            .Where(h => !string.IsNullOrEmpty(h.Amenities))
            .Select(h => h.Amenities)
            .ToListAsync();

        // Parse JSON arrays and get unique amenities
        var allAmenities = new HashSet<string>();
        foreach (var amenitiesJson in hotels)
        {
            try
            {
                var amenitiesList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(amenitiesJson);
                if (amenitiesList != null)
                {
                    foreach (var amenity in amenitiesList)
                    {
                        if (!string.IsNullOrWhiteSpace(amenity))
                        {
                            allAmenities.Add(amenity.Trim());
                        }
                    }
                }
            }
            catch
            {
                // If JSON parsing fails, try to extract amenities as simple string
                if (!string.IsNullOrWhiteSpace(amenitiesJson))
                {
                    allAmenities.Add(amenitiesJson.Trim());
                }
            }
        }

        return Ok(allAmenities.OrderBy(a => a).ToList());
    }

    // GET: api/hotels/price-range
    [HttpGet("price-range")]
    public async Task<ActionResult<object>> GetPriceRange()
    {
        var minPrice = await _context.Hotels.MinAsync(h => h.PricePerNight);
        var maxPrice = await _context.Hotels.MaxAsync(h => h.PricePerNight);

        return Ok(new { minPrice, maxPrice });
    }

    // GET: api/hotels/map
    [HttpGet("map")]
    public async Task<ActionResult<IEnumerable<object>>> GetHotelsForMap(
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? city,
        [FromQuery] string? country,
        [FromQuery] string? amenities,
        [FromQuery] decimal? minRating)
    {
        var query = _context.Hotels.AsQueryable();

        // Apply same filters as GetHotels
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(h => h.Name.Contains(search) || 
                                    h.Location.Contains(search) || 
                                    h.City.Contains(search) ||
                                    h.Country.Contains(search));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(h => h.PricePerNight >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(h => h.PricePerNight <= maxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(h => h.City == city);
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(h => h.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(amenities))
        {
            var amenityList = amenities.Split(',');
            foreach (var amenity in amenityList)
            {
                query = query.Where(h => h.Amenities.Contains(amenity.Trim()));
            }
        }

        if (minRating.HasValue)
        {
            query = query.Where(h => h.Rating >= minRating.Value);
        }

        // Only show available hotels
        query = query.Where(h => h.IsAvailable);

        // Return only necessary data for map markers (excluding heavy fields)
        var hotels = await query
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.City,
                h.Country,
                h.Latitude,
                h.Longitude,
                h.PricePerNight,
                h.Rating,
                h.ImageUrl
            })
            .ToListAsync();

        return Ok(hotels);
    }

    // POST: api/hotels
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Hotel>> CreateHotel(Hotel hotel)
    {
        try
        {
            // Get the current user's ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            // Log for debugging
            Console.WriteLine($"User ID from claims: {userId}");
            Console.WriteLine($"User claims count: {User.Claims.Count()}");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token", claims = User.Claims.Select(c => new { c.Type, c.Value }) });
            }

            hotel.CreatedById = userId;
            hotel.CreatedAt = DateTime.UtcNow;
            hotel.UpdatedAt = DateTime.UtcNow;

            Console.WriteLine($"About to save hotel with CreatedById: {hotel.CreatedById}");

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating hotel: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { 
                message = ex.Message, 
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace 
            });
        }
    }

    // PUT: api/hotels/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateHotel(int id, Hotel hotel)
    {
        if (id != hotel.Id)
        {
            return BadRequest();
        }

        var existingHotel = await _context.Hotels.FindAsync(id);
        if (existingHotel == null)
        {
            return NotFound();
        }

        // Check authorization: SuperAdmin can edit any, Admin can only edit own hotels
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        
        if (!isSuperAdmin && existingHotel.CreatedById != userId)
        {
            return Forbid("You can only edit hotels that you created");
        }

        // Update the existing hotel properties
        existingHotel.Name = hotel.Name;
        existingHotel.Description = hotel.Description;
        existingHotel.Location = hotel.Location;
        existingHotel.City = hotel.City;
        existingHotel.Country = hotel.Country;
        existingHotel.Latitude = hotel.Latitude;
        existingHotel.Longitude = hotel.Longitude;
        existingHotel.PricePerNight = hotel.PricePerNight;
        existingHotel.Rating = hotel.Rating;
        existingHotel.ImageUrl = hotel.ImageUrl;
        existingHotel.Images = hotel.Images;
        existingHotel.Amenities = hotel.Amenities;
        existingHotel.RoomTypes = hotel.RoomTypes;
        existingHotel.IsAvailable = hotel.IsAvailable;
        existingHotel.UpdatedAt = DateTime.UtcNow;

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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }

        // Check authorization: SuperAdmin can delete any, Admin can only delete own hotels
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        
        if (!isSuperAdmin && hotel.CreatedById != userId)
        {
            return Forbid("You can only delete hotels that you created");
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
