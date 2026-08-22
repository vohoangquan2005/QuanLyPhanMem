using HotelManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.User
{
    public class UserUpdateDto
    {
        [StringLength(50, ErrorMessage = "Tên người dùng không vượt quá 50 ký tự!")]
        [Required(ErrorMessage = "Vui lòng nhập tên người dùng!")]
        public string? FullName { get; set; }
        [EmailAddress]
        [StringLength(50, ErrorMessage = "Email không vượt quá 50 ký tự!")]
        [Required(ErrorMessage = "Vui lòng nhập email!")]
        public string? Email { get; set; }
        public string? HashPassword { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Mã vai trò không hợp lệ!")]
        public int RoleId { get; set; }
        public ActiveEnums IsLocked { get; set; }         // Khóa tài khoản: 0<hoạt động> 1 <khóa>
        public string? Gender { get; set; }     // Giới tính
        public DateTime? DateOfBirth { get; set; } // Ngày sinh
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        public string? PhoneNumber { get; set; }      // Số điện thoại
    }
}
