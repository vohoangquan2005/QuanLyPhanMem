namespace HotelManagement.DTOs.Statistics
{
    public class OccupancyMonthlyStatisticsDto
    {
        public DateTime Month { get; set; }      
        public int TotalRooms { get; set; }         // Tổng số phòng        
        public int OccupiedRoomNights { get; set; } // Tổng số lượt phòng được sử dụng       
        public int AvailableRoomNights { get; set; }    // Tổng số phòng có thể cung cấp trong tháng      
        public decimal OccupancyRate { get; set; }  // Tỷ lệ lấp đầy (%)
    }
}
