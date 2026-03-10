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
        var thirtyDaysFromNow = today.AddDays(30);

        // Get hotels with active discounts within the next 30 days
        var hotelsWithActiveDiscounts = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .Include(rt => rt.Discounts)
            .Where(rt => rt.Hotel != null &&
                !rt.Hotel.IsSuspendedBySuperAdmin &&
                rt.Hotel.IsAvailable &&
                rt.Discounts.Any(d =>
                d.EndDate >= today &&
                d.StartDate <= thirtyDaysFromNow))
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
                    .Where(d => d.EndDate >= today && d.StartDate <= thirtyDaysFromNow)
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
        var twoDaysFromNow = today.AddDays(1); // today + tomorrow only

        // Get room types with available rooms in the next 2 days (last-minute)
        var roomTypesWithLowAvailability = await _context.RoomTypes
            .Include(rt => rt.Hotel)
            .Include(rt => rt.Availabilities)
            .Include(rt => rt.Discounts)
            .Where(rt => rt.Hotel != null &&
                !rt.Hotel.IsSuspendedBySuperAdmin &&
                rt.Hotel.IsAvailable &&
                rt.Availabilities.Any(a =>
                a.Date >= today &&
                a.Date <= twoDaysFromNow &&
                a.AvailableCount > 0 &&
                !a.IsBlocked))
            .ToListAsync();

        var result = new List<LastMinuteOfferResponseDto>();
        
        foreach (var roomType in roomTypesWithLowAvailability)
        {
            // Get earliest available date (only within the 2-day window)
            var earliestDate = roomType.Availabilities
                .Where(a => a.Date >= today && a.Date <= twoDaysFromNow && a.AvailableCount > 0 && !a.IsBlocked)
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

            // Get available rooms count for the earliest available date
            var minAvailable = roomType.Availabilities
                .Where(a => a.Date == earliestDate && !a.IsBlocked)
                .Sum(a => a.AvailableCount);

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

        // Sort by days until check-in (today first), then by discount
        return Ok(result
            .OrderBy(r => r.DaysUntilCheckIn)
            .ThenByDescending(r => r.DiscountPercentage)
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
            .Where(rt => rt.Hotel != null &&
                !rt.Hotel.IsSuspendedBySuperAdmin &&
                rt.Hotel.IsAvailable &&
                rt.Discounts.Any(d =>
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
        // Get hotels with at least one room type
        var hotelsWithRooms = await _context.Hotels
            .Select(h => new 
            {
                Hotel = h,
                MinPrice = _context.RoomTypes
                    .Where(rt => rt.HotelId == h.Id)
                    .OrderBy(rt => rt.PricePerNight)
                    .Select(rt => (decimal?)rt.PricePerNight)
                    .FirstOrDefault() ?? 0
            })
            .Where(x => x.MinPrice > 0)
            .OrderByDescending(x => x.Hotel.Rating)
            .Take(10)
            .ToListAsync();

        var result = hotelsWithRooms.Select(x => new PackageDealResponseDto
        {
            Id = x.Hotel.Id,
            Name = x.Hotel.Name,
            City = x.Hotel.City,
            Country = x.Hotel.Country,
            Location = x.Hotel.Location,
            ImageUrl = x.Hotel.ImageUrl,
            Rating = x.Hotel.Rating,
            Description = x.Hotel.Description,
            PricePerNight = x.MinPrice,
            // 7 nights package with 15% discount
            PackageNights = 7,
            OriginalPackagePrice = x.MinPrice * 7,
            DiscountedPackagePrice = x.MinPrice * 7 * 0.85m,
            DiscountPercentage = 15,
            SaveAmount = x.MinPrice * 7 * 0.15m,
            PricePerNightWithDiscount = x.MinPrice * 0.85m
        });

        return Ok(result);
    }
}
