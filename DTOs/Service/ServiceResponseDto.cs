using HotelManagement.Enums;

namespace HotelManagement.DTOs.Service
{
    public class ServiceResponseDto
    {
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public decimal UnitPrice { get; set; }      // Đơn giá
        public string? Unit { get; set; }   // Đơn vị tính (Theo giờ, theo ngày, theo lần)
        public string? Description { get; set; }
        public ActiveEnums IsDeleted { get; set; } // Dùng để xóa tạm
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
