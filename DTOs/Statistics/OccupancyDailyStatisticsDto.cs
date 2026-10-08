namespace HotelManagement.DTOs.Statistics
{
    public class OccupancyDailyStatisticsDto
    {
        public DateTime Date { get; set; }       
        public int TotalRooms { get; set; }      // Tổng số phòng     
        public int OccupiedRooms { get; set; }  // Số phòng được sử dụng trong ngày       
        public decimal OccupancyRate { get; set; }  // Tỷ lệ lấp đầy (%)
    }
}
