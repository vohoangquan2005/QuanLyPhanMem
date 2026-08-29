using HotelManagement.DTOs.Auth;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
        // GET PROFILE
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null)
                return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();
            var profile = await _authService.GetProfileAsync(userId);
            return Ok(profile);
        }
        [Authorize]
        [HttpPut("profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateMyProfileDto dto)
        {
            var userId = GetAuthenticatedUserId();
            if (userId is null) return Unauthorized();

            var profile = await _authService.UpdateMyProfileAsync(userId.Value, dto);
            return Ok(profile);
        }
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetAuthenticatedUserId();
            if (userId is null) return Unauthorized();

            await _authService.ChangePasswordAsync(userId.Value, dto);
            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        private int? GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }
        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        // LOGOUT
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                message = "Đăng xuất thành công!"
            });
        }
    }
}
