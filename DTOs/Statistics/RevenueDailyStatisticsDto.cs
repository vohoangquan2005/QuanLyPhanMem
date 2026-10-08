namespace HotelManagement.DTOs.Statistics
{
    public class RevenueDailyStatisticsDto
    {
        public DateTime Date { get; set; }        
        public decimal Revenue { get; set; }    // Tổng số tiền đã thanh toán trong ngày      
        public int PaymentCount { get; set; }   // Số giao dịch thanh toán
    }
}
