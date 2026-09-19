namespace HotelManagement.DTOs.BookingService
{
    public class BookingServiceResponseDto
    {
        public int BookingServiceId { get; set; }
        public int BookingId { get; set; }
        // Thông tin dịch vụ
        public int ServiceId { get; set; }
        public string?  ServiceName { get; set; }
        public int Quantity { get; set; }           // Số lượng
        public decimal UnitPrice { get; set; }      // Đơn giá
        public decimal TotalAmount { get; set; }    // Thành tiền
        public DateTime? CreateAt { get; set; }
    }
}
