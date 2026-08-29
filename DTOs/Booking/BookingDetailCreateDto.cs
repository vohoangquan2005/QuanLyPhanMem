using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Booking
{
    public class BookingDetailCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Mã phòng không hợp lệ!")]
        public int RoomId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng khách phải lớn hơn 0!")]
        public int GuestCount { get; set; }
    }
}
