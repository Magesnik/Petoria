using Microsoft.EntityFrameworkCore;
using Petoria.Core.Contracts;
using Petoria.Core.DTOs.Reservation;
using Petoria.Infrastructure.Data;

namespace Petoria.Core.Services;

/// <summary>
/// Услуга за изчисление на цени на резервации.
/// Отчита отстъпки по дати, last-minute оферти (5%) и безплатен престой за модератори/собственици.
/// </summary>
public class PricingService : IPricingService
{
    private readonly ApplicationDbContext _context;

    public PricingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PriceCalculationResponseDto> CalculatePriceAsync(
        int roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate,
        int numberOfRooms,
        string? userId = null)
    {
        var roomType = await _context.RoomTypes.FindAsync(roomTypeId)
            ?? throw new ArgumentException("Room type not found", nameof(roomTypeId));

        var numberOfNights = (int)(checkOutDate.Date - checkInDate.Date).TotalDays;

        var discounts = await _context.RoomDiscounts
            .Where(d => d.RoomTypeId == roomTypeId &&
                        d.EndDate >= checkInDate.Date &&
                        d.StartDate <= checkOutDate.Date)
            .ToListAsync();

        // Проверка за last-minute резервация (настаняване днес или утре)
        var today = DateTime.UtcNow.Date;
        var isLastMinute = checkInDate.Date >= today && checkInDate.Date <= today.AddDays(1);
        // Проверка дали има свободни стаи на датата на настаняване
        if (isLastMinute)
        {
            var checkInAvailability = await _context.RoomAvailabilities
                .FirstOrDefaultAsync(a => a.RoomTypeId == roomTypeId && a.Date == checkInDate.Date);
            // Ако няма запис в БД, по подразбиране се използва TotalRooms
            var availableCount = checkInAvailability?.AvailableCount ?? roomType.TotalRooms;
            var isBlocked = checkInAvailability?.IsBlocked ?? false;
            isLastMinute = !isBlocked && availableCount >= numberOfRooms;
        }

        var breakdown = new List<DayPriceBreakdownDto>();
        decimal totalPrice = 0;
        decimal originalTotal = 0;

        for (var date = checkInDate.Date; date < checkOutDate.Date; date = date.AddDays(1))
        {
            var discount = discounts
                .Where(d => d.StartDate.Date <= date && d.EndDate.Date >= date)
                .OrderByDescending(d => d.DiscountPercentage)
                .FirstOrDefault();

            var dayOriginal = roomType.PricePerNight;
            decimal dayFinal;
            int? effectiveDiscountPct;

            if (discount != null)
            {
                effectiveDiscountPct = discount.DiscountPercentage;
                dayFinal = dayOriginal * (1 - discount.DiscountPercentage / 100m);
            }
            else if (isLastMinute)
            {
                // 5% last-minute отстъпка при настаняване днес/утре без друга отстъпка
                effectiveDiscountPct = 5;
                dayFinal = dayOriginal * 0.95m;
            }
            else
            {
                effectiveDiscountPct = null;
                dayFinal = dayOriginal;
            }

            breakdown.Add(new DayPriceBreakdownDto
            {
                Date = date,
                OriginalPrice = dayOriginal,
                DiscountPercentage = effectiveDiscountPct,
                FinalPrice = dayFinal
            });

            originalTotal += dayOriginal;
            totalPrice += dayFinal;
        }

        totalPrice *= numberOfRooms;
        originalTotal *= numberOfRooms;

        if (!string.IsNullOrEmpty(userId))
        {
            var isModerator = await _context.HotelModerators
                .AnyAsync(hm => hm.HotelId == roomType.HotelId && hm.UserId == userId);

            var isOwner = await _context.Hotels
                .AnyAsync(h => h.Id == roomType.HotelId && h.CreatedById == userId);

            if (isModerator || isOwner)
                totalPrice = 0;
        }

        return new PriceCalculationResponseDto
        {
            NumberOfNights = numberOfNights,
            PricePerNight = roomType.PricePerNight,
            NumberOfRooms = numberOfRooms,
            TotalPrice = totalPrice,
            OriginalPrice = originalTotal,
            TotalDiscount = originalTotal - totalPrice,
            Breakdown = breakdown
        };
    }
}
