
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.BookingService
{
    public class BookingServiceUpdateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập số lượng!")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0!")]
        public int Quantity { get; set; }           // Số lượng
    }
}
