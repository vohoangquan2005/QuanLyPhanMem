using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Auth
{
    public class UpdateMyProfileDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên!")]
        [StringLength(50, ErrorMessage = "Họ tên không vượt quá 50 ký tự!")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        [StringLength(100, ErrorMessage = "Email không vượt quá 100 ký tự!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0!")]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
