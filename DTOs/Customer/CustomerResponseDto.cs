namespace HotelManagement.DTOs.Customer
{
    public class CustomerResponseDto
    {
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public string? CCCD { get; set; }
        public string? Nationality { get; set; } = "Việt Nam";  // Quốc tịch
        // Thông tin Customer từ User
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }     // Giới tính
        public DateTime? DateOfBirth { get; set; } // Ngày sinh
        public string? PhoneNumber { get; set; }      // Số điện thoại
        public string? Avatar { get; set; }     // Ảnh đại diện
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
