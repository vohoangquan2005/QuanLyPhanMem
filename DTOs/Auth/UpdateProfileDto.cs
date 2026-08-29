using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Auth
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên!")]
        [StringLength(50, ErrorMessage = "Họ tên không vượt quá 50 ký tự!")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0)!")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Avatar { get; set; }

        [RegularExpression(@"^\d{9}(\d{3})?$", ErrorMessage = "CCCD/CMND phải gồm 9 hoặc 12 chữ số!")]
        public string? CCCD { get; set; }

        [StringLength(50, ErrorMessage = "Quốc tịch không được vượt quá 50 ký tự!")]
        public string? Nationality { get; set; }
    }
}
