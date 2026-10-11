namespace HotelManagement.DTOs.Dashboard
{
    public class DashboardBookingDto
    {
        public int BookingId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime ExpectedCheckIn { get; set; }
        public DateTime ExpectedCheckOut { get; set; }
        public int RoomCount { get; set; }
        public int GuestCount { get; set; }
        public string? Status { get; set; }
    }
}
