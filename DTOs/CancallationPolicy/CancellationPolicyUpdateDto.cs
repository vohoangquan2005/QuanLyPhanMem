using HotelManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.CancallationPolicy
{
    public class CancellationPolicyUpdateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên chính sách!")]
        [StringLength(100, ErrorMessage = "Tên chính sách phải ít hơn 100 ký tự!")]
        public string? PolicyName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập thời gian được áp dụng chính sách!")]
        [Range(0, 100, ErrorMessage = "Thời gian được áp dụng chính sách phải lớn hơn 0!")]
        public int CancellationTime { get; set; }    // Thời gian được áp dụng chinh sách
        public CancellationTimeUnitEnums CancellationTimeUnit { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập phần trăm (%) hoàn trả!")]
        public decimal RefundPercentage { get; set; }   // Phần trăm hoàn trả
        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự!")]
        public string? Description { get; set; }
    }
}
