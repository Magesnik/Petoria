using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Infrastructure.Data;

namespace Petoria.Controllers;

[Route("api/deals")]
[ApiController]
public class DealsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DealsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/deals/discounted
    [HttpGet("discounted")]
    public async Task<ActionResult<IEnumerable<object>>> GetDiscountedHotels()
    {
        var hotels = await _context.Hotels
            .Where(h => h.PricePerNight > 0)
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.City,
                h.Country,
                h.Location,
                h.ImageUrl,
                h.Rating,
                OriginalPrice = h.PricePerNight,
                DiscountedPrice = h.PricePerNight * 0.75m, // 25% discount
                DiscountPercentage = 25,
                h.Description,
                SaveAmount = h.PricePerNight * 0.25m
            })
            .OrderByDescending(h => h.Rating)
            .Take(12)
            .ToListAsync();

        return Ok(hotels);
    }

    // GET: api/deals/last-minute
    [HttpGet("last-minute")]
    public async Task<ActionResult<IEnumerable<object>>> GetLastMinuteDeals()
    {
        var today = DateTime.UtcNow.Date;
        var threeDaysFromNow = today.AddDays(3);

        // Get hotels with availability in the next 72 hours
        var hotelsWithAvailability = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .Include(rt => rt.Availabilities)
            .Where(rt => rt.Availabilities.Any(a => 
                a.Date >= today && 
                a.Date <= threeDaysFromNow && 
                a.AvailableCount > 0 &&
                !a.IsBlocked))
            .Select(rt => rt.Hotel)
            .Distinct()
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.City,
                h.Country,
                h.Location,
                h.ImageUrl,
                h.Rating,
                OriginalPrice = h.PricePerNight,
                DiscountedPrice = h.PricePerNight * 0.70m, // 30% off for last minute
                DiscountPercentage = 30,
                h.Description,
                SaveAmount = h.PricePerNight * 0.30m,
                HoursLeft = (int)(threeDaysFromNow - DateTime.UtcNow).TotalHours
            })
            .OrderBy(h => h.HoursLeft)
            .Take(8)
            .ToListAsync();

        return Ok(hotelsWithAvailability);
    }

    // GET: api/deals/seasonal
    [HttpGet("seasonal")]
    public async Task<ActionResult<IEnumerable<object>>> GetSeasonalDeals()
    {
        var currentMonth = DateTime.UtcNow.Month;
        decimal discountPercentage;
        string season;

        // Determine season and discount
        if (currentMonth >= 6 && currentMonth <= 8)
        {
            season = "Summer";
            discountPercentage = 0.20m; // 20% summer discount
        }
        else if (currentMonth >= 12 || currentMonth <= 2)
        {
            season = "Winter";
            discountPercentage = 0.15m; // 15% winter discount
        }
        else if (currentMonth >= 3 && currentMonth <= 5)
        {
            season = "Spring";
            discountPercentage = 0.18m; // 18% spring discount
        }
        else
        {
            season = "Autumn";
            discountPercentage = 0.22m; // 22% autumn discount
        }

        var hotels = await _context.Hotels
            .Where(h => h.PricePerNight > 0)
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.City,
                h.Country,
                h.Location,
                h.ImageUrl,
                h.Rating,
                OriginalPrice = h.PricePerNight,
                DiscountedPrice = h.PricePerNight * (1 - discountPercentage),
                DiscountPercentage = (int)(discountPercentage * 100),
                h.Description,
                SaveAmount = h.PricePerNight * discountPercentage,
                Season = season
            })
            .OrderByDescending(h => h.Rating)
            .Take(10)
            .ToListAsync();

        return Ok(hotels);
    }

    // GET: api/deals/packages
    [HttpGet("packages")]
    public async Task<ActionResult<IEnumerable<object>>> GetPackageDeals()
    {
        var hotels = await _context.Hotels
            .Where(h => h.PricePerNight > 0)
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.City,
                h.Country,
                h.Location,
                h.ImageUrl,
                h.Rating,
                h.Description,
                PricePerNight = h.PricePerNight,
                // 7 nights package with 15% discount
                PackageNights = 7,
                OriginalPackagePrice = h.PricePerNight * 7,
                DiscountedPackagePrice = h.PricePerNight * 7 * 0.85m,
                DiscountPercentage = 15,
                SaveAmount = h.PricePerNight * 7 * 0.15m,
                PricePerNightWithDiscount = h.PricePerNight * 0.85m
            })
            .OrderByDescending(h => h.Rating)
            .Take(10)
            .ToListAsync();

        return Ok(hotels);
    }
}
