using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Service
{
    public class ServiceUpdateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ!")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không vượt quá 100 ký tự!")]
        public string? ServiceName { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá dịch vụ phải lớn hơn 0!")]
        public decimal UnitPrice { get; set; }      // Đơn giá
        [Required(ErrorMessage = "Vui lòng nhập đơn vị tính!")]
        public string? Unit { get; set; }   // Đơn vị tính (Theo giờ, theo ngày, theo lần)
        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự!")]
        public string? Description { get; set; }
    }
}
