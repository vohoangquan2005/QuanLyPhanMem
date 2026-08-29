using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Vui lòng nhập email!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string Password { get; set; } = string.Empty;
    }
}
