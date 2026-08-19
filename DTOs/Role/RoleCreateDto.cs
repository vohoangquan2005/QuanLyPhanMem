using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Role
{
    public class RoleCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên vai trò!")]
        [StringLength(50, ErrorMessage = "Tên vai trò không vượt quá 50 ký tự!")]
        public string? RoleName { get; set; }
        [StringLength(255)]
        public string? Description { get; set; }
    }
}
