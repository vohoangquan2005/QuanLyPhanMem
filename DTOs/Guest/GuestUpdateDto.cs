using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Guest
{
    public class GuestUpdateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã đặt phòng!")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã đặt phòng không hợp lệ!")]
        public int BookingDetailId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng!")]
        [StringLength(100, ErrorMessage = "Tên khách hàng không vượt quá 100 ký tự!")]
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        [StringLength(50, ErrorMessage = "Quốc tịch không vượt quá 50 ký tự!")]
        public string? Nationality { get; set; } = "Việt Nam";
        [StringLength(10, ErrorMessage = "Số điện thoại không vượt quá 10 ký tự!")]
        public string? PhoneNumber { get; set; }
        [StringLength(100, ErrorMessage = "Email không vượt quá 100 ký tự!")]
        public string? Email { get; set; }
        public string? Note { get; set; }
    }
}
