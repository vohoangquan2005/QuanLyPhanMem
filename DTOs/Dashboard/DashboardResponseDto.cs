namespace HotelManagement.DTOs.Dashboard
{
    public class DashboardResponseDto
    {
        // ROOM
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        // BOOKING
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CheckedInBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        // CUSTOMER
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        // CHECK IN / CHECK OUT
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }
        // SERVICE
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }
        public int ServiceUsagesThisMonth { get; set; }
        // PROMOTION
        public int ActivePromotions { get; set; }
        public int ExpiringPromotions { get; set; }
        public int ExpiredPromotions { get; set; }
    }
}
