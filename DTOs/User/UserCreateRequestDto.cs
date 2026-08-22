namespace HotelManagement.DTOs.User
{
    public class UserCreateRequestDto
    {
        public UserCreateDto User { get; set; } = new();
        public CustomerCreateDto? Customer { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
