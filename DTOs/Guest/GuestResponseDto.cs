namespace HotelManagement.DTOs.Guest
{
    public class GuestResponseDto
    {
        public int GuestId { get; set; }
        public int BookingDetailId { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; } = "Việt Nam";
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Note { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
