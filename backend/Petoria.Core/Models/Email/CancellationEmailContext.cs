using System;

namespace Petoria.Core.Models.Email
{
    public class CancellationEmailContext
    {
        public string UserLanguage { get; set; } = "bg";
        public string HotelName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal RefundPercentage { get; set; }
        public int DaysBeforeCheckIn { get; set; }
    }
}
