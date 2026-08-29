using HotelManagement.Enums;

namespace HotelManagement.DTOs.Review
{
    public class ReviewResponseDto
    {
        public int ReviewId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int RoomId { get; set; }
        public string? RoomName { get; set; }
        public int BookingId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public ReviewEnums Status { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
