using HotelManagement.Enums;

namespace HotelManagement.DTOs.Payment
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int? InvoiceId { get; set; }
        public int? BookingId { get; set; }
        public BookingEnums? BookingStatus { get; set; }
        public decimal Amount { get; set; }
        public PaymentTypeEnums PaymentType { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionCode { get; set; }
        public PaymentEnums Status { get; set; } // "Đang xử lý" "Thành công" "Thất bại"
        public int? CreatedByUserId { get; set; }   // Người tạo thanh toán
        public DateTime? PaymentDate { get; set; }
        public DateTime? CreateAt { get; set; }
        public string? RefundSource { get; set; }
    }
}
