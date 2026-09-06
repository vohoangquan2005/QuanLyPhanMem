using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.BookingDetail
{
    public class BookingDetailUpdateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã đặt phòng!")]
        public int BookingId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã phòng!")]
        public int RoomId { get; set; }
        public DateTime? ActualCheckIn { get; set; } // Thời gian nhận phòng thực tế
        public DateTime? ActualCheckOut { get; set; }// Thời gian trả phòng thực tế
        [Required(ErrorMessage = "Vui lòng nhập tiền phòng 1 đêm!")]
        public decimal PricePerNight { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng khách ở phòng!")]
        public int GuestCount { get; set; }         // Số khách ở
    }
}
