using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.BookingService
{
    public class BookingServiceCreateOnlineDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã dịch vụ!")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã  dịch vụ không hợp lệ!")]
        public int ServiceId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng!")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0!")]
        public int Quantity { get; set; }           // Số lượng
    }
}
