using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.ServicePackage
{
    public class ServicePackageCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên gói dịch vụ!")]
        [StringLength(100, ErrorMessage = "Tên gói dịch vụ không vượt quá 100 ký tự!")]
        public string? PackageName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập giá tiền gói dịch vụ!")]
        public decimal PackagePrice { get; set; }
        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự!")]
        public string? Description { get; set; }
    }
}
