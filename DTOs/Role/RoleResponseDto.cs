using HotelManagement.Enums;

namespace HotelManagement.DTOs.Role
{
    public class RoleResponseDto
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public ActiveEnums IsDeleted { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
