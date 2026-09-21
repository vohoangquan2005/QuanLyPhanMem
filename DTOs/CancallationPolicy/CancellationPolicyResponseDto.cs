using HotelManagement.Enums;

namespace HotelManagement.DTOs.CancallationPolicy
{
    public class CancellationPolicyResponseDto
    {
        public int CancellationPolicyId { get; set; }
        public string? PolicyName { get; set; }
        public int CancellationTime { get; set; } = 24;    // Số giờ được hủy sau khi đặt phòng
        public CancellationTimeUnitEnums CancellationTimeUnit { get; set; }
        public decimal RefundPercentage { get; set; }   // Phần trăm hoàn trả
        public string? Description { get; set; }
        public ActiveEnums IsDeleted { get; set; } // Dùng để xóa tạm
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
