using System;

namespace Petoria.Core.Models.Email
{
    public class ConfirmationEmailItem
    {
        public string HotelName { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfRooms { get; set; }
    }
}
