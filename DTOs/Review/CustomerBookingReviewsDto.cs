namespace HotelManagement.DTOs.Review
{
    public class CustomerBookingReviewsDto
    {
        public int BookingId { get; set; }
        public bool CanReview { get; set; }
        public List<ReviewResponseDto> Reviews { get; set; } = new();
    }
}
