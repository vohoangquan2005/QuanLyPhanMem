using HotelManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Invoice
{
    public class InvoiceCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã đặt phòng!")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã đặt phòng không hợp lệ!")]
        public int BookingId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Mã người tạo hóa đơn không hợp lệ!")]
        public int? CreatedByUserId { get; set; }// Người tạo hóa đơn (Nhân viên)
    }
}
