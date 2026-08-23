using HotelManagement.Enums;

namespace HotelManagement.DTOs.ServicePackage
{
    public class ServicePackageResponseDto
    {
        public int ServicePackageId { get; set; }
        public string? PackageName { get; set; }
        public decimal PackagePrice { get; set; }
        public string? Description { get; set; }
        public ActiveEnums IsDeleted { get; set; } // Dùng để xóa tạm
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
