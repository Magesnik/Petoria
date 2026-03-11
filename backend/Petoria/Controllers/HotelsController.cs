using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Petoria.Core.DTOs.Hotel;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMemoryCache _cache;

    public HotelsController(ApplicationDbContext context, HttpClient httpClient, UserManager<ApplicationUser> userManager, IMemoryCache cache)
    {
        _context = context;
        _httpClient = httpClient;
        _userManager = userManager;
        _cache = cache;
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
        [FromQuery] decimal? minRating,
        [FromQuery] string? starRating,
        [FromQuery] DateTime? checkInDate,
        [FromQuery] int? nights,
        [FromQuery] int? guests)
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

        // Filter by star rating
        if (!string.IsNullOrWhiteSpace(starRating))
        {
            var stars = starRating.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var v) ? v : (int?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();
            if (stars.Any())
            {
                query = query.Where(h => stars.Contains(h.StarRating));
            }
        }

        // Filter by capacity and availability
        if (guests.HasValue || (checkInDate.HasValue && nights.HasValue && nights.Value > 0))
        {
            query = query.Where(h => _context.RoomTypes.Any(rt => 
                rt.HotelId == h.Id &&
                (!guests.HasValue || rt.Capacity >= guests.Value) &&
                (!checkInDate.HasValue || !nights.HasValue || nights.Value <= 0 ||
                    !rt.Availabilities.Any(a => 
                        a.Date >= checkInDate.Value.Date && 
                        a.Date < checkInDate.Value.Date.AddDays(nights.Value) && 
                        (a.AvailableCount <= 0 || a.IsBlocked)))
            ));
        }

        // Only show available hotels that are not suspended by SuperAdmin
        query = query.Where(h => h.IsAvailable && !h.IsSuspendedBySuperAdmin);

        var today = DateTime.UtcNow;
        var thirtyDaysAgo = today.AddDays(-30);
        
        var hotelsWithDiscounts = await query
            .Select(h => new
            {
                Hotel = h,
                // Get the cheapest room for this hotel
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
                    .FirstOrDefault(),
                ReviewCount = _context.HotelReviews.Count(r => r.HotelId == h.Id),
                RecentReviewCount = _context.HotelReviews.Count(r => r.HotelId == h.Id && r.CreatedAt >= thirtyDaysAgo),
                AvailableRoomsTotal = _context.RoomAvailabilities
                    .Where(a => a.Date >= today.Date && a.Date <= today.Date.AddDays(30) && !a.IsBlocked)
                    .Join(_context.RoomTypes.Where(rt => rt.HotelId == h.Id),
                        a => a.RoomTypeId, rt => rt.Id, (a, rt) => a.AvailableCount)
                    .Sum()
            })
            .OrderByDescending(h => h.Hotel.Rating)
            .ToListAsync();

        // Transform results and apply price filters in memory (since we need calculated prices)
        var result = hotelsWithDiscounts
            .Select(h =>
            {
                var hotel = h.Hotel;
                decimal originalPrice = h.MinRoomPrice?.PricePerNight ?? 0;
                decimal displayPrice = originalPrice;
                int? discountPercentage = null;
                bool hasDiscount = false;

                if (h.MinRoomPrice != null && h.MinRoomPrice.MaxDiscount.HasValue)
                {
                    hasDiscount = true;
                    discountPercentage = h.MinRoomPrice.MaxDiscount.Value;
                    displayPrice = h.MinRoomPrice.PricePerNight * (1 - discountPercentage.Value / 100m);
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
                    OriginalPrice = originalPrice, // Calculated from RoomTypes
                    DisplayPrice = displayPrice,   // Calculated from RoomTypes
                    HasDiscount = hasDiscount,
                    DiscountPercentage = discountPercentage,
                    Rating = hotel.Rating,
                    StarRating = hotel.StarRating,
                    ImageUrl = hotel.ImageUrl,
                    Images = hotel.Images,
                    Amenities = hotel.Amenities,
                    RoomTypes = hotel.RoomTypes,
                    CancellationPolicies = hotel.CancellationPolicies,
                    IsAvailable = hotel.IsAvailable,
                    IsSuspendedBySuperAdmin = hotel.IsSuspendedBySuperAdmin,
                    CreatedById = hotel.CreatedById,
                    CreatedAt = hotel.CreatedAt,
                    UpdatedAt = hotel.UpdatedAt,
                    ReviewCount = h.ReviewCount,
                    RecentReviewCount = h.RecentReviewCount,
                    AvailableRoomsTotal = h.AvailableRoomsTotal
                };
            });

        // Only show hotels that have a valid price (meaning they have rooms)
        result = result.Where(h => h.DisplayPrice > 0);

        // Apply price filters after calculation
        if (minPrice.HasValue)
        {
            result = result.Where(h => h.DisplayPrice >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            result = result.Where(h => h.DisplayPrice <= maxPrice.Value);
        }

        return Ok(result.ToList());
    }

    // GET: api/hotels/popular-destinations
    [HttpGet("popular-destinations")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PopularDestinationDto>>> GetPopularDestinations()
    {
        var destinations = new List<PopularDestinationDto>();
        
        // 1. Group reservations by Hotel City & Country to find top 3
        var popularCities = await _context.Reservations
            .Include(r => r.Hotel)
            .Where(r => r.Hotel != null && r.Status == "Confirmed" && !r.Hotel.IsSuspendedBySuperAdmin && r.Hotel.IsAvailable)
            .GroupBy(r => new { r.Hotel!.City, r.Hotel.Country })
            .Select(g => new 
            {
                g.Key.City,
                g.Key.Country,
                ReservationCount = g.Count()
            })
            .OrderByDescending(x => x.ReservationCount)
            .Take(3)
            .ToListAsync();

        if (popularCities.Any())
        {
            foreach (var dest in popularCities)
            {
                var repHotel = await _context.Hotels
                    .Where(h => h.City == dest.City && h.Country == dest.Country && !string.IsNullOrEmpty(h.ImageUrl) && !h.IsSuspendedBySuperAdmin && h.IsAvailable)
                    .OrderByDescending(h => h.Rating)
                    .FirstOrDefaultAsync();

                if (repHotel == null) continue;

                var minPrice = await _context.RoomTypes
                    .Include(rt => rt.Hotel)
                    .Where(rt => rt.Hotel!.City == dest.City && rt.Hotel.Country == dest.Country && !rt.Hotel.IsSuspendedBySuperAdmin && rt.Hotel.IsAvailable)
                    .MinAsync(rt => (decimal?)rt.PricePerNight) ?? 0;

                destinations.Add(new PopularDestinationDto
                {
                    HotelId = repHotel.Id,
                    City = dest.City,
                    Country = dest.Country,
                    ImageUrl = repHotel.ImageUrl,
                    StartingPrice = minPrice,
                    ReservationCount = dest.ReservationCount
                });
            }
        }
        else 
        {
            // Fallback: Pick top 3 highest-rated cities if no reservations exist
            var topRatedCities = await _context.Hotels
                .Where(h => !h.IsSuspendedBySuperAdmin && h.IsAvailable && !string.IsNullOrEmpty(h.ImageUrl))
                .GroupBy(h => new { h.City, h.Country })
                .Select(g => new
                {
                    g.Key.City,
                    g.Key.Country,
                    MaxRating = g.Max(h => h.Rating)
                })
                .OrderByDescending(x => x.MaxRating)
                .Take(3)
                .ToListAsync();

            foreach (var c in topRatedCities)
            {
                var repHotel = await _context.Hotels
                    .Where(h => h.City == c.City && h.Country == c.Country && !string.IsNullOrEmpty(h.ImageUrl) && !h.IsSuspendedBySuperAdmin && h.IsAvailable)
                    .OrderByDescending(h => h.Rating)
                    .FirstOrDefaultAsync();

                if (repHotel == null) continue; 

                var minPrice = await _context.RoomTypes
                    .Include(rt => rt.Hotel)
                    .Where(rt => rt.Hotel!.City == c.City && rt.Hotel.Country == c.Country && !rt.Hotel.IsSuspendedBySuperAdmin && rt.Hotel.IsAvailable)
                    .MinAsync(rt => (decimal?)rt.PricePerNight) ?? 0;

                destinations.Add(new PopularDestinationDto
                {
                    HotelId = repHotel.Id,
                    City = c.City,
                    Country = c.Country,
                    ImageUrl = repHotel.ImageUrl,
                    StartingPrice = minPrice,
                    ReservationCount = 0
                });
            }
        }

        return Ok(destinations);
    }

    // GET: api/hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<HotelResponseDto>> GetHotel(int id)
    {
        var hotel = await _context.Hotels
            .FirstOrDefaultAsync(h => h.Id == id);
            
        if (hotel == null)
        {
            return NotFound();
        }

        // Block access to suspended hotels for non-SuperAdmin users
        if (hotel.IsSuspendedBySuperAdmin && !User.IsInRole("SuperAdmin"))
        {
            return StatusCode(403, new { message = "Този хотел е временно спрян от администрацията." });
        }

        // Calculate price from room types
        var minRoomPrice = await _context.RoomTypes
            .Where(rt => rt.HotelId == id)
            .OrderBy(rt => rt.PricePerNight)
            .Select(rt => rt.PricePerNight)
            .FirstOrDefaultAsync(); // Returns 0 if no rooms

        // Check if user is moderator
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isModerator = false;
        
        if (!string.IsNullOrEmpty(userId))
        {
            isModerator = await _context.HotelModerators
                .AnyAsync(hm => hm.HotelId == id && hm.UserId == userId);
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
            OriginalPrice = minRoomPrice,
            DisplayPrice = minRoomPrice,
            HasDiscount = false,
            DiscountPercentage = null,
            Rating = hotel.Rating,
            StarRating = hotel.StarRating,
            ImageUrl = hotel.ImageUrl,
            Images = hotel.Images,
            Amenities = hotel.Amenities,
            RoomTypes = hotel.RoomTypes,
            CancellationPolicies = hotel.CancellationPolicies,
            IsAvailable = hotel.IsAvailable,
            CreatedById = hotel.CreatedById,
            CreatedAt = hotel.CreatedAt,
            UpdatedAt = hotel.UpdatedAt,
            IsModerator = isModerator
        });
    }

    // GET: api/hotels/my - Get hotels created by current user
    [HttpGet("my")]
    [Authorize(Roles = Petoria.Constants.Roles.Admin + "," + Petoria.Constants.Roles.SuperAdmin)]
    public async Task<ActionResult<IEnumerable<HotelResponseDto>>> GetMyHotels()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // We need to fetch hotels and then lookup their prices, or do a join
        // For simplicity with EF Core, let's fetch hotel items and a subquery for price
        var hotelsData = await _context.Hotels
            .Where(h => h.CreatedById == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new 
            {
                Hotel = h,
                MinPrice = _context.RoomTypes
                    .Where(rt => rt.HotelId == h.Id)
                    .OrderBy(rt => rt.PricePerNight)
                    .Select(rt => (decimal?)rt.PricePerNight)
                    .FirstOrDefault() ?? 0
            })
            .ToListAsync();

        var hotels = hotelsData.Select(h => new HotelResponseDto
            {
                Id = h.Hotel.Id,
                Name = h.Hotel.Name,
                Description = h.Hotel.Description,
                Location = h.Hotel.Location,
                City = h.Hotel.City,
                Country = h.Hotel.Country,
                Latitude = h.Hotel.Latitude,
                Longitude = h.Hotel.Longitude,
                OriginalPrice = h.MinPrice,
                DisplayPrice = h.MinPrice,
                HasDiscount = false,
                DiscountPercentage = null,
                Rating = h.Hotel.Rating,
                StarRating = h.Hotel.StarRating,
                ImageUrl = h.Hotel.ImageUrl,
                Images = h.Hotel.Images,
                Amenities = h.Hotel.Amenities,
                RoomTypes = h.Hotel.RoomTypes,
                CancellationPolicies = h.Hotel.CancellationPolicies,
                IsAvailable = h.Hotel.IsAvailable,
                CreatedById = h.Hotel.CreatedById,
                CreatedAt = h.Hotel.CreatedAt,
                UpdatedAt = h.Hotel.UpdatedAt
            })
            .ToList();

        return Ok(hotels);
    }

    // GET: api/hotels/moderated
    [HttpGet("moderated")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<HotelResponseDto>>> GetModeratedHotels()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        var hotels = await _context.HotelModerators
            .Where(hm => hm.UserId == userId)
            .Include(hm => hm.Hotel)
            .Select(hm => hm.Hotel)
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
                StarRating = h.StarRating,
                ImageUrl = h.ImageUrl,
                IsModerator = true
            })
            .ToListAsync();

        return Ok(hotels);
    }

    // GET: api/hotels/cities
    [HttpGet("cities")]
    public async Task<ActionResult<IEnumerable<string>>> GetCities()
    {
        var cities = await _context.Hotels
            .Where(h => !string.IsNullOrEmpty(h.City) && 
                        h.IsAvailable && 
                        !h.IsSuspendedBySuperAdmin &&
                        _context.RoomTypes.Any(rt => rt.HotelId == h.Id))
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
            .Where(h => !string.IsNullOrEmpty(h.Country) && 
                        h.IsAvailable && 
                        !h.IsSuspendedBySuperAdmin &&
                        _context.RoomTypes.Any(rt => rt.HotelId == h.Id))
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
            .Where(h => !string.IsNullOrEmpty(h.Amenities) && 
                        h.IsAvailable && 
                        !h.IsSuspendedBySuperAdmin &&
                        _context.RoomTypes.Any(rt => rt.HotelId == h.Id))
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
        // Calculate range based on RoomTypes for visible hotels only
        var query = _context.RoomTypes
            .Include(rt => rt.Hotel)
            .Where(rt => rt.Hotel.IsAvailable && !rt.Hotel.IsSuspendedBySuperAdmin);

        if (!await query.AnyAsync())
        {
             return Ok(new { minPrice = 0, maxPrice = 1000 }); // Default fallback
        }

        var minPrice = await query.MinAsync(rt => rt.PricePerNight);
        var maxPrice = await query.MaxAsync(rt => rt.PricePerNight);

        return Ok(new 
        { 
            minPrice = Math.Floor(minPrice), 
            maxPrice = Math.Ceiling(maxPrice) 
        });
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
        [FromQuery] decimal? minRating,
        [FromQuery] string? starRating,
        [FromQuery] DateTime? checkInDate,
        [FromQuery] int? nights,
        [FromQuery] int? guests)
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

        // Filter by star rating
        if (!string.IsNullOrWhiteSpace(starRating))
        {
            var stars = starRating.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var v) ? v : (int?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();
            if (stars.Any())
            {
                query = query.Where(h => stars.Contains(h.StarRating));
            }
        }

        // Filter by capacity and availability
        if (guests.HasValue || (checkInDate.HasValue && nights.HasValue && nights.Value > 0))
        {
            query = query.Where(h => _context.RoomTypes.Any(rt => 
                rt.HotelId == h.Id &&
                (!guests.HasValue || rt.Capacity >= guests.Value) &&
                (!checkInDate.HasValue || !nights.HasValue || nights.Value <= 0 ||
                    !rt.Availabilities.Any(a => 
                        a.Date >= checkInDate.Value.Date && 
                        a.Date < checkInDate.Value.Date.AddDays(nights.Value) && 
                        (a.AvailableCount <= 0 || a.IsBlocked)))
            ));
        }

        // Only show available hotels that are not suspended by SuperAdmin
        query = query.Where(h => h.IsAvailable && !h.IsSuspendedBySuperAdmin);

        // Fetch hotels with their min room price
        var hotelsData = await query
            .Select(h => new 
            {
                Hotel = h,
                MinPrice = _context.RoomTypes
                    .Where(rt => rt.HotelId == h.Id)
                    .OrderBy(rt => rt.PricePerNight)
                    .Select(rt => (decimal?)rt.PricePerNight)
                    .FirstOrDefault() ?? 0
            })
            .ToListAsync();

        var hotels = hotelsData.Select(h => new HotelMapResponseDto
        {
            Id = h.Hotel.Id,
            Name = h.Hotel.Name,
            City = h.Hotel.City,
            Country = h.Hotel.Country,
            Latitude = h.Hotel.Latitude,
            Longitude = h.Hotel.Longitude,
            PricePerNight = h.MinPrice,
            Rating = h.Hotel.Rating,
            StarRating = h.Hotel.StarRating,
            ImageUrl = h.Hotel.ImageUrl
        });

        // Apply price filters in memory
        if (minPrice.HasValue)
        {
            hotels = hotels.Where(h => h.PricePerNight >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            hotels = hotels.Where(h => h.PricePerNight <= maxPrice.Value);
        }

        // Only show hotels that have a valid price
        hotels = hotels.Where(h => h.PricePerNight > 0);

        return Ok(hotels.ToList());
    }

    // GET: api/hotels/geocode
    [HttpGet("geocode")]
    public async Task<IActionResult> GetAddress([FromQuery] double lat, [FromQuery] double lon)
    {
        // Round to 4 decimal places (~11m precision) to maximise cache hits
        var cacheKey = $"geocode_{lat:F4}_{lon:F4}";

        if (_cache.TryGetValue(cacheKey, out string? cached))
            return Content(cached!, "application/json");

        try
        {
            var latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latStr}&lon={lonStr}&zoom=18&addressdetails=1");
            request.Headers.Add("User-Agent", "PetoriaApp/1.0 (contact@petoria.com)");

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                // Cache a negative result briefly to avoid retry storms
                _cache.Set(cacheKey, "{}", TimeSpan.FromSeconds(30));
                return StatusCode(429, "Error from geocoding service");
            }

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error from geocoding service");

            var content = await response.Content.ReadAsStringAsync();
            _cache.Set(cacheKey, content, TimeSpan.FromHours(24));
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error during geocoding" });
        }
    }

    // POST: api/hotels
    [HttpPost]
    [Authorize(Roles = Petoria.Constants.Roles.Admin + "," + Petoria.Constants.Roles.SuperAdmin)]
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
                // PricePerNight removed
                StarRating = dto.StarRating,
                ImageUrl = dto.ImageUrl,
                Images = dto.Images,
                Amenities = dto.Amenities,
                RoomTypes = dto.RoomTypes,
                CancellationPolicies = dto.CancellationPolicies,
                // Force inactive by default until rooms/prices are added
                IsAvailable = false,
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
                OriginalPrice = 0, // No rooms yet
                DisplayPrice = 0,
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
            return StatusCode(500, new { message = "An unexpected error occurred while creating the hotel." });
        }
    }
    // PUT: api/hotels/5
    [HttpPut("{id}")]
    [Authorize(Roles = Petoria.Constants.Roles.Admin + "," + Petoria.Constants.Roles.SuperAdmin + "," + Petoria.Constants.Roles.HotelModerator)]
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
        var isModerator = await _context.HotelModerators.AnyAsync(hm => hm.HotelId == id && hm.UserId == userId);
        
        if (!isSuperAdmin && existingHotel.CreatedById != userId && !isModerator)
        {
            return Forbid("You can only edit hotels that you created or moderate");
        }

        // Validate activation rule: Cannot activate if no rooms
        if (dto.IsAvailable && !existingHotel.IsAvailable)
        {
            // Check if hotel has any room types
            var hasRooms = await _context.RoomTypes.AnyAsync(rt => rt.HotelId == id);
            if (!hasRooms)
            {
                return BadRequest(new { message = "Cannot activate hotel without any rooms or prices configured." });
            }
        }

        // Map DTO → Entity (update)
        existingHotel.Name = dto.Name;
        existingHotel.Description = dto.Description;
        existingHotel.Location = dto.Location;
        existingHotel.City = dto.City;
        existingHotel.Country = dto.Country;
        existingHotel.Latitude = dto.Latitude;
        existingHotel.Longitude = dto.Longitude;
        existingHotel.ImageUrl = dto.ImageUrl;
        existingHotel.Images = dto.Images;
        existingHotel.Amenities = dto.Amenities;
        existingHotel.CancellationPolicies = dto.CancellationPolicies;
        // RoomTypes are managed via separate controller usually, but if passed here, ignore or handle carefully. 
        // We generally don't update connection via UpdateHotelDto for RoomTypes as it's complex.
        // exisingHotel.RoomTypes = dto.RoomTypes; // Avoid updating detailed navigation property here if not needed
        
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
    [Authorize(Roles = Petoria.Constants.Roles.Admin + "," + Petoria.Constants.Roles.SuperAdmin)]
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

    // POST: api/hotels/5/moderators
    [HttpPost("{id}/moderators")]
    [Authorize]
    public async Task<IActionResult> AddModerator(int id, [FromBody] AddModeratorDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var hotel = await _context.Hotels.FindAsync(id);

        if (hotel == null) return NotFound("Hotel not found");

        // Only owner or super admin can add moderators
        if (hotel.CreatedById != userId && !User.IsInRole("SuperAdmin"))
        {
            return Forbid();
        }

        var moderatorUser = await _userManager.FindByEmailAsync(dto.Email);
        if (moderatorUser == null)
        {
            return BadRequest("User with this email not found");
        }

        if (moderatorUser.Id == hotel.CreatedById)
        {
             return BadRequest("Owner cannot be a moderator");
        }

        var existing = await _context.HotelModerators
            .FirstOrDefaultAsync(hm => hm.HotelId == id && hm.UserId == moderatorUser.Id);

        if (existing != null)
        {
            return BadRequest("User is already a moderator for this hotel");
        }

        var moderator = new HotelModerator
        {
            HotelId = id,
            UserId = moderatorUser.Id
        };

        _context.HotelModerators.Add(moderator);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Moderator added successfully" });
    }

    // GET: api/hotels/5/moderators
    [HttpGet("{id}/moderators")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ModeratorResponseDto>>> GetModerators(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var hotel = await _context.Hotels.FindAsync(id);

        if (hotel == null) return NotFound("Hotel not found");

        // Only owner or super admin can see moderators
        if (hotel.CreatedById != userId && !User.IsInRole("SuperAdmin"))
        {
            return Forbid();
        }

        var moderators = await _context.HotelModerators
            .Where(hm => hm.HotelId == id)
            .Include(hm => hm.User)
            .Select(hm => new ModeratorResponseDto
            {
                UserId = hm.UserId,
                Email = hm.User.Email,
                FirstName = hm.User.FirstName,
                LastName = hm.User.LastName,
                AddedAt = hm.CreatedAt
            })
            .ToListAsync();

        return Ok(moderators);
    }

    // DELETE: api/hotels/5/moderators/{userId}
    [HttpDelete("{id}/moderators/{moderatorId}")]
    [Authorize]
    public async Task<IActionResult> RemoveModerator(int id, string moderatorId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var hotel = await _context.Hotels.FindAsync(id);

        if (hotel == null) return NotFound("Hotel not found");

        // Only owner or super admin can remove moderators
        if (hotel.CreatedById != userId && !User.IsInRole("SuperAdmin"))
        {
            return Forbid();
        }

        var moderator = await _context.HotelModerators
            .FirstOrDefaultAsync(hm => hm.HotelId == id && hm.UserId == moderatorId);

        if (moderator == null)
        {
            return NotFound("Moderator not found");
        }

        _context.HotelModerators.Remove(moderator);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Moderator removed successfully" });
    }
}
