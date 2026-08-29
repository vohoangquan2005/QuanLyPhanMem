using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên!")]
        [StringLength(50, ErrorMessage = "Họ tên không được vượt quá 50 ký tự!")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự!")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu!")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp!")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0)!")]
        public string PhoneNumber { get; set; } = string.Empty;

        [RegularExpression(@"^\d{9}(\d{3})?$", ErrorMessage = "CCCD/CMND phải gồm 9 hoặc 12 chữ số!")]
        public string? CCCD { get; set; }

        [StringLength(50, ErrorMessage = "Quốc tịch không được vượt quá 50 ký tự!")]
        public string? Nationality { get; set; } = "Việt Nam";
    }
}
