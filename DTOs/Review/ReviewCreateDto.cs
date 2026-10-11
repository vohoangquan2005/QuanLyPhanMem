using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Review
{
    public class ReviewCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã khách hàng!")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã khách hàng không hợp lệ!")]
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã phòng!")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã phòng không hợp lệ!")]
        public int RoomId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã đặt phòng!")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã đặt phòng không hợp lệ!")]
        public int BookingId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số sao!")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5!")]
        public int Rating { get; set; }
        [StringLength(1000, ErrorMessage = "Nội dung đánh giá tối đa 1000 ký tự!")]
        public string? Comment { get; set; }
    }
}
