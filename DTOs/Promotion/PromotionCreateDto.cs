using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Promotion
{
    public class PromotionCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã khuyến mãi!")]
        [StringLength(50, ErrorMessage = "Mã khuyến mãi không vượt quá 50 ký tự!")]
        public string? PromotionCode { get; set; }  // Mã khuyến mãi
        [Required(ErrorMessage = "Vui lòng nhập tên khuyến mãi!")]
        [StringLength(100, ErrorMessage = "Tên khuyến mãi không vượt quá 100 ký tự!")]
        public string? PromotionName { get; set; }
        public HotelManagement.Enums.PromotionDiscountTypeEnums DiscountType { get; set; } = HotelManagement.Enums.PromotionDiscountTypeEnums.Percentage;
        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm giá!")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị khuyến mãi không được âm!")]
        public decimal? DiscountValue { get; set; } = 0;
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền tối thiểu không được âm!")]
        public decimal? MinAmount { get; set; } = 0;// Giá tiền tối thiểu mà khách hàng phải sử dụng để có thể áp dụng khuyến mãi
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm tối đa không được âm!")]
        public decimal? MaxDisCountAmount { get; set; } // Giá tiền tối đa được giảm
        [Range(0, int.MaxValue, ErrorMessage = "Giới hạn sử dụng không được âm!")]
        public int? UsageLimit { get; set; }    // Giới hạn số lượng áp dụng khuyến mãi
        [Required(ErrorMessage = "Vui lòng nhập thời gian bắt đầu áp dụng khuyến mãi!")]
        public DateTime StartDate { get; set; } // Ngày bắt đầu áp dụng
        [Required(ErrorMessage = "Vui lòng nhập thời gian bắt đầu áp dụng khuyến mãi!")]
        public DateTime EndDate { get; set; }   // Ngày kết thúc 
    }
}
