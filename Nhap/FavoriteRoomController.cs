using HotelManagement.DTOs.FavoriteRoom;
using HotelManagement.Models;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [Authorize(Roles = "Customer")]
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteRoomController : ControllerBase
    {
        private readonly FavoriteRoomService _favoriteRoomService;
        public FavoriteRoomController(FavoriteRoomService favoriteRoomService)
        {
            _favoriteRoomService = favoriteRoomService;
        }
        [HttpGet]
        public async Task<IActionResult> GetMyFavorites()
        {
            var customerIdClaim = User.FindFirst("CustomerId");
            if (customerIdClaim is null)
                return Unauthorized("Không phải tài khoản khách hàng!");
            if (!int.TryParse(customerIdClaim.Value, out var customerId))
                return Unauthorized("Mã khách hàng không hợp lệ!");
            var favoriteRooms = await _favoriteRoomService.GetMyFavoritesAsync(customerId);
            return Ok(favoriteRooms);
        }
        [HttpPost]
        public async Task<IActionResult> Create(FavoriteRoomCreateDto dto)
        {
            var customerIdClaim = User.FindFirst("CustomerId");
            if (customerIdClaim is null)
                return Unauthorized("Không phải tài khoản khách hàng!");
            if (!int.TryParse(customerIdClaim.Value, out var customerId))
                return Unauthorized("Mã khách hàng không hợp lệ!");
            var favoriteRoom = await _favoriteRoomService.CreateAsync(dto, customerId);
            return Ok(favoriteRoom);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customerIdClaim = User.FindFirst("CustomerId");
            if (customerIdClaim is null)
                return Unauthorized("Không phải tài khoản khách hàng!");
            if (!int.TryParse(customerIdClaim.Value, out var customerId))
                return Unauthorized("Mã khách hàng không hợp lệ!");
            var result = await _favoriteRoomService.DeleteAsync(id, customerId);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
