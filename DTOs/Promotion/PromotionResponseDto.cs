using HotelManagement.Enums;

namespace HotelManagement.DTOs.Promotion
{
    public class PromotionResponseDto
    {
        public int PromotionId { get; set; }
        public string? PromotionCode { get; set; }  // Mã khuyến mãi
        public string? PromotionName { get; set; }
        public HotelManagement.Enums.PromotionDiscountTypeEnums DiscountType { get; set; }
        public decimal? DiscountValue { get; set; } = 0;
        public decimal? MinAmount { get; set; } = 0;// Giá tiền tối thiểu mà khách hàng phải sử dụng để có thể áp dụng khuyến mãi
        public decimal? MaxDisCountAmount { get; set; } // Giá tiền tối đa được giảm
        public int? UsageLimit { get; set; }    // Giới hạn số  lượng áp dụng khuyến mãi
        public int? UsedCount { get; set; } // Số lượng đã sử dụng
        public DateTime StartDate { get; set; } // Ngày bắt đầu áp dụng
        public DateTime EndDate { get; set; }   // Ngày kết thúc 
        public ActiveEnums IsDeleted { get; set; } // Dùng để xóa tạm
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
