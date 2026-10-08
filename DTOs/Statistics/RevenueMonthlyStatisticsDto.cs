namespace HotelManagement.DTOs.Statistics
{
    public class RevenueMonthlyStatisticsDto
    {
        public DateTime Month { get; set; }       
        public decimal Revenue { get; set; }    // Tổng số tiền đã thanh toán trong tháng
        public int PaymentCount { get; set; }   // Số giao dịch thanh toán
    }
}
