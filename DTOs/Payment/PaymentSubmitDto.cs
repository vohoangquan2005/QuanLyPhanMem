using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Payment
{
    public class PaymentSubmitDto
    {
        [Required, StringLength(30)]
        public string PaymentMethod { get; set; } = string.Empty;
        [StringLength(100)]
        public string? TransactionCode { get; set; }
    }
}
