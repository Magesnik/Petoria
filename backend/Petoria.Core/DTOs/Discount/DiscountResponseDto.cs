namespace Petoria.Core.DTOs.Discount;

/// <summary>Отговор: данни за отстъпка (период, процент)</summary>
public class DiscountResponseDto
{
    public int Id { get; set; }
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DiscountPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
}
