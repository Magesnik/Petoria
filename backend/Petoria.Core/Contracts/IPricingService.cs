using Petoria.Core.DTOs.Reservation;

namespace Petoria.Core.Contracts;

public interface IPricingService
{
    Task<PriceCalculationResponseDto> CalculatePriceAsync(
        int roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate,
        int numberOfRooms,
        string? userId = null);
}
