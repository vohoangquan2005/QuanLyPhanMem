using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Cancellation
{
    public class CancellationCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã đặt phòng!")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã đặt phòng không hợp lệ!")]
        public int BookingId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập lý do hủy!")]
        [StringLength(500, ErrorMessage = "Lý do hủy không vượt quá 500 ký tự!")]
        public string? Reason { get; set; }
    }
}
