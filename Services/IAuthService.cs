using HotelManagement.DTOs.Auth;

namespace HotelManagement.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> RegisterAsync(RegisterDto dto);
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<UserProfileResponseDto> GetProfileAsync(int userId);
        Task<UserProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<UserProfileResponseDto> UpdateMyProfileAsync(int userId, UpdateMyProfileDto dto);
    }
}
