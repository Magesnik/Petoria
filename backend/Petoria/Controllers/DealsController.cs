using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petoria.Core.DTOs.Deals;
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
    public async Task<ActionResult<IEnumerable<DiscountedHotelResponseDto>>> GetDiscountedHotels()
    {
        var today = DateTime.UtcNow;

        // Get hotels with active discounts
        var hotelsWithActiveDiscounts = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .Include(rt => rt.Discounts)
            .Where(rt => rt.Discounts.Any(d => 
                d.StartDate <= today && 
                d.EndDate >= today))
            .Select(rt => new
            {
                HotelId = rt.Hotel!.Id,
                HotelName = rt.Hotel.Name,
                HotelCity = rt.Hotel.City,
                HotelCountry = rt.Hotel.Country,
                HotelLocation = rt.Hotel.Location,
                HotelImageUrl = rt.Hotel.ImageUrl,
                HotelRating = rt.Hotel.Rating,
                HotelDescription = rt.Hotel.Description,
                RoomPrice = rt.PricePerNight,
                MaxDiscount = rt.Discounts
                    .Where(d => d.StartDate <= today && d.EndDate >= today)
                    .OrderByDescending(d => d.DiscountPercentage)
                    .Select(d => d.DiscountPercentage)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var result = hotelsWithActiveDiscounts
            .GroupBy(x => x.HotelId)
            .Select(g => new DiscountedHotelResponseDto
            {
                Id = g.Key,
                Name = g.First().HotelName,
                City = g.First().HotelCity,
                Country = g.First().HotelCountry,
                Location = g.First().HotelLocation,
                ImageUrl = g.First().HotelImageUrl,
                Rating = g.First().HotelRating,
                Description = g.First().HotelDescription,
                MinPrice = g.Min(x => x.RoomPrice),
                MaxDiscount = g.Max(x => x.MaxDiscount),
                OriginalPrice = g.Min(x => x.RoomPrice),
                DiscountedPrice = g.Min(x => x.RoomPrice) * (1 - g.Max(x => x.MaxDiscount) / 100m),
                DiscountPercentage = g.Max(x => x.MaxDiscount),
                SaveAmount = g.Min(x => x.RoomPrice) * (g.Max(x => x.MaxDiscount) / 100m)
            })
            .OrderByDescending(h => h.Rating)
            .Take(12);

        return Ok(result);
    }

    // GET: api/deals/last-minute
    [HttpGet("last-minute")]
    public async Task<ActionResult<IEnumerable<LastMinuteOfferResponseDto>>> GetLastMinuteDeals()
    {
        var today = DateTime.UtcNow.Date;
        var sevenDaysFromNow = today.AddDays(7);
        
        // Get room types with low availability (≤3 rooms) in next 7 days
        var roomTypesWithLowAvailability = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .Include(rt => rt.Availabilities)
            .Include(rt => rt.Discounts)
            .Where(rt => rt.Availabilities.Any(a => 
                a.Date >= today && 
                a.Date <= sevenDaysFromNow && 
                a.AvailableCount > 0 && 
                a.AvailableCount <= 3 &&
                !a.IsBlocked))
            .ToListAsync();

        var result = new List<LastMinuteOfferResponseDto>();
        
        foreach (var roomType in roomTypesWithLowAvailability)
        {
            // Get earliest available date
            var earliestDate = roomType.Availabilities
                .Where(a => a.Date >= today && a.AvailableCount > 0 && !a.IsBlocked)
                .OrderBy(a => a.Date)
                .FirstOrDefault()?.Date ?? today;

            // Check for existing active discount
            var activeDiscount = roomType.Discounts
                .FirstOrDefault(d => d.StartDate <= earliestDate && d.EndDate >= earliestDate);

            int discountPercentage;
            decimal discountedPrice;
            decimal saveAmount;

            if (activeDiscount != null)
            {
                // Use existing discount
                discountPercentage = activeDiscount.DiscountPercentage;
                discountedPrice = roomType.PricePerNight * (1 - discountPercentage / 100m);
                saveAmount = roomType.PricePerNight * (discountPercentage / 100m);
            }
            else
            {
                // Apply 5% last-minute discount (for first 2 nights)
                discountPercentage = 5;
                discountedPrice = roomType.PricePerNight * 0.95m;
                saveAmount = roomType.PricePerNight * 0.05m;
            }

            // Get minimum available rooms count in the period
            var minAvailable = roomType.Availabilities
                .Where(a => a.Date >= today && a.Date <= sevenDaysFromNow && !a.IsBlocked)
                .Min(a => (int?)a.AvailableCount) ?? 0;

            result.Add(new LastMinuteOfferResponseDto
            {
                HotelId = roomType.HotelId,
                HotelName = roomType.Hotel?.Name ?? "",
                HotelCity = roomType.Hotel?.City ?? "",
                HotelCountry = roomType.Hotel?.Country ?? "",
                HotelImageUrl = roomType.Hotel?.ImageUrl ?? "",
                HotelRating = roomType.Hotel?.Rating ?? 0,
                
                RoomTypeId = roomType.Id,
                RoomTypeName = roomType.Name,
                Capacity = roomType.Capacity,
                Description = roomType.Description ?? "",
                
                OriginalPrice = roomType.PricePerNight,
                DiscountPercentage = discountPercentage,
                DiscountedPrice = discountedPrice,
                SaveAmount = saveAmount,
                
                AvailableRoomsCount = minAvailable,
                EarliestAvailableDate = earliestDate,
                DaysUntilCheckIn = (int)(earliestDate - today).TotalDays
            });
        }

        // Sort by highest discount first, then by days until check-in
        return Ok(result
            .OrderByDescending(r => r.DiscountPercentage)
            .ThenBy(r => r.DaysUntilCheckIn)
            .ToList());
    }

    // GET: api/deals/seasonal
    [HttpGet("seasonal")]
    public async Task<ActionResult<IEnumerable<SeasonalDealResponseDto>>> GetSeasonalDeals()
    {
        var today = DateTime.UtcNow;
        var currentMonth = today.Month;
        string season;

        // Determine current season for display only
        if (currentMonth >= 6 && currentMonth <= 8)
        {
            season = "Summer";
        }
        else if (currentMonth >= 12 || currentMonth <= 2)
        {
            season = "Winter";
        }
        else if (currentMonth >= 3 && currentMonth <= 5)
        {
            season = "Spring";
        }
        else
        {
            season = "Autumn";
        }

        // Get hotels with REAL active discounts created by admin
        var hotelsWithActiveDiscounts = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .Include(rt => rt.Discounts)
            .Where(rt => rt.Discounts.Any(d => 
                d.StartDate <= today && 
                d.EndDate >= today))
            .Select(rt => new
            {
                HotelId = rt.Hotel!.Id,
                HotelName = rt.Hotel.Name,
                HotelCity = rt.Hotel.City,
                HotelCountry = rt.Hotel.Country,
                HotelLocation = rt.Hotel.Location,
                HotelImageUrl = rt.Hotel.ImageUrl,
                HotelRating = rt.Hotel.Rating,
                HotelDescription = rt.Hotel.Description,
                RoomPrice = rt.PricePerNight,
                MaxDiscount = rt.Discounts
                    .Where(d => d.StartDate <= today && d.EndDate >= today)
                    .OrderByDescending(d => d.DiscountPercentage)
                    .Select(d => d.DiscountPercentage)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var result = hotelsWithActiveDiscounts
            .GroupBy(x => x.HotelId)
            .Select(g => new SeasonalDealResponseDto
            {
                Id = g.Key,
                Name = g.First().HotelName,
                City = g.First().HotelCity,
                Country = g.First().HotelCountry,
                Location = g.First().HotelLocation,
                ImageUrl = g.First().HotelImageUrl,
                Rating = g.First().HotelRating,
                Description = g.First().HotelDescription,
                MinPrice = g.Min(x => x.RoomPrice),
                MaxDiscount = g.Max(x => x.MaxDiscount),
                OriginalPrice = g.Min(x => x.RoomPrice),
                DiscountedPrice = g.Min(x => x.RoomPrice) * (1 - g.Max(x => x.MaxDiscount) / 100m),
                DiscountPercentage = g.Max(x => x.MaxDiscount),
                SaveAmount = g.Min(x => x.RoomPrice) * (g.Max(x => x.MaxDiscount) / 100m),
                Season = season
            })
            .OrderByDescending(h => h.Rating)
            .Take(12);

        return Ok(result);
    }

    // GET: api/deals/packages
    [HttpGet("packages")]
    public async Task<ActionResult<IEnumerable<PackageDealResponseDto>>> GetPackageDeals()
    {
        var hotels = await _context.Hotels
            .Where(h => h.PricePerNight > 0)
            .Select(h => new PackageDealResponseDto
            {
                Id = h.Id,
                Name = h.Name,
                City = h.City,
                Country = h.Country,
                Location = h.Location,
                ImageUrl = h.ImageUrl,
                Rating = h.Rating,
                Description = h.Description,
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
