using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.DTOs.Hotel;
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
    public async Task<ActionResult<IEnumerable<HotelResponseDto>>> GetHotels(
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

        var today = DateTime.UtcNow;
        
        // Get hotels with room types and active discounts
        var hotelsWithDiscounts = await query
            .Select(h => new
            {
                Hotel = h,
                MinRoomPrice = _context.RoomTypes
                    .Where(rt => rt.HotelId == h.Id)
                    .Select(rt => new {
                        rt.PricePerNight,
                        MaxDiscount = rt.Discounts
                            .Where(d => d.StartDate <= today && d.EndDate >= today)
                            .OrderByDescending(d => d.DiscountPercentage)
                            .Select(d => (int?)d.DiscountPercentage)
                            .FirstOrDefault()
                    })
                    .OrderBy(rt => rt.PricePerNight)
                    .FirstOrDefault()
            })
            .OrderByDescending(h => h.Hotel.Rating)
            .ToListAsync();

        // Transform to HotelResponseDto
        var result = hotelsWithDiscounts.Select(h =>
        {
            var hotel = h.Hotel;
            decimal displayPrice = hotel.PricePerNight;
            int? discountPercentage = null;
            bool hasDiscount = false;

            if (h.MinRoomPrice != null && h.MinRoomPrice.MaxDiscount.HasValue)
            {
                hasDiscount = true;
                discountPercentage = h.MinRoomPrice.MaxDiscount.Value;
                displayPrice = h.MinRoomPrice.PricePerNight * (1 - discountPercentage.Value / 100m);
            }
            else if (h.MinRoomPrice != null)
            {
                displayPrice = h.MinRoomPrice.PricePerNight;
            }

            return new HotelResponseDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                Location = hotel.Location,
                City = hotel.City,
                Country = hotel.Country,
                Latitude = hotel.Latitude,
                Longitude = hotel.Longitude,
                OriginalPrice = hotel.PricePerNight,
                DisplayPrice = displayPrice,
                HasDiscount = hasDiscount,
                DiscountPercentage = discountPercentage,
                Rating = hotel.Rating,
                StarRating = hotel.StarRating,
                ImageUrl = hotel.ImageUrl,
                Images = hotel.Images,
                Amenities = hotel.Amenities,
                RoomTypes = hotel.RoomTypes,
                IsAvailable = hotel.IsAvailable,
                CreatedById = hotel.CreatedById,
                CreatedAt = hotel.CreatedAt,
                UpdatedAt = hotel.UpdatedAt
            };
        }).ToList();

        return Ok(result);
    }

    // GET: api/hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<HotelResponseDto>> GetHotel(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);

        if (hotel == null)
        {
            return NotFound();
        }

        return Ok(new HotelResponseDto
        {
            Id = hotel.Id,
            Name = hotel.Name,
            Description = hotel.Description,
            Location = hotel.Location,
            City = hotel.City,
            Country = hotel.Country,
            Latitude = hotel.Latitude,
            Longitude = hotel.Longitude,
            OriginalPrice = hotel.PricePerNight,
            DisplayPrice = hotel.PricePerNight,
            HasDiscount = false,
            DiscountPercentage = null,
            Rating = hotel.Rating,
            StarRating = hotel.StarRating,
            ImageUrl = hotel.ImageUrl,
            Images = hotel.Images,
            Amenities = hotel.Amenities,
            RoomTypes = hotel.RoomTypes,
            IsAvailable = hotel.IsAvailable,
            CreatedById = hotel.CreatedById,
            CreatedAt = hotel.CreatedAt,
            UpdatedAt = hotel.UpdatedAt
        });
    }

    // GET: api/hotels/my - Get hotels created by current user
    [HttpGet("my")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<HotelResponseDto>>> GetMyHotels()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var hotels = await _context.Hotels
            .Where(h => h.CreatedById == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new HotelResponseDto
            {
                Id = h.Id,
                Name = h.Name,
                Description = h.Description,
                Location = h.Location,
                City = h.City,
                Country = h.Country,
                Latitude = h.Latitude,
                Longitude = h.Longitude,
                OriginalPrice = h.PricePerNight,
                DisplayPrice = h.PricePerNight,
                HasDiscount = false,
                DiscountPercentage = null,
                Rating = h.Rating,
                StarRating = h.StarRating,
                ImageUrl = h.ImageUrl,
                Images = h.Images,
                Amenities = h.Amenities,
                RoomTypes = h.RoomTypes,
                IsAvailable = h.IsAvailable,
                CreatedById = h.CreatedById,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            })
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
    public async Task<ActionResult<IEnumerable<HotelMapResponseDto>>> GetHotelsForMap(
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

        // Return only necessary data for map markers
        var hotels = await query
            .Select(h => new HotelMapResponseDto
            {
                Id = h.Id,
                Name = h.Name,
                City = h.City,
                Country = h.Country,
                Latitude = h.Latitude,
                Longitude = h.Longitude,
                PricePerNight = h.PricePerNight,
                Rating = h.Rating,
                StarRating = h.StarRating,
                ImageUrl = h.ImageUrl
            })
            .ToListAsync();

        return Ok(hotels);
    }

    // POST: api/hotels
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<HotelResponseDto>> CreateHotel(CreateHotelDto dto)
    {
        try
        {
            // Get the current user's ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            // Map DTO → Entity
            var hotel = new Hotel
            {
                Name = dto.Name,
                Description = dto.Description,
                Location = dto.Location,
                City = dto.City,
                Country = dto.Country,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                PricePerNight = dto.PricePerNight,
                StarRating = dto.StarRating,
                ImageUrl = dto.ImageUrl,
                Images = dto.Images,
                Amenities = dto.Amenities,
                RoomTypes = dto.RoomTypes,
                IsAvailable = dto.IsAvailable,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            // Map Entity → Response DTO
            var response = new HotelResponseDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                Location = hotel.Location,
                City = hotel.City,
                Country = hotel.Country,
                Latitude = hotel.Latitude,
                Longitude = hotel.Longitude,
                OriginalPrice = hotel.PricePerNight,
                DisplayPrice = hotel.PricePerNight,
                HasDiscount = false,
                DiscountPercentage = null,
                Rating = hotel.Rating,
                StarRating = hotel.StarRating,
                ImageUrl = hotel.ImageUrl,
                Images = hotel.Images,
                Amenities = hotel.Amenities,
                RoomTypes = hotel.RoomTypes,
                IsAvailable = hotel.IsAvailable,
                CreatedById = hotel.CreatedById,
                CreatedAt = hotel.CreatedAt,
                UpdatedAt = hotel.UpdatedAt
            };

            return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                message = ex.Message, 
                innerException = ex.InnerException?.Message
            });
        }
    }

    // PUT: api/hotels/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateHotel(int id, UpdateHotelDto dto)
    {
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

        // Map DTO → Entity (update)
        existingHotel.Name = dto.Name;
        existingHotel.Description = dto.Description;
        existingHotel.Location = dto.Location;
        existingHotel.City = dto.City;
        existingHotel.Country = dto.Country;
        existingHotel.Latitude = dto.Latitude;
        existingHotel.Longitude = dto.Longitude;
        existingHotel.PricePerNight = dto.PricePerNight;
        existingHotel.ImageUrl = dto.ImageUrl;
        existingHotel.Images = dto.Images;
        existingHotel.Amenities = dto.Amenities;
        existingHotel.RoomTypes = dto.RoomTypes;
        existingHotel.IsAvailable = dto.IsAvailable;
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
