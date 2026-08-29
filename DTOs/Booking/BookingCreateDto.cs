using HotelManagement.DTOs.BookingDetail;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Booking
{
    public class BookingCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Mã khách hàng không hợp lệ!")]
        public int CustomerId { get; set; }
        public DateTime ExpectedCheckIn { get; set; }
        public DateTime ExpectedCheckOut { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng khách phải lớn hơn 0!")]
        public int GuestCount { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phòng phải lớn hơn 0!")]
        public int RoomCount { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Mã khuyến mãi không hợp lệ!")]
        public int? PromotionId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Mã gói dịch vụ không hợp lệ!")]
        public int? ServicePackageId { get; set; }
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự!")]
        public string? Note { get; set; }
        public List<BookingDetailCreateDto> BookingDetails { get; set; } = new();
    }
}
