using Petoria.Core.DTOs.Reservation;

namespace Petoria.Core.Contracts;

/// <summary>
/// Интерфейс за изчисление на цени на резервации.
/// Отчита отстъпки, last-minute оферти и безплатен престой за модератори/собственици.
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Изчислява крайната цена за резервация, включително отстъпки по дати.
    /// Ако потребителят е модератор/собственик на хотела, цената е 0.
    /// </summary>
    Task<PriceCalculationResponseDto> CalculatePriceAsync(
        int roomTypeId,
        DateTime checkInDate,
        DateTime checkOutDate,
        int numberOfRooms,
        string? userId = null);
}
