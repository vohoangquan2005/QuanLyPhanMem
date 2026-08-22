using HotelManagement.Enums;

namespace HotelManagement.DTOs.User
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public int RoleId { get; set; }
        // Thông tin từ Role
        public string? RoleName { get; set; }
        public ActiveEnums IsLocked { get; set; }   // Active or Inactive
        public string? Gender { get; set; }     // Giới tính
        public DateTime? DateOfBirth { get; set; } // Ngày sinh
        public string? PhoneNumber { get; set; }      // Số điện thoại
        public string? Avatar { get; set; }     // Ảnh đại diện
        public ActiveEnums IsDeleted { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
