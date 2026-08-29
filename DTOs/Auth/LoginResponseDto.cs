namespace HotelManagement.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiredAt { get; set; }
        public int UserId { get; set; }
        public int? CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RoleName { get; set; }
        public string? Avatar { get; set; }
    }
}
