namespace HotelManagement.DTOs.Auth
{
    public class UserProfileResponseDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Avatar { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public int? CustomerId { get; set; }
        public string? CCCD { get; set; }
        public string? Nationality { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
